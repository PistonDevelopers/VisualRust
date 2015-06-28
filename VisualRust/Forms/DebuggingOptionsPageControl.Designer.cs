namespace VisualRust.Options
{
    partial class DebuggingOptionsPageControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.racerBox = new System.Windows.Forms.GroupBox();
            this.gdbBoxPanel = new System.Windows.Forms.TableLayoutPanel();
            this.defaultGdb = new System.Windows.Forms.RadioButton();
            this.customGdb = new System.Windows.Forms.RadioButton();
            this.customGdbPath = new System.Windows.Forms.TextBox();
            this.customGdbButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.argsLabel = new System.Windows.Forms.Label();
            this.extraArgs = new System.Windows.Forms.TextBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.racerBox.SuspendLayout();
            this.gdbBoxPanel.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // racerBox
            // 
            this.racerBox.AutoSize = true;
            this.racerBox.Controls.Add(this.gdbBoxPanel);
            this.racerBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.racerBox.Location = new System.Drawing.Point(0, 0);
            this.racerBox.Margin = new System.Windows.Forms.Padding(0);
            this.racerBox.Name = "racerBox";
            this.racerBox.Size = new System.Drawing.Size(150, 135);
            this.racerBox.TabIndex = 8;
            this.racerBox.TabStop = false;
            this.racerBox.Text = "GDB location";
            // 
            // gdbBoxPanel
            // 
            this.gdbBoxPanel.AutoSize = true;
            this.gdbBoxPanel.ColumnCount = 2;
            this.gdbBoxPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.gdbBoxPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.gdbBoxPanel.Controls.Add(this.defaultGdb, 0, 0);
            this.gdbBoxPanel.Controls.Add(this.customGdb, 0, 1);
            this.gdbBoxPanel.Controls.Add(this.customGdbPath, 0, 2);
            this.gdbBoxPanel.Controls.Add(this.customGdbButton, 1, 2);
            this.gdbBoxPanel.Location = new System.Drawing.Point(3, 19);
            this.gdbBoxPanel.Margin = new System.Windows.Forms.Padding(0);
            this.gdbBoxPanel.Name = "gdbBoxPanel";
            this.gdbBoxPanel.RowCount = 3;
            this.gdbBoxPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.gdbBoxPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.gdbBoxPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.gdbBoxPanel.Size = new System.Drawing.Size(213, 97);
            this.gdbBoxPanel.TabIndex = 5;
            // 
            // defaultGdb
            // 
            this.defaultGdb.AutoSize = true;
            this.gdbBoxPanel.SetColumnSpan(this.defaultGdb, 2);
            this.defaultGdb.Location = new System.Drawing.Point(8, 4);
            this.defaultGdb.Margin = new System.Windows.Forms.Padding(8, 4, 3, 3);
            this.defaultGdb.Name = "defaultGdb";
            this.defaultGdb.Size = new System.Drawing.Size(202, 34);
            this.defaultGdb.TabIndex = 3;
            this.defaultGdb.TabStop = true;
            this.defaultGdb.Text = "Automatically detect path to GDB\r\n(current path: <none>)";
            this.defaultGdb.UseVisualStyleBackColor = true;
            // 
            // customGdb
            // 
            this.customGdb.AutoSize = true;
            this.gdbBoxPanel.SetColumnSpan(this.customGdb, 2);
            this.customGdb.Location = new System.Drawing.Point(8, 44);
            this.customGdb.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
            this.customGdb.Name = "customGdb";
            this.customGdb.Size = new System.Drawing.Size(166, 19);
            this.customGdb.TabIndex = 4;
            this.customGdb.TabStop = true;
            this.customGdb.Text = "Use a custom path to GDB:";
            this.customGdb.UseVisualStyleBackColor = true;
            // 
            // customGdbPath
            // 
            this.customGdbPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.customGdbPath.Location = new System.Drawing.Point(27, 69);
            this.customGdbPath.Margin = new System.Windows.Forms.Padding(27, 3, 3, 4);
            this.customGdbPath.Name = "customGdbPath";
            this.customGdbPath.Size = new System.Drawing.Size(151, 23);
            this.customGdbPath.TabIndex = 1;
            // 
            // customGdbButton
            // 
            this.customGdbButton.Location = new System.Drawing.Point(184, 68);
            this.customGdbButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 4);
            this.customGdbButton.Name = "customGdbButton";
            this.customGdbButton.Size = new System.Drawing.Size(26, 25);
            this.customGdbButton.TabIndex = 8;
            this.customGdbButton.Text = "...";
            this.customGdbButton.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Controls.Add(this.argsLabel, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.extraArgs, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 135);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(150, 100);
            this.tableLayoutPanel2.TabIndex = 5;
            // 
            // argsLabel
            // 
            this.argsLabel.AutoSize = true;
            this.argsLabel.Location = new System.Drawing.Point(0, 4);
            this.argsLabel.Margin = new System.Windows.Forms.Padding(0, 4, 3, 3);
            this.argsLabel.Name = "argsLabel";
            this.argsLabel.Size = new System.Drawing.Size(127, 15);
            this.argsLabel.TabIndex = 3;
            this.argsLabel.Text = "Additional parameters:";
            // 
            // extraArgs
            // 
            this.extraArgs.Dock = System.Windows.Forms.DockStyle.Top;
            this.extraArgs.Location = new System.Drawing.Point(3, 25);
            this.extraArgs.Name = "extraArgs";
            this.extraArgs.Size = new System.Drawing.Size(144, 23);
            this.extraArgs.TabIndex = 4;
            this.toolTip.SetToolTip(this.extraArgs, "Additional parameters for debugger\'s command line.");
            // 
            // DebuggingOptionsPageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.racerBox);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "DebuggingOptionsPageControl";
            this.racerBox.ResumeLayout(false);
            this.racerBox.PerformLayout();
            this.gdbBoxPanel.ResumeLayout(false);
            this.gdbBoxPanel.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Label argsLabel;
        private System.Windows.Forms.GroupBox racerBox;
        private System.Windows.Forms.TableLayoutPanel gdbBoxPanel;
        private System.Windows.Forms.RadioButton defaultGdb;
        private System.Windows.Forms.RadioButton customGdb;
        private System.Windows.Forms.TextBox customGdbPath;
        private System.Windows.Forms.Button customGdbButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TextBox extraArgs;
    }
}
