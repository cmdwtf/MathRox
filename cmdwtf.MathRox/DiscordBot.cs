using System;
using System.Threading;
using System.Threading.Tasks;

using cmdwtf.MathRox.Configuration;
using cmdwtf.MathRox.Extensions;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace cmdwtf.MathRox
{
	public class DiscordBot : IHostedService
	{
		public IHost Host { get; set; }

		private DiscordSocketClient _client;
		private CommandHandler _commands;
		private ILogger _logger;
		private ILogger _discordLogger;
		private Settings _settings;

		public Guid Guid { get; } = System.Guid.NewGuid();

		public DiscordBot()
		{
			DiscordSocketConfig config = new()
			{
				LogLevel = LogSeverity.Verbose,
				MessageCacheSize = 20,
			};

			_client = new DiscordSocketClient(config);
		}

		private async Task RunAsync(IServiceProvider services, CancellationToken cancellationToken = default)
		{
			try
			{
				// grab services.
				ILoggerFactory loggerFactory = services.GetRequiredService<ILoggerFactory>();
				_logger = loggerFactory.CreateLogger<DiscordBot>();
				_discordLogger = loggerFactory.CreateLogger<IDiscordClient>();
				_settings = services.GetService<Settings>();

				_logger.LogInformation("Starting up...");

				_client.Log += DiscordLog;
				_client.Ready += ClientOnReady;
				_client.Connected += ClientOnConnected;

				_logger.LogInformation("Connecting...");

				await _client.LoginAsync(TokenType.Bot, _settings.ApiKeys.Discord.Token);
				await _client.StartAsync();

				// install commands.
				await _commands.InitializeCommandModulesAsync(services);

				// hang out forever.
				try
				{
					await Task.Delay(-1, cancellationToken);
				}
				catch (TaskCanceledException)
				{
					// all good!
				}
			}
			catch (System.Net.WebException wex)
			{
				_logger.LogInformation($"DiscordBot.Run() failed: {wex.Message}");
			}
		}

		#region Client Event Handlers

		private Task ClientOnConnected()
		{
			_logger.LogInformation($"Connection: {_client.ConnectionState}, Login: {_client.LoginState}.");
			return Task.CompletedTask;
		}

		private Task ClientOnReady()
		{
			_logger.LogInformation($"Ready.");
			return Task.CompletedTask;
		}

		private Task DiscordLog(LogMessage logMsg)
		{
			if (logMsg.Exception is null)
			{
				var level = logMsg.Severity.ToLogLevel();

				string msg = $"[{logMsg.Source}] {logMsg.Message}";
				_discordLogger.Log(level, msg);

			}
			else
			{
				_discordLogger.LogCritical($"[!:{logMsg.Exception.GetType().Name}] [{logMsg.Severity}:{logMsg.Source}] {logMsg.Exception.Message}; {logMsg.Message}");
			}

			return Task.CompletedTask;
		}

		#endregion Client Event Handlers

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			if (Host is null)
			{
				throw new NullReferenceException($"{nameof(Host)} must be set before start can be called.");
			}

			await RunAsync(Host.Services, cancellationToken);
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			if (_client != null && _client.ConnectionState == ConnectionState.Connected)
			{
				_logger.LogInformation("Shutting down...");
				await _client.StopAsync();
				_client = null;
			}
		}

		public async Task SetGameAsync(string game, string url = "")
		{
			await _client.SetGameAsync(game, url, ActivityType.Playing);
			_logger.LogInformation($"Game changed to {game} ({url}).");
		}

		internal void ConfigureServices(IServiceCollection services)
		{
			// add ourself to our dependency mapper
			services.AddSingleton(_client);

			// create our command handler, and get it's services.
			if (_commands == null)
			{
				_commands = new CommandHandler();
				_commands.ConfigureServices(services);
			}
		}
	}
}
