using System;
using System.IO;
using System.Windows.Forms;

namespace VisualRust.Options
{
    public partial class RustOptionsPageControl : UserControl
    {
        public RustOptionsPageControl()
        {
            InitializeComponent();
        }

        public void LoadSettings(RustOptionsPage optionsPage)
        {
            bundledRacer.Checked = !optionsPage.UseCustomRacer;
            customRacer.Checked = optionsPage.UseCustomRacer;

            switch (optionsPage.SourceType) {
                case RustOptionsPage.RustSource.EnvVariable:
                    envSource.Checked = true;
                    break;
                case RustOptionsPage.RustSource.Custom:
                    customSource.Checked = true;
                    break;
                default:
                    sysrootSource.Checked = true;
                    break;
            };

            customRacerPath.Text = optionsPage.CustomRacerPath;
            customSourcePath.Text = optionsPage.CustomSourcesPath;
            customRacer_CheckedChanged(null, null);
            customSource_CheckedChanged(null, null);
            string envSrcPath = Environment.GetEnvironmentVariable("RUST_SRC_PATH");
            if (!String.IsNullOrEmpty(envSrcPath))
                envSource.Text = String.Format("Read rust sources from enviroment variable RUST_SRC_PATH\n(current value: {0})", envSrcPath);
        }

        public void ApplySettings(RustOptionsPage optionsPage)
        {
            optionsPage.UseCustomRacer = !bundledRacer.Checked;
            optionsPage.CustomRacerPath = customRacerPath.Text;

            if (envSource.Checked)
                optionsPage.SourceType = RustOptionsPage.RustSource.EnvVariable;
            else if (customSource.Checked)
                optionsPage.SourceType = RustOptionsPage.RustSource.Custom;
            else
                optionsPage.SourceType = RustOptionsPage.RustSource.Sysroot;

            optionsPage.CustomSourcesPath = customSourcePath.Text;
        }

        private void OnCustomRacerPath_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Select Racer location";
            dialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            dialog.DefaultExt = ".exe";
            dialog.CheckFileExists = true;
            if (!string.IsNullOrEmpty(customRacerPath.Text))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(customRacerPath.Text);
                dialog.FileName = Path.GetFileName(customRacerPath.Text);
            }

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            customRacerPath.Text = dialog.FileName;
        }

        private void OnCustomSourcePath_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.Description = "Specify Rust source folder";

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            customSourcePath.Text = dialog.SelectedPath;
        }

        private void customRacer_CheckedChanged(object sender, EventArgs e)
        {
            customRacerPath.Enabled = customRacer.Checked;
            customRacerButton.Enabled = customRacer.Checked;
        }

        private void customSource_CheckedChanged(object sender, EventArgs e)
        {
            customSourcePath.Enabled = customSource.Checked;
            customSourceButton.Enabled = customSource.Checked;
        }
    }
}
