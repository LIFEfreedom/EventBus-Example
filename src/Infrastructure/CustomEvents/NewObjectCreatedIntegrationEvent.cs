using LIFEfreedom.EventBusExample.Insrastructure.EventBus.Events;

using System;
using System.Text.Json.Serialization;

namespace LIFEfreedom.EventBusExample.Insrastructure.EventBus.CustomEvents
{
	public class NewObjectCreatedIntegrationEvent : IntegrationEvent
	{
		public int ObjectId { get; }

		public string Description { get; }

		public NewObjectCreatedIntegrationEvent(int objectId, string description) : base()
		{
			ObjectId = objectId;
			Description = description;
		}

		[JsonConstructor]
		public NewObjectCreatedIntegrationEvent(Guid id, DateTime creationDate, int objectId, string description) : base(id, creationDate)
		{
			ObjectId = objectId;
			Description = description;
		}
	}
}
