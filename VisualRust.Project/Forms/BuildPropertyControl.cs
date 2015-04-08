using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisualRust.Project.Forms
{
    class BuildPropertyControl : UserControl
    {
        private Configuration.Build buildConfig;

        private TableLayoutPanel mainPanel;

        public BuildPropertyControl()
        {
            // This looks nonsensical but is required to avoid getting "Microsoft Sans Serif, 8.25pt"
            // http://stackoverflow.com/questions/297701/default-font-for-windows-forms-application
            this.Font = System.Drawing.SystemFonts.MessageBoxFont;
            mainPanel = new TableLayoutPanel
            {
                AutoSize = true,
                Width = 500,
                Height = 0,
                ColumnCount = 2,
            };
            TableLayoutPanel generalHeader = Utils.CreateHeaderLabel("General");
            mainPanel.Controls.Add(generalHeader);
            mainPanel.SetColumnSpan(generalHeader, 2);
            RadioButton defaultPlatform = Utils.CreateRadioButton("Use default platform target", Utils.Paddding().Left().Top());
            mainPanel.Controls.Add(defaultPlatform);
            RadioButton customPlatform = Utils.CreateRadioButton("Use custom target:", Utils.Paddding().Left().Bottom());
            mainPanel.Controls.Add(customPlatform);
            mainPanel.SetRow(customPlatform, 2);
            mainPanel.SetColumn(customPlatform, 0);
            TextBox customTargetTextBox = Utils.CreateTextBox("", Utils.Paddding().Bottom());
            mainPanel.Controls.Add(customTargetTextBox);
            mainPanel.SetRow(customTargetTextBox, 2);
            mainPanel.SetColumn(customTargetTextBox, 1);
            this.Controls.Add(mainPanel);
        }

        public void LoadSettings(CommonProjectNode node)
        {
            buildConfig = Configuration.Build.LoadFrom(node);
        }
    }
}
