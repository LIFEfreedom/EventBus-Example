using System;
using System.Text.Json.Serialization;

namespace LIFEfreedom.EventBusExample.Insrastructure.EventBus.Events
{
	public class IntegrationEvent
	{
		public IntegrationEvent()
		{
			Id = Guid.NewGuid();
			CreationDate = DateTime.UtcNow;
		}

		[JsonConstructor]
		public IntegrationEvent(Guid id, DateTime creationDate)
		{
			Id = id;
			CreationDate = creationDate;
		}

		public Guid Id { get; }

		public DateTime CreationDate { get; }
	}
}
