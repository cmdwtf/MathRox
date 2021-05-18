using System;
using System.Threading.Tasks;

using Discord.Commands;

namespace cmdwtf.MathRox.Access
{
	/// <summary>
	/// A parameter precondition that requires a parameter start with a specific string value.
	/// </summary>
	public class ParameterStartsWithAttribute : ParameterPreconditionAttribute
	{
		/// <summary>
		/// The value to check for.
		/// </summary>
		public string Expected { get; init; }

		/// <summary>
		/// Creates a ParamaterStartsWithAttribute with a specified value.
		/// </summary>
		/// <param name="expectedValue">The value expected for the parameter to start with</param>
		public ParameterStartsWithAttribute(string expectedValue)
		{
			Expected = expectedValue;
		}

		/// <inheritdoc cref="ParameterPreconditionAttribute.CheckPermissionsAsync(ICommandContext, ParameterInfo, object, IServiceProvider)"/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
		{
			if ($"{value}".StartsWith(Expected))
			{
				return PreconditionResult.FromSuccess();
			}

			return PreconditionResult.FromError($"Parameter must start with {Expected}");
		}
	}
}
