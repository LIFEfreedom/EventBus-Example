using EventGenerator.Options;

using LIFEfreedom.EventBusExample.Insrastructure.EventBus;
using LIFEfreedom.EventBusExample.Insrastructure.EventBus.Abstractions;
using LIFEfreedom.EventBusExample.Insrastructure.EventBus.RabbitMQ;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;

namespace EventGenerator
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
		{
            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

            services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
			{
                var options = sp.GetRequiredService<IOptions<RabbitMQEventBusOptions>>().Value;
                var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

                var factory = new ConnectionFactory()
                {
                    HostName = options.Hostname,
                    DispatchConsumersAsync = true
                };

                if (!string.IsNullOrEmpty(options.User))
                {
                    factory.UserName = options.User;
                }

                if (!string.IsNullOrEmpty(options.Password))
                {
                    factory.Password = options.Password;
                }

                var retryCount = 5;
                if (options.RetryCount != 0)
                {
                    retryCount = options.RetryCount;
                }

                return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
            });

			services.AddSingleton<IEventBus, RabbitMQEventBus>(sp =>
			{
				var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
				var logger = sp.GetRequiredService<ILogger<RabbitMQEventBus>>();
				var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
				var options = sp.GetRequiredService<IOptions<RabbitMQEventBusOptions>>().Value;

                var retryCount = 5;
                if (options.RetryCount != 0)
                {
                    retryCount = options.RetryCount;
                }

                return new RabbitMQEventBus(rabbitMQPersistentConnection, logger, sp, eventBusSubcriptionsManager, options.Application, retryCount);
			});

			return services;
		}
	}
}
