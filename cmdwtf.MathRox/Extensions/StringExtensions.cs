using System;

namespace cmdwtf.MathRox.Extensions
{
	/// <summary>
	/// An extension class for a few string operations used through out the commands.
	/// </summary>
	public static class StringExtensions
	{
		private static readonly Random _rand = new();
		public static string GetRandom(this string[] arr)
			=> arr[_rand.Next(arr.Length)];

		public static string Clip(string value, int maxLength)
		{
			if (value.Length > maxLength)
			{
				return value.Substring(0, maxLength - 3) + "...";
			}

			return value;
		}
	}
}
