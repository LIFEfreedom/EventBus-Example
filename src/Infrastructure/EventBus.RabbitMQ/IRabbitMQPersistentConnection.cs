using RabbitMQ.Client;

using System;

namespace LIFEfreedom.EventBusExample.Insrastructure.EventBusRabbitMQ
{
	public interface IRabbitMQPersistentConnection
		: IDisposable
	{
		bool IsConnected { get; }

		bool TryConnect();

		IModel CreateModel();
	}
}
