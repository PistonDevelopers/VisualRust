using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace VisualRust.Shared.Message
{
    // TODO: probably we should just get rid of this
    public static class RustcMessageHumanParser
    {
        private static readonly Regex defectRegex = new Regex(@"^([^\n:]+):(\d+):(\d+):\s+(\d+):(\d+)\s+(.*)$", RegexOptions.Multiline | RegexOptions.CultureInvariant);

        private static readonly Regex errorCodeRegex = new Regex(@"\[([A-Z]\d\d\d\d)\]$", RegexOptions.CultureInvariant);

        public static IEnumerable<RustcMessageHuman> Parse(string output)
        {
            MatchCollection errorMatches = defectRegex.Matches(output);

            RustcMessageHuman previous = null;
            foreach (Match match in errorMatches)
            {
                string remainingMsg = match.Groups[6].Value.Trim();
                Match errorMatch = errorCodeRegex.Match(remainingMsg);
                string errorCode = errorMatch.Success ? errorMatch.Groups[1].Value : null;
                int line = Int32.Parse(match.Groups[2].Value, NumberStyles.None);
                int col = Int32.Parse(match.Groups[3].Value, NumberStyles.None);
                int endLine = Int32.Parse(match.Groups[4].Value, NumberStyles.None);
                int endCol = Int32.Parse(match.Groups[5].Value, NumberStyles.None);

                if (remainingMsg.StartsWith("warning: "))
                {
                    string msg = match.Groups[6].Value.Substring(9, match.Groups[6].Value.Length - 9 - (errorCode != null ? 8 : 0));
                    if (previous != null) yield return previous;
                    previous = new RustcMessageHuman(RustcMessageType.Warning, msg, errorCode, match.Groups[1].Value,
                        line, col, endLine, endCol);
                }
                else if (remainingMsg.StartsWith("note: ") || remainingMsg.StartsWith("help: "))
                {
                    if (remainingMsg.StartsWith("help: pass `--explain ") && previous != null)
                    {
                        previous.CanExplain = true;
                        continue;
                    }

                    // NOTE: "note: " and "help: " are both 6 characters long (though hardcoding this is probably still not a very good idea)
                    string msg = remainingMsg.Substring(6, remainingMsg.Length - 6 - (errorCode != null ? 8 : 0));
                    var type = remainingMsg.StartsWith("note: ") ? RustcMessageType.Note : RustcMessageType.Help;
                    RustcMessageHuman note = new RustcMessageHuman(type, msg, errorCode, match.Groups[1].Value,
                        line, col, endLine, endCol);

                    if (previous != null)
                    {
                        // try to merge notes and help messages with a previous message (warning or error where it belongs to), if the span is the same
                        if (previous.TryMergeWithFollowing(note))
                        {
                            continue; // skip setting new previous, because we successfully merged the new note into the previous message
                        }
                        else
                        {
                            yield return previous;
                        }
                    }
                    previous = note;
                }
                else
                {
                    bool startsWithError = remainingMsg.StartsWith("error: ");
                    string msg = remainingMsg.Substring((startsWithError ? 7 : 0), remainingMsg.Length - (startsWithError ? 7 : 0) - (errorCode != null ? 8 : 0));
                    if (previous != null) yield return previous;
                    previous = new RustcMessageHuman(RustcMessageType.Error, msg, errorCode, match.Groups[1].Value,
                        line, col, endLine, endCol);
                }
            }

            if (previous != null) yield return previous;
        }
    }
}
