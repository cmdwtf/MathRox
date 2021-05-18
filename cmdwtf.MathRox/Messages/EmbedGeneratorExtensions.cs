using System;
using System.Collections.Generic;
using System.Linq;

using cmdwtf.MathRox.Extensions;
using cmdwtf.NumberStones;
using cmdwtf.NumberStones.DiceTypes;
using cmdwtf.NumberStones.Expression;

using Discord;
using Discord.WebSocket;

namespace cmdwtf.MathRox.Messages
{
	public static class EmbedGeneratorExtensions
	{
		private const string _defaultIconUrl = "https://cdn.discordapp.com/embed/avatars/0.png";
		private const string _defaultUrl = "https://cmd.wtf/mathrox";

		public static Color DefaultColor { get; } = Color.DarkBlue;
		public static Color UnknownColor { get; } = Color.DarkMagenta;
		public static Color CritSuccessColor { get; } = Color.Green;
		public static Color CritFailureColor { get; } = Color.DarkRed;

		public static Color GetDiscordColor(this DiceResult rollResult)
		{
			if (rollResult.Results is DiceExpressionResult der)
			{
				return der.GetDiscordColor();
			}

			return UnknownColor;
		}

		public static Color GetDiscordColor(this DiceExpressionResult der)
		{
			if (der.CriticalSuccess)
			{
				return CritSuccessColor;
			}
			else if (der.CriticalFailure)
			{
				return CritFailureColor;
			}

			return DefaultColor;
		}

		public static string GetDiceVerb(this DiceResult rollResult)
		{
			if (rollResult.Results is DiceExpressionResult der)
			{
				if (der.Success)
				{
					return "succeeded with";
				}

				if (der.Failure)
				{
					return "failed with";
				}

				if (der.CriticalSuccess)
				{
					return @"\*\*CRIT\*\*";
				}

				if (der.CriticalFailure)
				{
					return @"\*\*oof'd\*\*";
				}
			}

			return "rolled";
		}

		private static EmbedBuilder GetDefaultBuilder(this DiceResult rollResult)
		{
			return new()
			{
				Footer = new()
				{
					IconUrl = _defaultIconUrl,
					Text = $" <more roll info soon> | MathRox"
				},
				Timestamp = DateTimeOffset.Now,
				Url = _defaultUrl,
				ThumbnailUrl = _defaultIconUrl,
				//Author = new()
				//{
				//	Name = $"{msg.Author.Username}",
				//	Url = _defaultUrl,
				//	IconUrl = _defaultIconUrl
				//},
			};
		}

		public static Embed ToEmbed(this DiceResult rollResult, SocketUserMessage msg)
		{
			EmbedBuilder builder = rollResult.GetDefaultBuilder();
			builder.Color = rollResult.GetDiscordColor();
			builder.Title = "Roll Result";
			builder.Description = $"{msg.Author.Mention} {rollResult.GetDiceVerb()}: **{rollResult.Value}**";

			string details = rollResult.Results.ToString();

			if (rollResult.Results is DiceExpressionResult der)
			{
				if (der.Type == DiceType.Fudge)
				{
					if (der.HasMultipleTermResults &&
						der is MultipleDiceTermResult mdtr)
					{
						details = string.Empty;
						foreach (DiceExpressionResult r in mdtr.SubResults)
						{
							if (r.Type != DiceType.Fudge)
							{
								continue;
							}

							details += (r as FudgeDiceExpressionResult).Result switch
							{
								FudgeResult.Nothing => "Nothing".ToDiscordEmoji(),
								FudgeResult.Plus => "Pass".ToDiscordEmoji(),
								FudgeResult.Minus => "Fail".ToDiscordEmoji(),
								_ => "?",
							};
						}
					}
					else
					{
						details = (der as FudgeDiceExpressionResult).Result switch
						{
							FudgeResult.Nothing => "Nothing".ToDiscordEmoji(),
							FudgeResult.Plus => "Pass".ToDiscordEmoji(),
							FudgeResult.Minus => "Fail".ToDiscordEmoji(),
							_ => "?",
						};
					}
				}
			}

			builder.Fields = new()
			{
				new()
				{
					Name = "Roll",
					Value = $"{rollResult.Expression}"
							.Clip(64),
					IsInline = true
				},
				new()
				{
					Name = "Detailed Results",
					Value = $"{details}"
							.Clip(256),
					IsInline = false
				}
			};
			return builder.Build();
		}

		private static readonly List<string> _stats = new()
		{
			"STR",
			"DEX",
			"CON",
			"INT",
			"WIS",
			"CHA"
		};

		public static Embed ToCharacterEmbed(this IEnumerable<DiceResult> rollResults, SocketUserMessage msg)
		{
			int count = rollResults.Count();
			if (count < _stats.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(rollResults), "We need at least 6 results to embed a character.");
			}

			EmbedBuilder builder = rollResults.First().GetDefaultBuilder();
			builder.Color = DefaultColor; // class color maybe?
			builder.Title = $"{msg.Author.Username}'s Character";
			builder.Description = $"Your total stat count: {rollResults.Sum(r => r.Value)}";

			List<EmbedFieldBuilder> fields = new();
			IEnumerator<DiceResult> rollEnum = rollResults.GetEnumerator();
			rollEnum.MoveNext();

			foreach (string stat in _stats)
			{
				DiceResult statResult = rollEnum.Current;
				int statScore = (int)statResult.Value;
				int statMod = (statScore / 2) - 5;

				fields.Add(new()
				{
					Name = $"{stat}: {statScore} (**{statMod}**)",
					Value = statResult.ToString(),
					IsInline = true
				});

				rollEnum.MoveNext();
			}

			builder.Fields = fields;

			return builder.Build();
		}
	}
}
