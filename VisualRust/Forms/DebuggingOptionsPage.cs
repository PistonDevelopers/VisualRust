using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace VisualRust.Options
{
    [ComVisible(true)]
    public partial class DebuggingOptionsPage : DialogPage
    {
        public string DebuggerLocation { get; set; }
        public string ExtraArgs { get; set; }

        protected override IWin32Window Window
        {
            get
            {
                var page = new DebuggingOptionsPageControl(this);
                return page;
            }
        }
    }
}
