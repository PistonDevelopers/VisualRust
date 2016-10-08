using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VisualRust.Shared
{
	/// <summary>
	/// Used to parse version strings (-v flag) from Rust tools (rustc and cargo)
	/// </summary>
	public struct ToolVersion
	{
		public uint Major { get; private set; }
		public uint Minor { get; private set; }
		public uint Patch { get; private set; }
		public string PreRelease { get; private set; }
		public string GitHash { get; private set; }
		public DateTime Date { get; private set; }

		private static Regex versionRegex = new Regex(@"(\w+) (\d+)\.(\d+)\.(\d+)(-(\w+))? \((\w+) (\d{4})-(\d{2})-(\d{2})\)", RegexOptions.Compiled);

		public static ToolVersion? Parse(string versionString)
		{
			var match = versionRegex.Match(versionString);
			if (!match.Success) return null;

			uint major, minor, patch;
			if (!uint.TryParse(match.Groups[2].Value, out major)) return null;
			if (!uint.TryParse(match.Groups[3].Value, out minor)) return null;
			if (!uint.TryParse(match.Groups[4].Value, out patch)) return null;
			string preRelease = match.Groups[6].Value;
			string gitHash = match.Groups[7].Value;
			int year, month, day;
			if (!int.TryParse(match.Groups[8].Value, out year)) return null;
			if (!int.TryParse(match.Groups[9].Value, out month)) return null;
			if (!int.TryParse(match.Groups[10].Value, out day)) return null;

			return new ToolVersion
			{
				Major = major,
				Minor = minor,
				Patch = patch,
				PreRelease = preRelease,
				GitHash = gitHash,
				Date = new DateTime(year, month, day)
			};
		}
	}
}
