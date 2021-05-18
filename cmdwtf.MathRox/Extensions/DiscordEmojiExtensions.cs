
using cmdwtf.MathRox.Configuration;

using Microsoft.Extensions.DependencyInjection;

namespace cmdwtf.MathRox.Extensions
{
	public static class DiscordEmojiExtensions
	{
		private static Settings _settings = null;

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
	}
}
