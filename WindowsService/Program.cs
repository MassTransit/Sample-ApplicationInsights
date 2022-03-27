using System;
using Azure.Monitor.OpenTelemetry.Exporter;
using MassTransit;
using Messaging.Activities;
using Messaging.Consumers;
using Messaging.Contracts;
using Messaging.StateMachines;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace WindowsService;

internal class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureLogging((context, builder) =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Trace);
            })
            .ConfigureServices((hostContext, services) =>
            {
                var hostConfig = hostContext.Configuration;
                
                services.AddOpenTelemetryTracing(builder =>
                {
                    builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                            .AddService("SampleService")
                            .AddTelemetrySdk()
                            .AddEnvironmentVariableDetector())
                        .AddSource("MassTransit")
                        .AddAzureMonitorTraceExporter(o =>
                        {
                            o.ConnectionString = hostConfig["ApplicationInsightsConnectionString"];
                        });
                });


                services.AddSingleton(DefaultEndpointNameFormatter.Instance);
                services.AddMassTransit(x =>
                {
                    x.SetInMemorySagaRepositoryProvider();
                    
                    var assembly = typeof(SubmitOrderConsumer).Assembly;

                    x.AddConsumers(assembly);
                    x.AddSagaStateMachines(assembly);
                    x.AddSagas(assembly);
                    x.AddActivities(assembly);

                    x.UsingAzureServiceBus((ctx, cfg) => 
                    {
                        cfg.Host(hostConfig["AzureServiceBusConnectionString"],
                            h =>
                            {
      
                            });
                        cfg.ConfigureEndpoints(ctx);
                    });
                });
            });
    }
}