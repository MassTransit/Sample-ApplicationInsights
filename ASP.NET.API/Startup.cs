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
            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((m, o) =>
            {
                m.IncludeDiagnosticSourceActivities.Add("MassTransit");
            });

            services.AddApplicationInsightsTelemetry(Configuration["ApplicationInsightsInstrumentationKey"]);
            services.AddMassTransit(x =>
            {
                x.AddBus(provider => Bus.Factory.CreateUsingAzureServiceBus(cfg =>
                {
                    var host = cfg.Host(Configuration["AzureServiceBusConnectionString"],
                                        h =>
                                        {
                                            h.OperationTimeout = TimeSpan.FromSeconds(30);
                                        });
                }));
            });

            services.AddHostedService<BusService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            LogContext.ConfigureCurrentLogContext(loggerFactory);

            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}