using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VisualRust.Project.Controls
{
    public partial class OpenManifestErrorWindow : Window
    {
        public OpenManifestErrorWindow()
        {
            InitializeComponent();
        }

        public string FileName { get; private set; }
        public string Errors { get; private set; }
        public string NewManifest { get; private set; }

        public OpenManifestErrorWindow(Window owner, string fileName, string[] errors) : this()
        {
            Owner = owner;
            FileName = fileName;
            Errors = String.Join(Environment.NewLine, errors);
            DataContext = this;
        }

        private void Browse(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog()
            {
                Filter = "TOML files|*.toml|All files|*.*",
                InitialDirectory = System.IO.Path.GetDirectoryName(FileName),
            };
            bool? result = openFile.ShowDialog();
            if (result == true)
            {
                NewManifest = openFile.FileName;
                DialogResult = true;
                this.Close();
            }
        }

        private void Reload(object sender, RoutedEventArgs e)
        {
            DialogResult = null;
            this.Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
