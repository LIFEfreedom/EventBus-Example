﻿using LIFEfreedom.EventBusExample.Insrastructure.EventBus.Abstractions;
using LIFEfreedom.EventBusExample.Insrastructure.EventBus.Events;
using LIFEfreedom.EventBusExample.Insrastructure.EventBus.Extensions;

using Microsoft.Extensions.Logging;

using Polly;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LIFEfreedom.EventBusExample.Insrastructure.EventBus.RabbitMQ
{
	public class RabbitMQEventBus : IEventBus, IDisposable
	{
		private const string BROKER_NAME = "event_bus_example";

		private readonly IRabbitMQPersistentConnection _persistentConnection;
		private readonly ILogger<RabbitMQEventBus> _logger;
		private readonly IEventBusSubscriptionsManager _subsManager;
		private readonly IServiceProvider _serviceProvider;
		private readonly int _retryCount;

		private IModel _consumerChannel;
		private string _queueName;

		public RabbitMQEventBus(IRabbitMQPersistentConnection persistentConnection, ILogger<RabbitMQEventBus> logger,
				IServiceProvider serviceProvider, IEventBusSubscriptionsManager subsManager, string queueName = null, int retryCount = 5)
		{
			_persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
			_serviceProvider = serviceProvider;
			_queueName = queueName;
			_consumerChannel = CreateConsumerChannel();
			_retryCount = retryCount;
			_subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
		}

		private void SubsManager_OnEventRemoved(object sender, string eventName)
		{
			if (!_persistentConnection.IsConnected)
			{
				_persistentConnection.TryConnect();
			}

			using (IModel channel = _persistentConnection.CreateModel())
			{
				channel.QueueUnbind(queue: _queueName,
					exchange: BROKER_NAME,
					routingKey: eventName);

				if (_subsManager.IsEmpty)
				{
					_queueName = string.Empty;
					_consumerChannel.Close();
				}
			}
		}

		public void Publish(IntegrationEvent @event)
		{
			if (!_persistentConnection.IsConnected)
			{
				_persistentConnection.TryConnect();
			}

			Polly.Retry.RetryPolicy policy = Policy.Handle<BrokerUnreachableException>()
				.Or<SocketException>()
				.WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
				{
					_logger.LogWarning(ex, "Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})", @event.Id, $"{time.TotalSeconds:n1}", ex.Message);
				});

			string eventName = @event.GetType().Name;

			_logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", @event.Id, eventName);

			using (IModel channel = _persistentConnection.CreateModel())
			{

				_logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.Id);

				channel.ExchangeDeclare(exchange: BROKER_NAME, type: "direct");

				byte[] body = JsonSerializer.SerializeToUtf8Bytes(@event);

				policy.Execute(() =>
				{
					IBasicProperties properties = channel.CreateBasicProperties();
					properties.DeliveryMode = 2; // persistent

					_logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);

					channel.BasicPublish(
						exchange: BROKER_NAME,
						routingKey: eventName,
						mandatory: true,
						basicProperties: properties,
						body: body);
				});
			}
		}

		public void Subscribe<TIntegrationEvent, TIntegrationEventHandler>()
			where TIntegrationEvent : IntegrationEvent
			where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
		{
			string eventName = _subsManager.GetEventKey<TIntegrationEvent>();
			DoInternalSubscription(eventName);

			_logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TIntegrationEventHandler).GetGenericTypeName());

			_subsManager.AddSubscription<TIntegrationEvent, TIntegrationEventHandler>();
			StartBasicConsume();
		}

		private void DoInternalSubscription(string eventName)
		{
			bool containsKey = _subsManager.HasSubscriptionsForEvent(eventName);
			if (!containsKey)
			{
				if (!_persistentConnection.IsConnected)
				{
					_persistentConnection.TryConnect();
				}

				using (IModel channel = _persistentConnection.CreateModel())
				{
					channel.QueueBind(queue: _queueName,
									  exchange: BROKER_NAME,
									  routingKey: eventName);
				}
			}
		}

		public void Unsibscribe<TIntegrationEvent, TIntegrationEventHandler>()
			where TIntegrationEvent : IntegrationEvent
			where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
		{
			string eventName = _subsManager.GetEventKey<TIntegrationEvent>();

			_logger.LogInformation("Unsubscribing from event {EventName}", eventName);

			_subsManager.RemoveSubscription<TIntegrationEvent, TIntegrationEventHandler>();
		}

		private IModel CreateConsumerChannel()
		{
			if (!_persistentConnection.IsConnected)
			{
				_persistentConnection.TryConnect();
			}

			_logger.LogTrace("Creating RabbitMQ consumer channel");

			IModel channel = _persistentConnection.CreateModel();

			channel.ExchangeDeclare(exchange: BROKER_NAME,
									type: "direct");

			channel.QueueDeclare(queue: _queueName,
								 durable: true,
								 exclusive: false,
								 autoDelete: false,
								 arguments: null);

			channel.CallbackException += (sender, ea) =>
			{
				_logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel");

				_consumerChannel.Dispose();
				_consumerChannel = CreateConsumerChannel();
				StartBasicConsume();
			};

			return channel;
		}

		private void StartBasicConsume()
		{
			_logger.LogTrace("Starting RabbitMQ basic consume");

			if (_consumerChannel != null)
			{
				AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_consumerChannel);

				consumer.Received += Consumer_Received;

				_consumerChannel.BasicConsume(
					queue: _queueName,
					autoAck: false,
					consumer: consumer);
			}
			else
			{
				_logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
			}
		}

		private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
		{
			string eventName = eventArgs.RoutingKey;
			string message = Encoding.UTF8.GetString(eventArgs.Body.Span);

			try
			{
				if (message.ToLowerInvariant().Contains("throw-fake-exception"))
				{
					throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
				}

				await ProcessEvent(eventName, message);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "----- ERROR Processing message \"{Message}\"", message);
			}

			// Even on exception we take the message off the queue.
			// in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX). 
			// For more information see: https://www.rabbitmq.com/dlx.html
			_consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
		}

		private async Task ProcessEvent(string eventName, string message)
		{
			_logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);

			if (_subsManager.HasSubscriptionsForEvent(eventName))
			{
				IEnumerable<SubscriptionInfo> subscriptions = _subsManager.GetHandlersForEvent(eventName);
				foreach (SubscriptionInfo subscription in subscriptions)
				{
					object handler = _serviceProvider.GetService(subscription.HandlerType);
					if (handler == null)
					{
						continue;
					}

					Type eventType = _subsManager.GetEventTypeByName(eventName);
					object integrationEvent = System.Text.Json.JsonSerializer.Deserialize(message, eventType);
					Type concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);

					await Task.Yield();
					await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
				}
			}
			else
			{
				_logger.LogWarning("No subscription for RabbitMQ event: {EventName}", eventName);
			}
		}

		public void Dispose()
		{
			if (_consumerChannel is object)
			{
				_consumerChannel.Dispose();
			}
		}
	}
}
