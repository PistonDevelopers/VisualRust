using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace VisualRust.Options
{
    [ComVisible(true)]
    [Guid("93F42A39-0AF6-40EE-AE2C-1C44AB5F8B15")]
    public partial class DebuggingOptionsPage : DialogPage
    {
        public string DebuggerLocation { get; set; }
        public string ExtraArgs { get; set; }

        private DebuggingOptionsPageControl _page;

        protected override IWin32Window Window
        {
            get
            {
                _page = new DebuggingOptionsPageControl();
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
