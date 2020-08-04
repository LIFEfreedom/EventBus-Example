using RabbitMQ.Client;

using System;

namespace LIFEfreedom.EventBusExample.Insrastructure.EventBus.RabbitMQ
{
	public interface IRabbitMQPersistentConnection
		: IDisposable
	{
		bool IsConnected { get; }

		bool TryConnect();

		IModel CreateModel();
	}
}
