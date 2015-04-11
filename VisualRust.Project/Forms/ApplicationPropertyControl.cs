using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VisualRust.Shared;

namespace VisualRust.Project.Forms
{
    class ApplicationPropertyControl : UserControl
    {
        private Configuration.Application config;
        private TableLayoutPanel mainPanel;
        TextBox crateBox;
        ComboBox typeComboBox;

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
            crateBox = Utils.CreateTextBox("", Utils.Paddding());
            crateBox.Width = 294;
            mainPanel.Controls.Add(crateBox);
            mainPanel.Controls.Add(Utils.CreateLabel("Output type:", Utils.Paddding()));
            typeComboBox = Utils.CreateComboBox(
                new[]
                {
                    BuildOutputType.Application.ToDisplayString(),
                    BuildOutputType.Library.ToDisplayString()
                },
                Utils.Paddding());
            typeComboBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            mainPanel.Controls.Add(typeComboBox);
            this.Controls.Add(mainPanel);
        }

        public void LoadSettings(CommonProjectNode node)
        {
            config = Configuration.Application.LoadFrom(node);
            crateBox.Text = config.CrateName;
            crateBox.TextChanged += (src, arg) => config.CrateName = crateBox.Text;
            typeComboBox.SelectedIndex = (int)config.OutputType;
            typeComboBox.SelectedIndexChanged += (src, arg) => config.OutputType = (BuildOutputType)typeComboBox.SelectedIndex;
        }
    }
}
