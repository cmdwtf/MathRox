using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cmdwtf.MathRox.Configuration
{
	public class ApiKeyCollection
	{
		public DiscordApiKeys Discord { get; set; } = new();
		public bool KeysSet => Discord.KeysSet;
	}
}
