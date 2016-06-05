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
    partial class OpenManifestErrorWindow : ChildWindow
    {
        public OpenManifestErrorWindow()
        {
            InitializeComponent();
        }

        public string FileName { get; private set; }
        public string Errors { get; private set; }
        public string NewManifest { get; private set; }
        public bool Reload { get; private set; }

        public OpenManifestErrorWindow(Window owner, string fileName, string[] errors) : base(owner)
        {
            InitializeComponent();
            FileName = fileName;
            Errors = String.Join(Environment.NewLine, errors);
            DataContext = this;
        }

        private void OnBrowse(object sender, RoutedEventArgs e)
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

        private void OnReload(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Reload = true;
            this.Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
