using Microsoft.VisualStudioTools.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VisualRust.Shared;

namespace VisualRust.Project.Forms
{
    class BuildPropertyControl : UserControl
    {
        private string[] knownTargets;
        private Configuration.Build config;
        private TableLayoutPanel mainPanel;
        private ComboBox customTargetBox;
        private ComboBox optimizationBox;
        private CheckBox lto;
        private CheckBox emitDebug;

        public BuildPropertyControl()
        {
            knownTargets = new string[] { Shared.Environment.DefaultTarget }.Union(Shared.Environment.FindInstalledTargets()).ToArray();
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

        private int AddPlatformTargetSection(TableLayoutPanel mainPanel, int startingRow)
        {
            TableLayoutPanel platformHeader = Utils.CreateHeaderLabel("Target");
            mainPanel.Controls.Add(platformHeader);
            mainPanel.SetColumnSpan(platformHeader, 2);
            mainPanel.Controls.Add(Utils.CreateLabel("Platform target:", Utils.Paddding().Left().Top().Bottom()));
            customTargetBox = Utils.CreateComboBox(knownTargets, Utils.Paddding().Top().Bottom());
            customTargetBox.DropDownStyle = ComboBoxStyle.DropDown;
            customTargetBox.Width =(int)(customTargetBox.Width * 1.5);
            mainPanel.Controls.Add(customTargetBox);
            return startingRow + 2;
        }

        private int AddOutputSection(TableLayoutPanel mainPanel, int startingRow)
        {
            TableLayoutPanel outputHeader = Utils.CreateHeaderLabel("Compilation");
            mainPanel.Controls.Add(outputHeader);
            mainPanel.SetColumnSpan(outputHeader, 2);
            mainPanel.Controls.Add(Utils.CreateLabel("Optimization level:", Utils.Paddding().Left().Top()));
            optimizationBox = Utils.CreateComboBox(
                new string[] 
                {
                    OptimizationLevel.O0.ToDisplayString(),
                    OptimizationLevel.O1.ToDisplayString(),
                    OptimizationLevel.O2.ToDisplayString(),
                    OptimizationLevel.O3.ToDisplayString(),
                },
                Utils.Paddding().Top());
            mainPanel.Controls.Add(optimizationBox);
            lto = Utils.CreateCheckBox("Apply link-time optimization", Utils.Paddding().Left());
            mainPanel.Controls.Add(lto);
            mainPanel.SetColumnSpan(lto, 2);
            emitDebug = Utils.CreateCheckBox("Emit debug info", Utils.Paddding().Left().Bottom());
            mainPanel.Controls.Add(emitDebug);
            mainPanel.SetColumnSpan(emitDebug, 2);
            return startingRow + 4;
        }

        public void LoadSettings(CommonProjectNode node)
        {
            config = Configuration.Build.LoadFrom(node);
            customTargetBox.Text = config.PlatformTarget;
            customTargetBox.TextChanged += (src,arg) => config.PlatformTarget = customTargetBox.Text;
            optimizationBox.SelectedIndex = (int)config.OptimizationLevel;
            optimizationBox.SelectedIndexChanged += (src,arg) => config.OptimizationLevel = (OptimizationLevel)optimizationBox.SelectedIndex;
            lto.Checked = config.LTO;
            lto.CheckedChanged += (src,arg) => config.LTO = lto.Checked;
            emitDebug.Checked = config.EmitDebug;
            emitDebug.CheckedChanged += (src,arg) => config.EmitDebug = emitDebug.Checked;
        }
    }
}
