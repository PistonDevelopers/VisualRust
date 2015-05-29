using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
                    config.StartAction = Configuration.StartAction.ExternalProgram;
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
            dialog.Title = "Selectprogram";
            dialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            dialog.FileName = externalProg.Text;
            dialog.DefaultExt = ".exe";
            dialog.CheckFileExists = true;

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
    }
}
