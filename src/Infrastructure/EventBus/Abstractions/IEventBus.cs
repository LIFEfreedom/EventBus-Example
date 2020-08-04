using LIFEfreedom.EventBusExample.Insrastructure.EventBus.Events;

namespace LIFEfreedom.EventBusExample.Insrastructure.EventBus.Abstractions
{
	public interface IEventBus
	{
		void Publish<TIntegrationEvent>(TIntegrationEvent @event) where TIntegrationEvent : IntegrationEvent;

		void Subscribe<TIntegrationEvent, TIntegrationEventHandler>()
			where TIntegrationEvent : IntegrationEvent
			where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>;

		void Unsibscribe<TIntegrationEvent, TIntegrationEventHandler>()
			where TIntegrationEvent : IntegrationEvent
			where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>;
	}
}
