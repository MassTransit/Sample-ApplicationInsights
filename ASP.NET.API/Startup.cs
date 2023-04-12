using System;
using Azure.Monitor.OpenTelemetry.Exporter;
using MassTransit;
using MassTransit.Logging;
using MassTransit.Monitoring;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ASP.NET.API;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Trace);
        });

        static void ConfigureResource(ResourceBuilder builder)
        {
            builder
                .AddService("SampleAPI")
                .AddTelemetrySdk()
                .AddEnvironmentVariableDetector();
        }

        services.AddOpenTelemetry()
            .ConfigureResource(ConfigureResource)
            .WithTracing(x => x.AddSource(DiagnosticHeaders.DefaultListenerName)
                .AddAspNetCoreInstrumentation()
                .AddAzureMonitorTraceExporter(o =>
                {
                    o.ConnectionString = Configuration["ApplicationInsightsConnectionString"];
                }))
            .WithMetrics(x => x.AddMeter(InstrumentationOptions.MeterName)
                .AddAspNetCoreInstrumentation()
                .AddAzureMonitorMetricExporter(o =>
                {
                    o.ConnectionString = Configuration["ApplicationInsightsConnectionString"];
                }));

        services.AddMassTransit(x =>
        {
            x.UsingAzureServiceBus((ctx, cfg) =>
            {
                cfg.Host(Configuration["AzureServiceBusConnectionString"],
                    h => { });
            });
        });

        services.AddOpenApiDocument(cfg => cfg.PostProcess = d => d.Info.Title = "Sample-ApplicationInsights");
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        LogContext.ConfigureCurrentLogContext(loggerFactory);

        if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseOpenApi();
        app.UseSwaggerUi3();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}
