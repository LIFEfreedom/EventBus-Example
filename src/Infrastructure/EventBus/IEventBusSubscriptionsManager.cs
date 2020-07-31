﻿using LIFEfreedom.EventBusExample.Insrastructure.EventBus.Abstractions;
using LIFEfreedom.EventBusExample.Insrastructure.EventBus.Events;

using System;
using System.Collections.Generic;

namespace LIFEfreedom.EventBusExample.Insrastructure.EventBus
{
	public interface IEventBusSubscriptionsManager
	{
		bool IsEmpty { get; }

		event EventHandler<string> OnEventRemoved;

		void AddSubscription<TIntegrationEvent, TIntegrationEventHandler>()
			 where TIntegrationEvent : IntegrationEvent
			 where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>;

		void RemoveSubscription<TIntegrationEvent, TIntegrationEventHandler>()
			 where TIntegrationEvent : IntegrationEvent
			 where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>;

		bool HasSubscriptionsForEvent<TIntegrationEvent>() where TIntegrationEvent : IntegrationEvent;

		bool HasSubscriptionsForEvent(string eventName);

		Type GetEventTypeByName(string eventName);

		void Clear();

		IEnumerable<SubscriptionInfo> GetHandlersForEvent<TIntegrationEvent>() where TIntegrationEvent : IntegrationEvent;

		IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);

		string GetEventKey<TIntegrationEvent>() where TIntegrationEvent : IntegrationEvent;
	}
}
