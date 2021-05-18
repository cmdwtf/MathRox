using System.Text;
using System.Threading.Tasks;

using cmdwtf.MathRox.Access;
using cmdwtf.MathRox.Configuration;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.Logging;

namespace cmdwtf.MathRox.Modules
{
	[Name("Admin")]
	[Group("admin")]
	[RequiredAccessLevel(AccessLevel.BotOwner)]
	public class Administration : ModuleBase<SocketCommandContext>
	{
		private readonly Settings _settings;
		private readonly ILogger _logger;

		/// <summary>
		/// Constructor to grab the program settings from DI.
		/// </summary>
		/// <param name="settings">The program's settings.</param>
		/// <param name="logger">A spot to output logs.</param>
		public Administration(Settings settings, ILogger<Administration> logger)
		{
			_settings = settings;
			_logger = logger;
		}

		[Command("link"), Alias("invite", "invitelink")]
		[Summary("Has the bot private message you the link to invite him to a server.")]
		[Remarks("Has the bot private message you the link to invite him to a server.")]
		[RequiredAccessLevel(AccessLevel.BotOwner)]
		public async Task GetInviteLink()
		{
			string clientID = _settings.ApiKeys.Discord.ClientID;
			string link = $"https://discord.com/api/oauth2/authorize?client_id={clientID}&permissions=379904&redirect_uri=https%3A%2F%2Fcmd.wtf%2Foauth2&response_type=code&scope=messages.read%20bot%20rpc.notifications.read%20rpc%20identify%20email";
			//string classicLink = $"https://discordapp.com/oauth2/authorize?client_id={_settings.ApiKeys.Discord.ClientID}&scope=bot&permissions=0";
			string message = $"Hey, my invite link is {link}\nThe person who uses this link must have the `Manage Server` role to add me!";
			IDMChannel dmc = await Context.User.GetOrCreateDMChannelAsync();
			await dmc.SendMessageAsync(message);
		}

		[Command("guilds"), Alias("servers")]
		[Remarks("Displays the guilds the bot is currently a member of.")]
		[Summary("Displays the guilds the bot is currently a member of.")]
		[RequiredAccessLevel(AccessLevel.BotOwner)]
		public async Task GetGuilds()
		{
			var output = new StringBuilder();
			output.AppendLine("I'm a member of: ```");

			foreach (SocketGuild g in Context.Client.Guilds)
			{
				output.AppendLine($"{g.Name} (Owner: {g.Owner})");
			}

			output.Append("```");

			await ReplyAsync(output.ToString());
		}

		[Command("playing"), Alias("game")]
		[Summary("Changes the 'game' that the bot is playing.")]
		[Remarks("Changes the 'game' that the bot is playing.")]
		[RequiredAccessLevel(AccessLevel.BotOwner)]
		public async Task SetPlaying([Remainder] string game)
		{
			Log($"SetPlaying: {game}");
			await Context.Client.SetGameAsync(game, null, ActivityType.Playing);
		}

		/// <summary>
		/// Just a simple wrapper for this class to dump to the console.
		/// </summary>
		/// <param name="s">The text to output.</param>
		/// <param name="show_message">If true, will prepend the command message before the string.</param>
		protected void Log(string s, bool show_message = false)
		{
			string msg = (show_message ? $"({Context.Message.Content}) " : "");

			string chan = Context.Message.Channel?.Name ?? "?";
			if (Context.Message.Channel is SocketGuildChannel sgc)
			{
				chan = $"{sgc.Guild.Name}#{chan}";
			}

			_logger.LogInformation($"[Req:{chan}/{Context.Message.Author.Username}#{Context.Message.Author.Discriminator}]: {msg}{s}");
		}
	}
}
