
using System.Collections.Generic;

using cmdwtf.MathRox.Configuration;

using Microsoft.Extensions.DependencyInjection;

namespace cmdwtf.MathRox.Extensions
{
	public static class DiscordEmojiExtensions
	{
		private static Settings _settings = null;

		/// <summary>
		/// Gets an emoji reference string from settings.
		/// </summary>
		/// <param name="s">The emoji to get.</param>
		/// <returns>The emoji reference string.</returns>
		public static string ToDiscordEmoji(this string s)
		{
			if (_settings == null)
			{
				_settings = Program._host.Services.GetService<Settings>();
			}

			if (_settings.Options.Emoji.ContainsKey(s))
			{
				return _settings.Options.Emoji[s];
			}

			return string.Empty;
		}

		/// <summary>
		/// Replaces emoji strings in a string with the actual emoji reference from the settings.
		/// </summary>
		/// <param name="s">The string to operate on.</param>
		/// <returns>The emojified string.</returns>
		public static string UseDiscordEmoji(this string s)
		{
			if (_settings == null)
			{
				_settings = Program._host.Services.GetService<Settings>();
			}

			string result = s;

			foreach (KeyValuePair<string, string> e in _settings.Options.Emoji)
			{
				result = result.Replace($":{e.Key}:", e.Value, System.StringComparison.OrdinalIgnoreCase);
			}

			return result;
		}
	}
}
