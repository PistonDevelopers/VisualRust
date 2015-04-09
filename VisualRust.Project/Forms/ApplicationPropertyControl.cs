using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VisualRust.Project.Forms
{
    class ApplicationPropertyControl : UserControl
    {
        private Configuration.Application config;
        private TableLayoutPanel mainPanel;

        public ApplicationPropertyControl()
        {
            this.Font = System.Drawing.SystemFonts.MessageBoxFont;
            mainPanel = new TableLayoutPanel
            {
                AutoSize = true,
                Width = 0,
                Height = 0,
                ColumnCount = 1,
            };
            mainPanel.Controls.Add(Utils.CreateLabel("Crate name:", Utils.Paddding()));
            TextBox crateBox = Utils.CreateTextBox("", Utils.Paddding());
            crateBox.Width = 294;
            mainPanel.Controls.Add(crateBox);
            mainPanel.Controls.Add(Utils.CreateLabel("Output type:", Utils.Paddding()));
            ComboBox typeComboBox = Utils.CreateComboBox(new[] { "Application", "Library"}, Utils.Paddding());
            typeComboBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            mainPanel.Controls.Add(typeComboBox);
            this.Controls.Add(mainPanel);
        }

        public void LoadSettings(CommonProjectNode node)
        {
            config = Configuration.Application.LoadFrom(node);
        }
    }
}
