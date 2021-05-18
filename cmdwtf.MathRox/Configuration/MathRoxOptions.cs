using System.Collections.Generic;

namespace cmdwtf.MathRox.Configuration
{
	/// <summary>
	/// Options related to the operation of the bot.
	/// </summary>
	public class MathRoxOptions
	{
		public Dictionary<string, string> Emoji = new()
		{
			// result
			{ "Pass", "<:Pass:843844391663960134>" },
			{ "Fail", "<:Fail:843844391679950928>" },
			{ "Nothing", "<:Nothing:843844391697252382>" },

			// application
			{ "NumberStonesW", "<:NumberStonesW:843844044781125632>" },
			{ "NumberStonesB", "<:NumberStonesB:843844044660670505>" },

			// other
			{ "ggstare", "<:ggstare:230180638882267136>" },
			{ "gold", "<:gold:280485596420505600>" },
		};
	}
}
