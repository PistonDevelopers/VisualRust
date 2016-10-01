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
            defaultGdb.CheckedChanged += OnDefaultGdb_CheckedChanged;
            customGdb.CheckedChanged += OnCustomGdb_CheckedChanged;
            customGdbButton.Click += OnCustomGdbButton_Click;
        }

        public DebuggingOptionsPageControl(DebuggingOptionsPage options)
            : this()
        {
            LoadSettings(options);
        }

        void OnDefaultGdb_CheckedChanged(object sender, EventArgs e)
        {
            customGdbPath.Enabled = !defaultGdb.Checked;
            customGdbButton.Enabled = !defaultGdb.Checked;
        }

        void OnCustomGdb_CheckedChanged(object sender, EventArgs e)
        {
            customGdbPath.Enabled = customGdb.Checked;
            customGdbButton.Enabled = customGdb.Checked;
        }

        public void LoadSettings(DebuggingOptionsPage optionsPage)
        {
            defaultGdb.Checked = !optionsPage.UseCustomGdbPath;
            customGdb.Checked = optionsPage.UseCustomGdbPath;
            customGdbPath.Text = optionsPage.DebuggerLocation;
            extraArgs.Text = optionsPage.ExtraArgs;
        }

        public void ApplySettings(DebuggingOptionsPage optionsPage)
        {
            optionsPage.UseCustomGdbPath = customGdb.Checked;
            optionsPage.DebuggerLocation = customGdbPath.Text;
            optionsPage.ExtraArgs = extraArgs.Text;
        }

        private void OnCustomGdbButton_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Select debugger location";
            dialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            dialog.DefaultExt = ".exe";
            dialog.CheckFileExists = true;
            if (!string.IsNullOrEmpty(customGdbPath.Text))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(customGdbPath.Text);
                dialog.FileName = Path.GetFileName(customGdbPath.Text);
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                customGdbPath.Text = dialog.FileName;
            }
        }
    }
}
