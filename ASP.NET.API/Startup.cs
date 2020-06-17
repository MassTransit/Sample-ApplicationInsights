namespace ASP.NET.API
{
    using System;
    using MassTransit;
    using MassTransit.Azure.ServiceBus.Core;
    using MassTransit.Context;
    using Microsoft.ApplicationInsights.DependencyCollector;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

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

            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((m, o) =>
                                                                                 {
                                                                                     m.IncludeDiagnosticSourceActivities.Remove("Microsoft.Azure.ServiceBus");
                                                                                     m.IncludeDiagnosticSourceActivities.Remove("Microsoft.Azure.EventHubs");
                                                                                     m.IncludeDiagnosticSourceActivities.Add("MassTransit");
                                                                                 });

            services.AddApplicationInsightsTelemetry(options =>
                                                     {
                                                         options.EnableAdaptiveSampling = false;
                                                         options.InstrumentationKey     = Configuration["ApplicationInsightsInstrumentationKey"];
                                                     });

            services.AddMassTransit(x =>
                                    {
                                        x.AddBus(provider => Bus.Factory.CreateUsingAzureServiceBus(cfg =>
                                                                                                    {
                                                                                                        cfg.Host(Configuration["AzureServiceBusConnectionString"],
                                                                                                                            h =>
                                                                                                                            {
                                                                                                                                h.OperationTimeout = TimeSpan.FromSeconds(30);
                                                                                                                            });
                                                                                                    }));
                                    });

            services.AddHostedService<BusService>();

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

            app.UseEndpoints(endpoints =>
                             {
                                 endpoints.MapControllers();
                             });
        }
    }
}