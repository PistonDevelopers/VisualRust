using System;
using System.Windows.Forms;

namespace VisualRust.Options
{
    public partial class RustOptionsPageControl : UserControl
    {
        public RustOptionsPageControl()
        {
            InitializeComponent();
        }

        private RustOptionsPage optionsPage;

        internal void Initialize(RustOptionsPage optionsPage)
        {
            this.optionsPage = optionsPage;

            //RacerPathTextBox.Text = optionsPage.RacerPath;
            //RustSrcFolderPathTextBox.Text = optionsPage.RustSourcePath;
        }

        private void SetRacerPathButton_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Selection Racer file";
            dialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            dialog.DefaultExt = ".exe";
            dialog.CheckFileExists = true;

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var filePath = dialog.FileName;
            //RacerPathTextBox.Text = filePath;
            optionsPage.RacerPath = filePath;
        }

        private void SetRustSrcFolderPathButton_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.Description = "Specify rust source folder";

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var folderPath = dialog.SelectedPath;
            //RustSrcFolderPathTextBox.Text = folderPath;
            optionsPage.RustSourcePath = folderPath;
        }

        private void RustSrcFolderPathTextBox_TextChanged(object sender, EventArgs e)
        {
            //optionsPage.RustSourcePath = RustSrcFolderPathTextBox.Text;
        }

        private void RacerPathTextBox_TextChanged(object sender, EventArgs e)
        {
            //optionsPage.RacerPath = RacerPathTextBox.Text;
        }
    }
}
