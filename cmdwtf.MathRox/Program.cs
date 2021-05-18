using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using cmdwtf.MathRox.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace cmdwtf.MathRox
{
	internal class Program
	{
		internal static IHost _host;

		private static readonly Settings _settings = new();
		private static ILogger _log;
		private static readonly CancellationTokenSource _botCancellationTokenSource = new();

		private static async Task Main(string[] args)
		{
			SetWindowTitle();

			// init host/di
			IHostBuilder hostBuilder = CreateHostBuilder(args);

			// build host/di
			_host = hostBuilder.Build();

			// get top level services/settings
			IConfiguration config = _host.Services.GetRequiredService<IConfiguration>();
			config.Bind(_settings);
			_log = _host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();

			// make sure api keys are set.
			if (_settings.HasRequiredSettings == false)
			{
				_log.LogInformation("One or more required settings are not configured. Please check the documentation for more information.\nPress any key to exit.");
				Console.ReadKey();
				Environment.Exit(-1);
			}

			// give the bot the host
			DiscordBot bot = _host.Services.GetRequiredService<DiscordBot>();
			bot.Host = _host;

			// run!
			await _host.RunAsync(_botCancellationTokenSource.Token).ConfigureAwait(false);

			// give things a few ms to finish up.
			Task.Delay(500).Wait();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
#if DEBUG
			.UseEnvironment("Development")
#else
			.UseEnvironment("Production")
#endif //DEBUG
			.ConfigureHostConfiguration(builder =>
			{
				// nothing else yet.
			})
			.ConfigureLogging(logging =>
			{
				logging.SetMinimumLevel(LogLevel.Debug);
			})
			.ConfigureAppConfiguration((context, builder) =>
			{
				builder.AddJsonFile("appsettings.json", optional: true);

				if (context.HostingEnvironment.IsDevelopment())
				{
					builder.AddUserSecrets<Program>();
				}

			})
			.ConfigureServices(services =>
			{
				// init discord
				DiscordBot bot = new();
				bot.ConfigureServices(services);

				// add it as a singleton and also as a hosted service
				services.AddSingleton(bot);
				services.AddHostedService(s => bot);

				// add our settings so they can be discovered later.
				services.AddSingleton(_settings);
			});

		private static void SetWindowTitle()
		{
			var asy = Assembly.GetExecutingAssembly();
			if (string.IsNullOrWhiteSpace(asy.FullName) == false)
			{
				Console.Title = asy.FullName;
			}
		}
	}
}
