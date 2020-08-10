using LIFEfreedom.EventBusExample.Insrastructure.EventBus.Abstractions;
using LIFEfreedom.EventBusExample.Insrastructure.EventBus.CustomEvents;

using Microsoft.Extensions.Logging;

using System.Threading.Tasks;

namespace LIFEfreedom.EventBusExample.EventListener.EventHandlers
{
	public class NewObjectCreatedIntegrationEventHandler : IIntegrationEventHandler<NewObjectCreatedIntegrationEvent>
	{
		private readonly ILogger<NewObjectCreatedIntegrationEventHandler> _logger;

		public NewObjectCreatedIntegrationEventHandler(ILogger<NewObjectCreatedIntegrationEventHandler> logger)
		{
			_logger = logger;
		}

		public Task HandleAsync(NewObjectCreatedIntegrationEvent ev)
		{
			_logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", ev.Id, Program.AppName, ev);

			return Task.CompletedTask;
		}
	}
}
