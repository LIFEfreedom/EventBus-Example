using LIFEfreedom.EventBusExample.EventListener.Options;

using LIFEfreedom.EventBusExample.Insrastructure.EventBus;
using LIFEfreedom.EventBusExample.Insrastructure.EventBus.Abstractions;
using LIFEfreedom.EventBusExample.Insrastructure.EventBus.RabbitMQ;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RabbitMQ.Client;

namespace LIFEfreedom.EventBusExample.EventListener
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

			services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
			{
				RabbitMQEventBusOptions options = sp.GetRequiredService<IOptions<RabbitMQEventBusOptions>>().Value;
				ILogger<DefaultRabbitMQPersistentConnection> logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

				ConnectionFactory factory = new ConnectionFactory()
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

				int retryCount = 5;
				if (options.RetryCount != 0)
				{
					retryCount = options.RetryCount;
				}

				return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
			});

			services.AddSingleton<IEventBus, RabbitMQEventBus>(sp =>
			{
				IRabbitMQPersistentConnection rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
				ILogger<RabbitMQEventBus> logger = sp.GetRequiredService<ILogger<RabbitMQEventBus>>();
				IEventBusSubscriptionsManager eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
				RabbitMQEventBusOptions options = sp.GetRequiredService<IOptions<RabbitMQEventBusOptions>>().Value;

				int retryCount = 5;
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
