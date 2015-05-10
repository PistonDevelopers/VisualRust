using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using VisualRust.Racer;

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

        protected override IWin32Window Window
        {
            get
            {
                var page = new RustOptionsPageControl(this);
                return page;
            }
        }
    }
}