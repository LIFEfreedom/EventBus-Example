using LIFEfreedom.EventBusExample.Insrastructure.EventBus.Events;

using System.Threading.Tasks;

namespace LIFEfreedom.EventBusExample.Insrastructure.EventBus.Abstractions
{
	public interface IIntegrationEventHandler<in TIntegrationEvent>
		where TIntegrationEvent : IntegrationEvent
	{
		Task HandleAsync(TIntegrationEvent @event);
	}
}
