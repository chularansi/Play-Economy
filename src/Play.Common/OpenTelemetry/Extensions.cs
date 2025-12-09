using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Play.Common.Settings;

namespace Play.Common.OpenTelemetry
{
    public static class Extensions
    {
        public static IServiceCollection AddTracingAndMetrics(this IServiceCollection services, IConfiguration configuration)
        {
            ServiceSettings serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

            // Configure OpenTelemetry
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(
                    serviceName: serviceSettings.ServiceName,
                    serviceVersion: "1.0.0"))
                .WithTracing(tracingBuilder =>
                {
                    tracingBuilder
                        .AddSource(serviceSettings.ServiceName) // Define your ActivitySource name
                        .AddSource("MassTransit")
                        .AddAspNetCoreInstrumentation() // Track any inbound request into our controllers via APIs
                        .AddHttpClientInstrumentation() // Track Http calls that come from our microservice to the outside
                                                        // Add other instrumentation as needed
                                                        //.AddConsoleExporter();
                        .AddOtlpExporter(options =>
                        {
                            JaegerSettings jaegerSettings = configuration.GetSection(nameof(JaegerSettings)).Get<JaegerSettings>();

                            options.Endpoint = new Uri($"http://{jaegerSettings.Host}:{jaegerSettings.Port}");
                        }); // Export traces to an OTLP collector (Jaeger)
                })
                .WithMetrics(metricsBuilder =>
                {
                    metricsBuilder
                        .AddMeter(serviceSettings.ServiceName)
                        .AddMeter("MassTransit")
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddPrometheusExporter();
                });

            //services.AddConsumeObserver<IConsumeObserver>();

            return services;
        }
    }
}
