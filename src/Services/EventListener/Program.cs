using LIFEfreedom.EventBusExample.EventListener.EventHandlers;
using LIFEfreedom.EventBusExample.EventListener.Options;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Reflection;

namespace LIFEfreedom.EventBusExample.EventListener
{
	public class Program
	{
		public static readonly string AppName = Assembly.GetExecutingAssembly().GetName().Name;

		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					services.Configure<RabbitMQEventBusOptions>(hostContext.Configuration.GetSection("EventBus"));
					services.AddEventBus(hostContext.Configuration);
					services.AddTransient<NewObjectCreatedIntegrationEventHandler>();
					services.AddHostedService<Worker>();
				});
		}
	}
}
