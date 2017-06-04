using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace VisualRust
{
    internal sealed class RustLanguage : LanguageService
    {
        private LanguagePreferences preferences;

        public override LanguagePreferences GetLanguagePreferences()
        {
            if (preferences == null)
            {
                preferences = new LanguagePreferences(this.Site, typeof(RustLanguage).GUID, this.Name);
                preferences.EnableCodeSense = true;
                preferences.EnableMatchBraces = true;
                preferences.EnableCommenting = true;
                preferences.EnableShowMatchingBrace = true;
                preferences.EnableMatchBracesAtCaret = true;
                preferences.HighlightMatchingBraceFlags = _HighlightMatchingBraceFlags.HMB_USERECTANGLEBRACES;
                preferences.LineNumbers = true;
                preferences.MaxErrorMessages = 100;
                preferences.AutoOutlining = false;
                preferences.MaxRegionTime = 2000;
                preferences.ShowNavigationBar = true;

                preferences.AutoListMembers = false;
                preferences.EnableQuickInfo = false;
                preferences.ParameterInformation = false;
                preferences.InsertTabs = false;
            }
            return preferences;
        }

        public override string GetFormatFilterList()
        {
            return "Rust File (*.rs)|*.rs";
        }

        public override IScanner GetScanner(IVsTextLines buffer)
        {
            return null;
        }

        public override string Name
        {
            get { return "Rust"; }
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            return null;
        }
    }
}
