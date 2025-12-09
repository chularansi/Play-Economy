//using MassTransit;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Play.Common.Settings;
//using System.Reflection;

//namespace Play.Common.MassTransit
//{
//    public static class Extensions
//    {
//        public static IServiceCollection AddMassTransitWithRabbitMQ(
//            this IServiceCollection services,
//            Action<IRetryConfigurator> configureRetries = null)
//        {
//            services.AddMassTransit(configure =>
//            {
//                configure.AddConsumers(Assembly.GetEntryAssembly());
//                configure.UsingPlayEconomyRabbitMq(configureRetries);
//            });

//            return services;
//        }

//        public static void UsingPlayEconomyRabbitMq(this IBusRegistrationConfigurator configure,
//            Action<IRetryConfigurator> configureRetries = null)
//        {
//            configure.UsingRabbitMq((context, configurator) =>
//            {
//                var configuration = context.GetService<IConfiguration>();
//                var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
//                var rabbitMQSettings = configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
//                configurator.Host(rabbitMQSettings.Host);
//                configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ToString(), false));

//                if (configureRetries == null)
//                {
//                    configureRetries = (retryConfigurator) => retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
//                }

//                configurator.UseMessageRetry(configureRetries);
//            });
//        }
//    }
//}
