using System;
using System.Windows.Forms;

namespace VisualRust.Options
{
    public partial class RustOptionsPageControl : UserControl
    {
        private RustOptionsPage optionsPage;

        public RustOptionsPageControl(RustOptionsPage optionsPage)
        {
            this.optionsPage = optionsPage;
            InitializeComponent();
            bundledRacer.Checked = !optionsPage.UseCustomRacer;
            customRacer.Checked = optionsPage.UseCustomRacer;
            envSource.Checked = !optionsPage.UseCustomSources;
            customSource.Checked = optionsPage.UseCustomSources;
            customRacerPath.Text = optionsPage.CustomRacerPath;
            customSourcePath.Text = optionsPage.CustomSourcesPath;
            customRacer_CheckedChanged(null, null);
            customSource_CheckedChanged(null, null);
            string envSrcPath = Environment.GetEnvironmentVariable("RUST_SRC_PATH");
            if(!String.IsNullOrEmpty(envSrcPath))
                envSource.Text = String.Format("Read rust sources from enviroment variable RUST_SRC_PATH\n(current value: {0})", envSrcPath);
        }

        private void OnCustomRacerPath_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Select Racer location";
            dialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            dialog.DefaultExt = ".exe";
            dialog.CheckFileExists = true;

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            customRacerPath.Text = dialog.FileName;
        }

        private void OnCustomRacerPath_TextChanged(object sender, EventArgs e)
        {
            optionsPage.CustomRacerPath = customRacerPath.Text;
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

        private void OnCustomSourcePath_TextChanged(object sender, EventArgs e)
        {
            optionsPage.CustomSourcesPath = customSourcePath.Text;
        }

        private void bundledRacer_CheckedChanged(object sender, EventArgs e)
        {
            optionsPage.UseCustomRacer = !bundledRacer.Checked;
        }

        private void customRacer_CheckedChanged(object sender, EventArgs e)
        {
            customRacerPath.Enabled = customRacer.Checked;
            customRacerButton.Enabled = customRacer.Checked;
        }

        private void envSource_CheckedChanged(object sender, EventArgs e)
        {
            optionsPage.UseCustomSources = !envSource.Checked;
        }

        private void customSource_CheckedChanged(object sender, EventArgs e)
        {
            customSourcePath.Enabled = customSource.Checked;
            customSourceButton.Enabled = customSource.Checked;
        }
    }
}
