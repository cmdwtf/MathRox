using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using cmdwtf.MathRox.Configuration;
using cmdwtf.MathRox.Extensions;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace cmdwtf.MathRox
{
	public class CommandHandler
	{
		private bool _initialized = false;
		private CommandService _commands;
		private DiscordSocketClient _client;
		private IServiceProvider _serviceProvider;
		private ILogger _logger;
		private Settings _settings;

		public void ConfigureServices(IServiceCollection serviceCollection)
		{
			// Create Command Service, inject it into Dependency Map
			CommandServiceConfig config = new()
			{
				DefaultRunMode = RunMode.Async
			};

			_commands = new CommandService(config);
			serviceCollection.AddSingleton(_commands);
		}

		public async Task InitializeCommandModulesAsync(IServiceProvider provider)
		{
			if (_initialized)
			{
				return;
			}

			_initialized = true;

			// grab the service provider
			_serviceProvider = provider;

			// hookup logs
			_logger = _serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<CommandHandler>();
			_commands.Log += CommandLog;

			// load all the command modules
			await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);


			// subscribe to the incoming message
			_client = _serviceProvider.GetService(typeof(DiscordSocketClient)) as DiscordSocketClient;
			_client.MessageReceived += HandleCommand;

			// grab settings
			_settings = _serviceProvider.GetService<Settings>();
		}

		private Task CommandLog(LogMessage logMsg)
		{
			if (logMsg.Exception is null)
			{
				_logger.Log(logMsg.Severity.ToLogLevel(), logMsg.Message);
			}
			else
			{
				_logger.LogCritical($"[!:{logMsg.Exception.GetType().Name}] [{logMsg.Severity}:{logMsg.Source}] {logMsg.Exception.Message}; {logMsg.Message}");
			}

			return Task.CompletedTask;
		}

		public async Task HandleCommand(SocketMessage parameterMessage)
		{
			// Don't handle the command if it is a system message
			if (parameterMessage is not SocketUserMessage message)
			{
				return;
			}

			string[] pfx = _settings.Prefixes;

			// Mark where the prefix ends and the command begins
			int argPos = 0;
			// Determine if the message has a valid prefix, adjust argPos
			if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) ||
				pfx.Any(p => message.HasStringPrefix(p, ref argPos))))
			{
				return;
			}

			// Create a Command Context
			var context = new SocketCommandContext(_client, message);
			// Execute the Command, store the result
			IResult result = await _commands.ExecuteAsync(context, argPos, _serviceProvider);

			// If the command failed, notify the user
			if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
			{
				await message.Channel.SendMessageAsync($"**Error:** {result.ErrorReason}");
			}
		}
	}
}
