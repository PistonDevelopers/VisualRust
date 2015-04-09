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
            int generalSectionRows =  AddPlatformTargetSection(mainPanel, 0);
            AddOutputSection(mainPanel, generalSectionRows);
            this.Controls.Add(mainPanel);
        }

        private static int AddPlatformTargetSection(TableLayoutPanel mainPanel, int startingRow)
        {
            TableLayoutPanel platformHeader = Utils.CreateHeaderLabel("Platform target");
            mainPanel.Controls.Add(platformHeader);
            mainPanel.SetColumnSpan(platformHeader, 2);
            RadioButton defaultPlatform = Utils.CreateRadioButton("Use default platform target", Utils.Paddding().Left().Top());
            mainPanel.Controls.Add(defaultPlatform);
            RadioButton customPlatform = Utils.CreateRadioButton("Use custom target:", Utils.Paddding().Left().Bottom());
            mainPanel.Controls.Add(customPlatform);
            mainPanel.SetRow(customPlatform, startingRow + 2);
            mainPanel.SetColumn(customPlatform, 0);
            TextBox customTargetTextBox = Utils.CreateTextBox("", Utils.Paddding().Bottom());
            mainPanel.Controls.Add(customTargetTextBox);
            mainPanel.SetRow(customTargetTextBox, startingRow + 2);
            mainPanel.SetColumn(customTargetTextBox, 1);
            return startingRow + 3;
        }

        private static int AddOutputSection(TableLayoutPanel mainPanel, int startingRow)
        {
            TableLayoutPanel outputHeader = Utils.CreateHeaderLabel("Compilation");
            mainPanel.Controls.Add(outputHeader);
            mainPanel.SetColumnSpan(outputHeader, 2);
            mainPanel.Controls.Add(Utils.CreateLabel("Optimization level:", Utils.Paddding().Left().Top()));
            mainPanel.Controls.Add(Utils.CreateComboBox(new string[] { "none (O0)", "minimal (O1)", "optimized (O2)", "aggresive (O3)" }, Utils.Paddding().Top()));
            CheckBox lto = Utils.CreateCheckBox("Apply link-time optimization", Utils.Paddding().Left());
            mainPanel.Controls.Add(lto);
            mainPanel.SetColumnSpan(lto, 2);
            mainPanel.Controls.Add(Utils.CreateLabel("Debug information:", Utils.Paddding().Left().Bottom()));
            mainPanel.Controls.Add(Utils.CreateComboBox(new string[] { "none", "line numbers", "full" }, Utils.Paddding().Bottom()));
            return startingRow + 4;
        }

        public void LoadSettings(CommonProjectNode node)
        {
            buildConfig = Configuration.Build.LoadFrom(node);
        }
    }
}
