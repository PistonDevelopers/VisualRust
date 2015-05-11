using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisualRust.Project.Forms
{
    public static class Utils
    {
        public static TableLayoutPanel CreateHeaderLabel(string text)
        {
            var panel = new TableLayoutPanel
            {
                RowCount = 1,
                ColumnCount = 2,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            panel.Controls.Add(
                new Label
                {
                    Anchor = AnchorStyles.Left,
                    AutoSize = true,
                    Margin = new Padding(0, 0, 3, 0),
                    Text = text
                });
            panel.Controls.Add(
                new Label
                {
                    AccessibleRole = System.Windows.Forms.AccessibleRole.Separator,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    BackColor = System.Drawing.SystemColors.ControlDark,
                    Margin = new Padding(3, 0, 0, 0),
                    Height = 1
                });
            return panel;
        }

        public static RadioButton CreateRadioButton(string text, Padding margin)
        {
            return new RadioButton
            {
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Text = text,
                Margin = margin
            };
        }

        public static Label CreateLabel(string text, Padding margin)
        {
            return new Label
            {
                Anchor = AnchorStyles.Left,
                AutoSize = true,
                Margin = margin,
                Text = text
            };
        }

        public static CheckBox CreateCheckBox(string text, Padding margin)
        {
            return new CheckBox
            {
                Anchor = AnchorStyles.Left,
                AutoSize = true,
                Margin = margin,
                Text = text
            };
        }

        public static ComboBox CreateComboBox(string[] text, Padding margin)
        {
            var box = new ComboBox
            {
                Anchor = AnchorStyles.Left,
                AutoSize = true,
                Margin = margin,
                DropDownStyle =  ComboBoxStyle.DropDownList
            };
            box.Items.AddRange(text);
            return box;
        }

        public static TextBox CreateTextBox(string text, Padding margin)
        {
            return new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                AutoSize = true,
                Margin = margin
            };
        }

        public static Padding Paddding()
        {
            return new Padding(3,4,3,4);
        }

        public static Padding Left(this Padding pad)
        {
            pad.Left += 22;
            return pad;
        }

        public static Padding Top(this Padding pad)
        {
            pad.Top += 10;
            return pad;
        }

        public static Padding Bottom(this Padding pad)
        {
            pad.Bottom += 10;
            return pad;
        }
    }
}
