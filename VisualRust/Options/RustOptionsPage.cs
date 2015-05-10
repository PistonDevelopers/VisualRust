using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using VisualRust.Racer;

namespace VisualRust.Options
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("189B1BF9-0D6D-4325-A2BB-6BFE9DA3A92E")]
    public class RustOptionsPage : DialogPage
    {
        [Category("General")]
        [DisplayName("Path to Racer.exe")]
        [Description("Specify the path to folder with Racer.exe")]
        public string RacerPath { get; set; }

        [Category("General")]
        [DisplayName("Path to sources of rust")]
        [Description("Specify the path to folder with sources of rust")]
        public string RustSourcePath { get; set; }

        public override void LoadSettingsFromStorage()
        {
            var autoCompleter = AutoCompleter.Instance;
            RacerPath = autoCompleter.RacerPath;
            RustSourcePath = autoCompleter.RustSrcPath;
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            var autoCompleter = AutoCompleter.Instance;
            autoCompleter.RustSrcPath = RustSourcePath;
            autoCompleter.RacerPath = RacerPath;
        }

        protected override IWin32Window Window
        {
            get
            {
                var page = new RustOptionsPageControl();
                page.Initialize(this);
                return page;
            }
        }
    }
}