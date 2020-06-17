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
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DependencyCollector;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.ApplicationInsights;
    using Microsoft.Extensions.Options;

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
                           var hostConfig = hostContext.Configuration;
                           var module = new DependencyTrackingTelemetryModule();
                           module.IncludeDiagnosticSourceActivities.Add("MassTransit");

                           TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
                           configuration.InstrumentationKey = hostConfig["ApplicationInsightsInstrumentationKey"];
                           configuration.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());
                           
                           module.Initialize(configuration);
                           
                           var loggerOptions = new ApplicationInsightsLoggerOptions();
                           var applicationInsightsLoggerProvider = new ApplicationInsightsLoggerProvider(Options.Create(configuration),
                                                                                                         Options.Create(loggerOptions));
                           ILoggerFactory factory = new LoggerFactory();
                           factory.AddProvider(applicationInsightsLoggerProvider);
                           LogContext.ConfigureCurrentLogContext(factory);
                           
                           services.AddMassTransit(x =>
                           {
                               x.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
                               x.AddBus(context => Bus.Factory.CreateUsingAzureServiceBus(
                                                                                          configurator =>
                                                                                          {
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