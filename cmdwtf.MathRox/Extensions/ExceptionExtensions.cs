using System;

namespace cmdwtf.MathRox.Extensions
{
	public static class ExceptionExtensions
	{
		public static string GetCallingSite(this Exception ex)
			=> ex.StackTrace.Remove(ex.StackTrace.IndexOf("\r\n")).Trim();
	}
}
