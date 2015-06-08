using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace VisualRust.Options
{
    [ComVisible(true)]
    [Guid("189B1BF9-0D6D-4325-A2BB-6BFE9DA3A92E")]
    public class RustOptionsPage : DialogPage
    {
        public bool UseCustomRacer { get; set; }
        public string CustomRacerPath { get; set; }

        public bool UseCustomSources { get; set; }
        public string CustomSourcesPath { get; set; }

        private RustOptionsPageControl _page;

        protected override IWin32Window Window
        {
            get
            {
                _page = new RustOptionsPageControl();
                _page.LoadSettings(this);
                return _page;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _page.LoadSettings(this);
            base.OnClosed(e);
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            if (e.ApplyBehavior == ApplyKind.Apply)
                _page.ApplySettings(this);
            base.OnApply(e);
        }
    }
}