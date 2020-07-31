using System;

namespace LIFEfreedom.EventBusExample.Insrastructure.EventBus
{
	public class SubscriptionInfo
	{
		public Type HandlerType { get; }

		public SubscriptionInfo(Type handlerType)
		{
			HandlerType = handlerType;
		}
	}
}
