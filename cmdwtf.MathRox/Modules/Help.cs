using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using cmdwtf.MathRox.Configuration;
using cmdwtf.Toolkit;

using Discord;
using Discord.Commands;

namespace cmdwtf.MathRox.Modules
{
	[Name("Help")]
	public class Help : ModuleBase<SocketCommandContext>
	{
		private readonly CommandService _commandService;
		private readonly Settings _settings;
		private readonly IServiceProvider _services;

		private const string _github = "https://github.com";
		private const string _repoOwner = "cmdwtf";
		private const string _repo = "MathRox";

		/// <summary>
		/// The changelog values returned by the `changelog` commmand.
		/// </summary>
		private readonly string[] _changelog =
		{
			"```asciidoc",
			"v1.0.0:: Initial release.",
			"```"
		};

		/// <summary>
		/// A short information paragraph returned by the `about` command.
		/// </summary>
		private readonly string[] _about =
		{
			"Hi, I was created by <@116026533688115204>.",
			"I'm just a silly little bot that does a little RNG represented in the form of dice rolls and such.",
			"I'm definitely not perfect, so please do feel free to share issues or ideas you have with my creator!",
			$"If you'd like, you can check out my source code at <{_github}/{_repoOwner}/{_repo}>!"
		};

		/// <summary>
		/// Constructor to grab the program settings from DI.
		/// </summary>
		/// <param name="settings">The program's settings.</param>
		public Help(CommandService commandService, Settings settings, IServiceProvider services)
		{
			_commandService = commandService;
			_settings = settings;
			_services = services;
		}

		[Command("help")]
		[Summary("This command!")]
		[Remarks("Really?")]
		public async Task HelpCommand([Remainder] string command = "")
		{
			if (string.IsNullOrEmpty(command))
			{
				await HelpGeneral();
			}
			else
			{
				await HelpCommandInfo(command);
			}
		}

		private IEnumerable<string> GetEscapedIfMarkdownPrefixes()
		{
			// probably not the best way to do this. oh well.
			string[] markdownPrefixes = new string[]
			{
				"`", "*", "_", "||", "~~"
			};

			return _settings.Prefixes.Select(prefix =>
			{
				if (markdownPrefixes.Contains(prefix))
				{
					return $"\\{prefix}";
				}

				return prefix;
			});
		}

		/// <summary>
		/// Spit out info about all the commands I have, that the user has access to.
		/// </summary>
		public async Task HelpGeneral()
		{
			string[] fixedPrefixes = GetEscapedIfMarkdownPrefixes().ToArray();
			string prefixes = string.Join(", ", fixedPrefixes);
			string primaryPrefix = fixedPrefixes[0];

			var builder = new EmbedBuilder()
			{
				Color = new Color(114, 137, 218),
				Description = $"These are the commands you can use.\nTry {primaryPrefix}help <command> for more information."
			};

			if (fixedPrefixes.Length > 1)
			{
				builder.Description += $"\nYou can use these command prefixes: {prefixes}";
			}

			foreach (ModuleInfo module in _commandService.Modules)
			{
				string description = null;
				foreach (CommandInfo cmd in module.Commands)
				{
					PreconditionResult result = await cmd.CheckPreconditionsAsync(Context, _services);
					if (result.IsSuccess)
					{
						description += $"{primaryPrefix}{cmd.Aliases[0]}";
						if (cmd.Summary?.Length > 0)
						{
							description += " — " + cmd.Summary;
						}

						description += "\n";
					}
				}

				if (!string.IsNullOrWhiteSpace(description))
				{
					builder.AddField(x =>
					{
						x.Name = module.Name;
						x.Value = description;
						x.IsInline = false;
					});
				}
			}

			await ReplyAsync(string.Empty, false, builder.Build());
		}

		/// <summary>
		/// Spit out information about the specifically requested command.
		/// </summary>
		/// <param name="command">The command to get info about</param>
		public async Task HelpCommandInfo(string command)
		{
			SearchResult result = _commandService.Search(Context, command);

			if (!result.IsSuccess)
			{
				await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
				return;
			}

			//string prefix = _settings.Prefixes[0];
			EmbedBuilder builder = new()
			{
				Color = new(114, 137, 218),
				Description = $"Here are some commands like **{command}**"
			};

			foreach (CommandMatch match in result.Commands)
			{
				CommandInfo cmd = match.Command;

				builder.AddField(field =>
				{
					field.Name = string.Join(", ", cmd.Aliases);
					field.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
							  $"Remarks: {cmd.Remarks}";
					field.IsInline = false;
				});
			}

			await ReplyAsync("", false, builder.Build());
		}

		[Command("about"), Alias("info")]
		[Summary("Information about this bot.")]
		[Remarks("Returns a little bit of information about myself!")]
		public async Task About()
		{
			string about = string.Join("\n", _about) +
							"\n" + GenerateTechnicalInfoForNerds();

			await ReplyAsync(about);
		}

		[Command("changelog"), Alias("changes")]
		[Summary("What's new?")]
		[Remarks("Returns a little bit of information about what's changed lately.")]
		public async Task Changelog()
		{
			string changelog = string.Join("\n", _changelog) +
							"\n" + GenerateTechnicalInfoForNerds();

			await ReplyAsync(changelog);
		}

		/// <summary>
		/// Generate a small code block that describes our application and the version of discord.net we're using.
		/// </summary>
		/// <returns>The description string</returns>
		private string GenerateTechnicalInfoForNerds()
		{
			string appFullName = Toolkit.Assembly.FullName;
			DateTime buildDate = Toolkit.Assembly.CompileDate;
			string appConfig = Toolkit.Assembly.CallingAssembly.IsDebug() ? "Debug" : "Release";
			string builtOn = buildDate.ToLongDateString() + " at " + buildDate.ToLongTimeString();
			var numberStonesAssy = System.Reflection.Assembly.GetAssembly(typeof(NumberStones.Dice));
			string numberStonesVer = ((AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(numberStonesAssy, typeof(AssemblyFileVersionAttribute), false)).Version;
			string numberStones = $"v{numberStonesVer} {numberStonesAssy.FullName}";
			string discordNet = $"v{DiscordConfig.Version} (API v{DiscordConfig.APIVersion}); " + System.Reflection.Assembly.GetAssembly(Context.Client.GetType()).FullName;
			return "Technical info for nerds:\n" +
					$"```" +
					$"Application: {appFullName} ({appConfig})\n" +
					$"NumberStones: {numberStones}\n" +
					$"Discord.Net: {discordNet}\n" +
					$"Built on: {builtOn}" +
					$"```";
		}
	}
}
