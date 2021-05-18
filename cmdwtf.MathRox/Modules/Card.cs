
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using cmdwtf.MathRox.Configuration;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.Logging;

namespace cmdwtf.MathRox.Modules
{
	[Name("Dice Dice Baby")]
	[Group("card"), Alias("cards", "c")]
	public class Card : ModuleBase<SocketCommandContext>
	{
		private readonly Settings _settings;
		private readonly ILogger _logger;

		/// <summary>
		/// Constructor to grab the program settings from DI.
		/// </summary>
		/// <param name="settings">The program's settings.</param>
		/// <param name="logger">A spot to output logs.</param>
		public Card(Settings settings, ILogger<Card> logger)
		{
			_settings = settings;
			_logger = logger;
		}

		[Command]
		[Alias("deal")]
		[Remarks("Everyone in the channel uses the same deck.")]
		[Summary("Deal a number of cards from the top of the current deck.")]
		public async Task DealCardsAsync(
			[Summary("The number of cards to deal from the deck, defaulting to 1 card.")]
			int numberCards = 1)
			=> await Context.Message.ReplyAsync($"NYI: Would deal {numberCards} card{(numberCards == 1 ? "" : "s")}.");

		[Command("shuffle")]
		[Alias("new", "randomize")]
		[Remarks("Returns all cards to the deck, and randomizes the order.")]
		[Summary("Get and shuffle a new deck of cards.")]
		public async Task ShuffleDeckAsync()
			=> await Context.Message.ReplyAsync($"NYI: Card shuffling coming soon...");

		private async Task ReplyErrorAsync(string message)
		{
			AllowedMentions mentions = new()
			{
				MentionRepliedUser = true
			};

			await Context.Message.ReplyAsync(message, allowedMentions: mentions);
		}
	}
}
