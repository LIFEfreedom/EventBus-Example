using LIFEfreedom.EventBusExample.Insrastructure.EventBus.Abstractions;
using LIFEfreedom.EventBusExample.Insrastructure.EventBus.CustomEvents;

using Microsoft.Extensions.Logging;

using System;

namespace LIFEfreedom.EventBusExample.EventGenerator.Services
{
	public class CustomObjectManager
	{
		private readonly IEventBus _eventBus;
		private readonly ILogger<CustomObjectManager> _logger;

		public CustomObjectManager(IEventBus eventBus, ILogger<CustomObjectManager> logger)
		{
			_eventBus = eventBus;
			_logger = logger;
		}

		public void PublishNewObjectCreatedEvent(int orderId, string description)
		{
			NewObjectCreatedIntegrationEvent ev = new NewObjectCreatedIntegrationEvent(orderId, description);
			try
			{
				_logger.LogInformation("----- Publishing integration event: {IntegrationEventId_published} from {AppName} - ({@IntegrationEvent})", ev.Id, Program.AppName, ev);
				_eventBus.Publish(ev);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "ERROR Publishing integration event: {IntegrationEventId} from {AppName} - ({@IntegrationEvent})", ev.Id, Program.AppName, ev);
			}
		}
	}
}
