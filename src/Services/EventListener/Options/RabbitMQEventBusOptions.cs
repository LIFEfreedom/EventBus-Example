namespace LIFEfreedom.EventBusExample.EventListener.Options
{
	public class RabbitMQEventBusOptions
	{
		public string Application { get; set; }

		public int RetryCount { get; set; }

		public string Hostname { get; set; }

		public string User { get; set; }

		public string Password { get; set; }
	}
}
