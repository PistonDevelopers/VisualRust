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
        private Action<bool> isDirty;
        private Configuration.Application config;
        private Configuration.Application originalConfig;
        private TableLayoutPanel mainPanel;
        TextBox crateBox;
        ComboBox typeComboBox;

        public ApplicationPropertyControl(Action<bool> isDirtyAction)
        {
            isDirty = isDirtyAction;
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
            originalConfig = Configuration.Application.LoadFrom(node);
            config = originalConfig.Clone();
            crateBox.Text = config.CrateName;
            crateBox.TextChanged += (src, arg) => config.CrateName = crateBox.Text;
            typeComboBox.SelectedIndex = (int)config.OutputType;
            typeComboBox.SelectedIndexChanged += (src, arg) => config.OutputType = (BuildOutputType)typeComboBox.SelectedIndex;
            config.Changed += (src, arg) => isDirty(config.HasChangedFrom(originalConfig));
        }

        public void ApplyConfig(CommonProjectNode node)
        {
            config.SaveTo(node);
            originalConfig = config.Clone();
        }
    }
}
