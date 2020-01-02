namespace WindowsService
{
    using System;
    using MassTransit;
    using MassTransit.Azure.ServiceBus.Core;
    using MassTransit.Context;
    using MassTransit.Saga;
    using Messaging.Activities;
    using Messaging.Consumers;
    using Messaging.Contracts;
    using Messaging.StateMachines;
    using Microsoft.ApplicationInsights.DependencyCollector;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    internal class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();

        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                       .UseWindowsService()
                       .ConfigureAppConfiguration(cfg =>
                       {
                           cfg.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                       })
                       .ConfigureLogging((context, builder) =>
                       {
                           builder.AddConsole();
                           builder.SetMinimumLevel(LogLevel.Trace);
                       })
                       .ConfigureServices((hostContext, services) =>
                       {
                           var provider      = services.BuildServiceProvider();
                           var loggerFactory = provider.GetService<ILoggerFactory>();

                           var hostConfig = hostContext.Configuration;
                           services.AddApplicationInsightsTelemetryWorkerService(hostConfig["ApplicationInsightsInstrumentationKey"]);
                           services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((m, o) =>
                           {
                               m.IncludeDiagnosticSourceActivities.Add("MassTransit");
                           });

                           LogContext.ConfigureCurrentLogContext(loggerFactory);
                           services.AddMassTransit(x =>
                           {
                               x.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
                               x.AddBus(context => Bus.Factory.CreateUsingAzureServiceBus(
                                                                                          configurator =>
                                                                                          {
                                                                                              var host =
                                                                                                  configurator.Host(hostConfig["AzureServiceBusConnectionString"],
                                                                                                           h => { });

                                                                                              configurator.ReceiveEndpoint("submit-order", e =>
                                                                                              {
                                                                                                  e.Consumer<SubmitOrderConsumer>();
                                                                                              });

                                                                                              configurator.ReceiveEndpoint("order-observer", e =>
                                                                                              {
                                                                                                  e.Consumer<OrderSubmittedConsumer>();
                                                                                              });

                                                                                              configurator.ReceiveEndpoint("order-state", e =>
                                                                                              {
                                                                                                  var machine = new OrderStateMachine();
                                                                                                  var repository = new InMemorySagaRepository<OrderState>();

                                                                                                  e.StateMachineSaga(machine, repository);

                                                                                                  EndpointConvention.Map<OrderProcessed>(e.InputAddress);
                                                                                              });

                                                                                              configurator.ReceiveEndpoint("execute-process-order", e =>
                                                                                              {
                                                                                                  e.ExecuteActivityHost<ProcessOrderActivity, ProcessOrderArguments>();

                                                                                                  EndpointConvention.Map<ProcessOrderArguments>(e.InputAddress);
                                                                                              });
                                                                                          }));
                           });

                           services.AddHostedService<WindowsService>();
                       });
        }
    }
}