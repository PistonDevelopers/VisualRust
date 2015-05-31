using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisualRust.Options
{
    public partial class DebuggingOptionsPageControl : UserControl
    {
        private DebuggingOptionsPage optionsPage;

        public DebuggingOptionsPageControl(DebuggingOptionsPage optionsPage)
        {
            this.optionsPage = optionsPage;
            InitializeComponent();

            debuggerLocation.Text = optionsPage.DebuggerLocation;
            debuggerLocation.TextChanged += (src, arg) => optionsPage.DebuggerLocation = debuggerLocation.Text;
            extraArgs.Text = optionsPage.ExtraArgs;
            extraArgs.TextChanged += (src, arg) => optionsPage.ExtraArgs = extraArgs.Text;
        }

        private void browseDebugger_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Select debugger location";
            dialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            dialog.FileName = debuggerLocation.Text;
            dialog.DefaultExt = ".exe";
            dialog.CheckFileExists = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                debuggerLocation.Text = dialog.FileName;
            }
        }
    }
}
