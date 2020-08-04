using LIFEfreedom.EventBusExample.Insrastructure.EventBus.Abstractions;
using LIFEfreedom.EventBusExample.Insrastructure.EventBus.Events;

using System;
using System.Collections.Generic;
using System.Linq;

namespace LIFEfreedom.EventBusExample.Insrastructure.EventBus
{
	public class InMemoryEventBusSubscriptionsManager : IEventBusSubscriptionsManager
	{
		private readonly Dictionary<string, List<SubscriptionInfo>> _handlers = new Dictionary<string, List<SubscriptionInfo>>();
		private readonly List<Type> _eventTypes = new List<Type>();

		public bool IsEmpty => !_handlers.Keys.Any();

		public event EventHandler<string> OnEventRemoved;

		public void AddSubscription<TIntegrationEvent, TIntegrationEventHandler>()
			where TIntegrationEvent : IntegrationEvent
			where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
		{
			Type eventType = typeof(TIntegrationEvent);

			DoAddSubscription(typeof(TIntegrationEventHandler), GetEventKeyByType(eventType));

			if (!_eventTypes.Contains(eventType))
			{
				_eventTypes.Add(eventType);
			}
		}

		public void Clear()
		{
			_handlers.Clear();
		}

		public string GetEventKey<TIntegrationEvent>() where TIntegrationEvent : IntegrationEvent
		{
			return GetEventKeyByType(typeof(TIntegrationEvent));
		}

		public string GetEventKeyByType(Type eventType)
		{
			return eventType.Name;
		}

		public Type GetEventTypeByName(string eventName)
		{
			return _eventTypes.SingleOrDefault(t => t.Name == eventName);
		}

		public IEnumerable<SubscriptionInfo> GetHandlersForEvent<TIntegrationEvent>() where TIntegrationEvent : IntegrationEvent
		{
			string key = GetEventKey<TIntegrationEvent>();
			return GetHandlersForEvent(key);
		}

		public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName)
		{
			return _handlers[eventName];
		}

		public bool HasSubscriptionsForEvent<TIntegrationEvent>() where TIntegrationEvent : IntegrationEvent
		{
			string key = GetEventKey<TIntegrationEvent>();
			return HasSubscriptionsForEvent(key);
		}

		public bool HasSubscriptionsForEvent(string eventName)
		{
			return _handlers.ContainsKey(eventName);
		}

		public void RemoveSubscription<TIntegrationEvent, TIntegrationEventHandler>()
			where TIntegrationEvent : IntegrationEvent
			where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
		{
			SubscriptionInfo handlerToRemove = FindSubscriptionToRemove<TIntegrationEvent, TIntegrationEventHandler>();
			string eventName = GetEventKey<TIntegrationEvent>();
			DoRemoveHandler(eventName, handlerToRemove);
		}

		private SubscriptionInfo FindSubscriptionToRemove<TIntegrationEvent, TIntegrationEventHandler>()
			where TIntegrationEvent : IntegrationEvent
			where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
		{
			string eventName = GetEventKey<TIntegrationEvent>();
			return DoFindSubscriptionToRemove(eventName, typeof(TIntegrationEventHandler));
		}

		private SubscriptionInfo DoFindSubscriptionToRemove(string eventName, Type handlerType)
		{
			if (!HasSubscriptionsForEvent(eventName))
			{
				return null;
			}

			return _handlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType);
		}

		private void DoRemoveHandler(string eventName, SubscriptionInfo subsToRemove)
		{
			if (subsToRemove != null)
			{
				_handlers[eventName].Remove(subsToRemove);

				if (!_handlers[eventName].Any())
				{
					_handlers.Remove(eventName);
					Type eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);

					if (eventType != null)
					{
						_eventTypes.Remove(eventType);
					}
					RaiseOnEventRemoved(eventName);
				}
			}
		}

		private void RaiseOnEventRemoved(string eventName)
		{
			EventHandler<string> handler = OnEventRemoved;
			handler?.Invoke(this, eventName);
		}

		private void DoAddSubscription(Type handlerType, string eventName)
		{
			if (!HasSubscriptionsForEvent(eventName))
			{
				_handlers.Add(eventName, new List<SubscriptionInfo>());
			}

			if (_handlers[eventName].Any(s => s.HandlerType == handlerType))
			{
				throw new ArgumentException(
					$"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
			}

			_handlers[eventName].Add(new SubscriptionInfo(handlerType));
		}
	}
}
