using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cmdwtf.MathRox.Configuration
{
	public class DiscordApiKeys
	{
		public string ClientID { get; set; } = string.Empty;
		public string Token { get; set; } = string.Empty;

		public bool KeysSet =>
			string.IsNullOrWhiteSpace(ClientID) == false
			&& string.IsNullOrWhiteSpace(Token) == false;

		public DiscordApiKeys() { }

		public DiscordApiKeys(string client_id, string token)
		{
			ClientID = client_id;
			Token = token;
		}
	}
}
