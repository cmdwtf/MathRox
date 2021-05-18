using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using cmdwtf.MathRox.Configuration;
using cmdwtf.MathRox.Messages;
using cmdwtf.NumberStones;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.Logging;

namespace cmdwtf.MathRox.Modules
{
	[Name("Dice Dice Baby")]
	[Group("roll"), Alias("dice", "r")]
	public class Dice : ModuleBase<SocketCommandContext>
	{
		private readonly Settings _settings;
		private readonly ILogger _logger;

		private static readonly Regex _variableNameCheck = new(@"^\$(?<name>\w*)\s*(?<equals>=)\s*(?<expression>.*)$", RegexOptions.Compiled);

		/// <summary>
		/// Constructor to grab the program settings from DI.
		/// </summary>
		/// <param name="settings">The program's settings.</param>
		/// <param name="logger">A spot to output logs.</param>
		public Dice(Settings settings, ILogger<Dice> logger)
		{
			_settings = settings;
			_logger = logger;
		}

		[Command]
		[Remarks("Accepts dice expressions that I might document one day. With no arguments, will roll a d20. No, you don't get advantage.")]
		[Summary("Rolls dice.")]
		[Priority(-1)]
		public async Task RollMainAsync(
			[Remainder, Summary("The dice expression to roll. Alternatively, may be a memorized roll in the pattern of `$rollName`, or an assignment to remember the roll in the pattern of `$rollName = 1d4`.")]
			string expression = "")
		{
			// check for no arguments
			if (string.IsNullOrWhiteSpace(expression))
			{
				await ReplyErrorAsync($"{nameof(Dice)} expects at least one parameter.");
				return;
			}

			// check for memorized roll expressions
			if (await CheckMemorizedRollAsync(expression))
			{
				// a memorized roll was handled
				return;
			}

			// no earlier cases handled, must be a normal roll expression!
			await RollDice(expression);
		}

		private async Task<bool> CheckMemorizedRollAsync(string expression)
		{
			// check for a memorized roll name
			if (expression.StartsWith("$"))
			{
				// check for an assignment or a use
				await TryHandleRememberDiceAsync(expression);
				return true;
			}

			return false;
		}

		private async Task TryHandleRememberDiceAsync(string expression)
		{
			Match matches = _variableNameCheck.Match(expression);

			string rollName = matches.Groups["name"].Value ?? string.Empty;
			bool isAssignment = matches.Groups["equals"].Value == "=";
			string assignExpression = matches.Groups["expression"].Value ?? string.Empty;

			// if it's not an assignment, just recall it.
			if (isAssignment == false)
			{
				// #nyi: recall rolls
				await ReplyErrorAsync($"NYI: Recalling roll: ${rollName}...");
				return;
			}

			// it is an assignment, make sure the expression isn't empty
			if (string.IsNullOrWhiteSpace(assignExpression))
			{
				await ReplyErrorAsync($"Memorized rolls need a value to remember.");
				return;
			}

			// make sure the expression is valid
			DiceExpression parsed = NumberStones.Dice.Parse(assignExpression);

			// #nyi remember the roll for later!
			await ReplyErrorAsync($"NYI: Storing ${rollName} as {parsed}");
		}

		private async Task RollDice(string expression)
		{
			DiceExpression diceExp = NumberStones.Dice.Parse(expression);
			DiceResult rollResult = diceExp.Roll();
			var embed = rollResult.ToEmbed(Context.Message);
			await Context.Message.ReplyAsync(embed: embed);
		}

		[Command("$")]
		[Remarks("We remember so you don't have to. You'll only see dice that you have access to.")]
		[Summary("Recalls a list of currently saved dice.")]
		public async Task RememberDiceAsync()
			=> await Context.Message.ReplyAsync($"Dice memory coming soon...");

		[Command("character")]
		[Alias("char", "dnd", "d&d", "stats")]
		[Remarks("You want a quick 5e stat block? You got a quick 5e stat block.")]
		[Summary("Rolls 4d6, dropping the lowest one, a total of 6 times. Instant character!")]
		public async Task RollCharacterAsync()
		{
			DiceExpression statRoll = NumberStones.Dice.Parse("4d6dl1");
			IEnumerable<DiceResult> results =
				from _ in Enumerable.Range(0, 6)
				select statRoll.Roll();

			Embed embed = results.ToCharacterEmbed(Context.Message);
			await Context.Message.ReplyAsync(embed: embed);
		}

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
