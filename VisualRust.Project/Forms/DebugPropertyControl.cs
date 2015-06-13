using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;

namespace VisualRust.Project.Forms
{
    partial class DebugPropertyControl : UserControl
    {
        private Action<bool> isDirty;
        private Configuration.Debug originalConfig;
        private Configuration.Debug config;

        public DebugPropertyControl(Action<bool> isDirtyAction)
        {
            isDirty = isDirtyAction;
            this.Font = System.Drawing.SystemFonts.MessageBoxFont;
            InitializeComponent();
        }

        public void LoadSettings(ProjectConfig[] configs)
        {
            originalConfig = Configuration.Debug.LoadFrom(configs);
            config = originalConfig.Clone();

            radioButton1.CheckedChanged += (src, arg) => {
                if (radioButton1.Checked)
                    config.StartAction = Configuration.StartAction.Project;
                else
                    config.StartAction = Configuration.StartAction.Program;
                externalProg.Enabled = !radioButton1.Checked;
                browseProg.Enabled = !radioButton1.Checked;
            };
            radioButton1.Checked = config.StartAction == Configuration.StartAction.Project;
            radioButton2.Checked = !radioButton1.Checked;

            externalProg.Text = config.ExternalProgram;
            externalProg.TextChanged += (src, arg) => config.ExternalProgram = externalProg.Text;

            commandLineArgs.Text = config.CommandLineArgs;
            commandLineArgs.TextChanged += (src, arg) => config.CommandLineArgs = commandLineArgs.Text;

            workDir.Text = config.WorkingDir;
            workDir.TextChanged += (src, arg) => config.WorkingDir = workDir.Text;

            debuggerScript.Text = config.DebuggerScript;
            debuggerScript.TextChanged += (src, erg) => config.DebuggerScript = debuggerScript.Text;

            config.Changed += (src, arg) => isDirty(config.HasChangedFrom(originalConfig));
        }

        public void ApplyConfig(ProjectConfig[] configs)
        {
            config.SaveTo(configs);
            originalConfig = config.Clone();
        }

        private void browseProg_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Select program";
            dialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            dialog.DefaultExt = ".exe";
            dialog.CheckFileExists = true;
            if (!string.IsNullOrEmpty(externalProg.Text))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(externalProg.Text);
                dialog.FileName = Path.GetFileName(externalProg.Text);
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                externalProg.Text = dialog.FileName;
            }
        }

        private void browseWorkDir_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.SelectedPath = workDir.Text;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                workDir.Text = dialog.SelectedPath;
            }
        }

        private void commandLineArgs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
            }
        }

        private void gdbReference_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = "https://sourceware.org/gdb/onlinedocs/gdb/Command-and-Variable-Index.html";

            if ((Control.ModifierKeys & Keys.Control) == Keys.Control || e.Button == MouseButtons.Middle)
            {
                Process.Start(url);
            }
            else
            {
                var service = (IVsWebBrowsingService)Package.GetGlobalService(typeof(IVsWebBrowsingService));
                IVsWindowFrame frame;
                service.Navigate(url, 0, out frame);
            }
        }
    }
}
