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
        GroupBox libraryGroup;
        CheckBox buildRlib;
        CheckBox buildStaticlib;
        CheckBox buildDylib;

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
            AddLibraryTypeBox();
            this.Controls.Add(mainPanel);
        }

        private void AddLibraryTypeBox()
        {
            libraryGroup = new GroupBox()
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                AutoSize = true,
                Margin = Utils.Paddding(),
                Text = "Library Type"
            };
            TableLayoutPanel panel = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                Margin = new Padding(),
                RowCount = 3,
                ColumnCount = 1,
            };
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Padding topMargin = Utils.Paddding();
            topMargin.Top +=7;
            topMargin.Left +=7;
            buildDylib = Utils.CreateCheckBox("Build dynamic library", topMargin);
            panel.Controls.Add(buildDylib);
            Padding midMargin = Utils.Paddding();
            midMargin.Left +=7;
            buildStaticlib = Utils.CreateCheckBox("Build static library", midMargin);
            panel.Controls.Add(buildStaticlib);
            Padding botMargin = Utils.Paddding();
            botMargin.Left +=7;
            botMargin.Bottom +=7;
            buildRlib = Utils.CreateCheckBox("Build Rust library", botMargin);
            panel.Controls.Add(buildRlib);
            libraryGroup.Controls.Add(panel);
            mainPanel.Controls.Add(libraryGroup);
        }

        public void LoadSettings(CommonProjectNode node)
        {
            originalConfig = Configuration.Application.LoadFrom(node);
            config = originalConfig.Clone();
            crateBox.Text = config.CrateName;
            crateBox.TextChanged += (src, arg) => config.CrateName = crateBox.Text;
            typeComboBox.SelectedIndex = (int)config.OutputType;
            libraryGroup.Enabled = config.OutputType == BuildOutputType.Library;
            typeComboBox.SelectedIndexChanged += (src, arg) =>
            {
                config.OutputType = (BuildOutputType)typeComboBox.SelectedIndex;
                libraryGroup.Enabled = config.OutputType == BuildOutputType.Library;
            };
            buildDylib.Checked = config.BuildDylib;
            buildDylib.CheckedChanged += (src, arg) => config.BuildDylib = buildDylib.Checked;
            buildStaticlib.Checked = config.BuildStaticlib;
            buildStaticlib.CheckedChanged += (src, arg) => config.BuildStaticlib = buildStaticlib.Checked;
            buildRlib.Checked = config.BuildRlib;
            buildRlib.CheckedChanged += (src, arg) => config.BuildRlib = buildRlib.Checked;
            MakeSureAtLeastOneLibraryTypeIsSelected();
            config.Changed += (src, arg) => isDirty(config.HasChangedFrom(originalConfig));
        }

        private void MakeSureAtLeastOneLibraryTypeIsSelected()
        {
            if (config.OutputType == BuildOutputType.Library && !config.BuildDylib && !config.BuildRlib && !config.BuildStaticlib)
                config.BuildRlib = true;
        }

        public bool ApplyConfig(CommonProjectNode node)
        {
            if (config.OutputType == BuildOutputType.Library && !config.BuildDylib && !config.BuildRlib && !config.BuildStaticlib)
            {
                MessageBox.Show("Can't build a library project with no library type.", "Visual Rust");
                return false;
            }
            config.SaveTo(node);
            originalConfig = config.Clone();
            return true;
        }
    }
}
