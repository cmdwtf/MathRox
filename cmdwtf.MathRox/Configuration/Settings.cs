namespace cmdwtf.MathRox.Configuration
{
	public class Settings
	{
		public string[] Prefixes { get; set; } = new string[] { "`" };
		public ApiKeyCollection ApiKeys { get; set; } = new();
		public MathRoxOptions Options { get; set; } = new();
		public ulong[] Owners { get; set; } = { 116026533688115204 };

		public bool HasRequiredSettings
			=> ApiKeys.KeysSet;
	}
}
