using System;

using Discord;

using Microsoft.Extensions.Logging;

namespace cmdwtf.MathRox.Extensions
{
	/// <summary>
	/// A few tools to help out with dealing with Discord logs.
	/// </summary>
	public static class DiscordLogExtensions
	{
		/// <summary>
		/// Gets a <see cref="Microsoft.Extensions.Logging.LogLevel"/> based
		/// on a similar <see cref="Discord.LogSeverity"/>
		/// </summary>
		/// <param name="severity">The severity to translate</param>
		/// <returns>The resultant log level</returns>
		public static LogLevel ToLogLevel(this LogSeverity severity)
		{
			LogLevel level = severity switch
			{
				LogSeverity.Critical => LogLevel.Critical,
				LogSeverity.Error => LogLevel.Error,
				LogSeverity.Warning => LogLevel.Warning,
				LogSeverity.Info => LogLevel.Information,
				LogSeverity.Verbose => LogLevel.Debug,
				LogSeverity.Debug => LogLevel.Trace,
				_ => throw new NotImplementedException(),
			};

			return level;
		}
	}
}
