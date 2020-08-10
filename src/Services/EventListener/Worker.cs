using LIFEfreedom.EventBusExample.EventListener.EventHandlers;

using LIFEfreedom.EventBusExample.Insrastructure.EventBus.Abstractions;
using LIFEfreedom.EventBusExample.Insrastructure.EventBus.CustomEvents;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Threading;
using System.Threading.Tasks;

namespace LIFEfreedom.EventBusExample.EventListener
{
	public class Worker : BackgroundService
	{
		private readonly ILogger<Worker> _logger;
		private readonly IEventBus _eventBus;

		public Worker(ILogger<Worker> logger, IEventBus eventBus)
		{
			_logger = logger;
			_eventBus = eventBus;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Attempting to subscribe to an event");

			_eventBus.Subscribe<NewObjectCreatedIntegrationEvent, NewObjectCreatedIntegrationEventHandler>();

			_logger.LogInformation("Event subscription was successful");

			return Task.CompletedTask;
		}
	}
}
