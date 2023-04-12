using Azure.Monitor.OpenTelemetry.Exporter;
using MassTransit;
using MassTransit.Logging;
using MassTransit.Monitoring;
using Messaging.Consumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;

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
                
                static void ConfigureResource(ResourceBuilder builder)
                {
                    builder
                        .AddService("SampleService")
                        .AddTelemetrySdk()
                        .AddEnvironmentVariableDetector();
                }

                services.AddOpenTelemetry()
                    .ConfigureResource(ConfigureResource)
                    .WithTracing(x => x.AddSource(DiagnosticHeaders.DefaultListenerName)
                        .AddAzureMonitorTraceExporter(o =>
                        {
                            o.ConnectionString = hostConfig["ApplicationInsightsConnectionString"];
                        }))
                    .WithMetrics(x => x.AddMeter(InstrumentationOptions.MeterName)
                        .AddAzureMonitorMetricExporter(o =>
                        {
                            o.ConnectionString = hostConfig["ApplicationInsightsConnectionString"];
                        }));


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
