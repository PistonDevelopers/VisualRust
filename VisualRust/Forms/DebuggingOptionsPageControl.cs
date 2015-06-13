using System;
using System.IO;
using System.Windows.Forms;

namespace VisualRust.Options
{
    public partial class DebuggingOptionsPageControl : UserControl
    {
        public DebuggingOptionsPageControl()
        {
            InitializeComponent();
        }

        public void LoadSettings(DebuggingOptionsPage optionsPage)
        {
            debuggerLocation.Text = optionsPage.DebuggerLocation;
            extraArgs.Text = optionsPage.ExtraArgs;
        }

        public void ApplySettings(DebuggingOptionsPage optionsPage)
        {
            optionsPage.DebuggerLocation = debuggerLocation.Text;
            optionsPage.ExtraArgs = extraArgs.Text;
        }

        private void browseDebugger_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Select debugger location";
            dialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            dialog.DefaultExt = ".exe";
            dialog.CheckFileExists = true;
            if (!string.IsNullOrEmpty(debuggerLocation.Text))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(debuggerLocation.Text);
                dialog.FileName = Path.GetFileName(debuggerLocation.Text);
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                debuggerLocation.Text = dialog.FileName;
            }
        }
    }
}
