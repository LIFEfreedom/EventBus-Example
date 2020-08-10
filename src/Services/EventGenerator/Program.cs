using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using System.Reflection;

namespace EventGenerator
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
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
		}
	}
}
