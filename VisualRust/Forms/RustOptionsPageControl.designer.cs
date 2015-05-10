namespace VisualRust.Options
{
    partial class RustOptionsPageControl
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
            this.racerBox = new System.Windows.Forms.GroupBox();
            this.bundledRacer = new System.Windows.Forms.RadioButton();
            this.customRacer = new System.Windows.Forms.RadioButton();
            this.customRacerPath = new System.Windows.Forms.TextBox();
            this.racerBoxPanel = new System.Windows.Forms.TableLayoutPanel();
            this.customRacerButton = new System.Windows.Forms.Button();
            this.sourceBox = new System.Windows.Forms.GroupBox();
            this.sourceBoxPanel = new System.Windows.Forms.TableLayoutPanel();
            this.envSource = new System.Windows.Forms.RadioButton();
            this.customSource = new System.Windows.Forms.RadioButton();
            this.customSourcePath = new System.Windows.Forms.TextBox();
            this.customSourceButton = new System.Windows.Forms.Button();
            this.racerBox.SuspendLayout();
            this.racerBoxPanel.SuspendLayout();
            this.sourceBox.SuspendLayout();
            this.sourceBoxPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // racerBox
            // 
            this.racerBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.racerBox.AutoSize = true;
            this.racerBox.Controls.Add(this.racerBoxPanel);
            this.racerBox.Location = new System.Drawing.Point(0, 0);
            this.racerBox.Name = "racerBox";
            this.racerBox.Size = new System.Drawing.Size(379, 103);
            this.racerBox.TabIndex = 7;
            this.racerBox.TabStop = false;
            this.racerBox.Text = "Racer location";
            // 
            // bundledRacer
            // 
            this.bundledRacer.AutoSize = true;
            this.racerBoxPanel.SetColumnSpan(this.bundledRacer, 2);
            this.bundledRacer.Location = new System.Drawing.Point(8, 4);
            this.bundledRacer.Margin = new System.Windows.Forms.Padding(8, 4, 3, 3);
            this.bundledRacer.Name = "bundledRacer";
            this.bundledRacer.Size = new System.Drawing.Size(132, 17);
            this.bundledRacer.TabIndex = 3;
            this.bundledRacer.TabStop = true;
            this.bundledRacer.Text = "Use bundled racer.exe";
            this.bundledRacer.UseVisualStyleBackColor = true;
            // 
            // customRacer
            // 
            this.customRacer.AutoSize = true;
            this.racerBoxPanel.SetColumnSpan(this.customRacer, 2);
            this.customRacer.Location = new System.Drawing.Point(8, 29);
            this.customRacer.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
            this.customRacer.Name = "customRacer";
            this.customRacer.Size = new System.Drawing.Size(174, 17);
            this.customRacer.TabIndex = 4;
            this.customRacer.TabStop = true;
            this.customRacer.Text = "Use racer from custom location:";
            this.customRacer.UseVisualStyleBackColor = true;
            // 
            // customRacerPath
            // 
            this.customRacerPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.customRacerPath.Location = new System.Drawing.Point(27, 54);
            this.customRacerPath.Margin = new System.Windows.Forms.Padding(27, 3, 3, 4);
            this.customRacerPath.Name = "customRacerPath";
            this.customRacerPath.Size = new System.Drawing.Size(306, 20);
            this.customRacerPath.TabIndex = 1;
            // 
            // racerBoxPanel
            // 
            this.racerBoxPanel.ColumnCount = 2;
            this.racerBoxPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.racerBoxPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.racerBoxPanel.Controls.Add(this.bundledRacer, 0, 0);
            this.racerBoxPanel.Controls.Add(this.customRacer, 0, 1);
            this.racerBoxPanel.Controls.Add(this.customRacerPath, 0, 2);
            this.racerBoxPanel.Controls.Add(this.customRacerButton, 1, 2);
            this.racerBoxPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.racerBoxPanel.Location = new System.Drawing.Point(3, 16);
            this.racerBoxPanel.Margin = new System.Windows.Forms.Padding(0);
            this.racerBoxPanel.Name = "racerBoxPanel";
            this.racerBoxPanel.RowCount = 3;
            this.racerBoxPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.racerBoxPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.racerBoxPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.racerBoxPanel.Size = new System.Drawing.Size(373, 84);
            this.racerBoxPanel.TabIndex = 5;
            // 
            // customRacerButton
            // 
            this.customRacerButton.Location = new System.Drawing.Point(344, 53);
            this.customRacerButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 4);
            this.customRacerButton.Name = "customRacerButton";
            this.customRacerButton.Size = new System.Drawing.Size(26, 22);
            this.customRacerButton.TabIndex = 8;
            this.customRacerButton.Text = "...";
            this.customRacerButton.UseVisualStyleBackColor = true;
            // 
            // sourceBox
            // 
            this.sourceBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sourceBox.AutoSize = true;
            this.sourceBox.Controls.Add(this.sourceBoxPanel);
            this.sourceBox.Location = new System.Drawing.Point(0, 111);
            this.sourceBox.Name = "sourceBox";
            this.sourceBox.Size = new System.Drawing.Size(379, 113);
            this.sourceBox.TabIndex = 8;
            this.sourceBox.TabStop = false;
            this.sourceBox.Text = "Rust sources";
            // 
            // sourceBoxPanel
            // 
            this.sourceBoxPanel.ColumnCount = 2;
            this.sourceBoxPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.sourceBoxPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.sourceBoxPanel.Controls.Add(this.envSource, 0, 0);
            this.sourceBoxPanel.Controls.Add(this.customSource, 0, 1);
            this.sourceBoxPanel.Controls.Add(this.customSourcePath, 0, 2);
            this.sourceBoxPanel.Controls.Add(this.customSourceButton, 1, 2);
            this.sourceBoxPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourceBoxPanel.Location = new System.Drawing.Point(3, 16);
            this.sourceBoxPanel.Margin = new System.Windows.Forms.Padding(0);
            this.sourceBoxPanel.Name = "sourceBoxPanel";
            this.sourceBoxPanel.RowCount = 3;
            this.sourceBoxPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.sourceBoxPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.sourceBoxPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.sourceBoxPanel.Size = new System.Drawing.Size(373, 94);
            this.sourceBoxPanel.TabIndex = 5;
            // 
            // envSource
            // 
            this.envSource.AutoSize = true;
            this.sourceBoxPanel.SetColumnSpan(this.envSource, 2);
            this.envSource.Location = new System.Drawing.Point(8, 4);
            this.envSource.Margin = new System.Windows.Forms.Padding(8, 4, 3, 3);
            this.envSource.Name = "envSource";
            this.envSource.Size = new System.Drawing.Size(325, 30);
            this.envSource.TabIndex = 3;
            this.envSource.TabStop = true;
            this.envSource.Text = "Read rust sources from enviroment variable RUST_SRC_PATH\r\n(current value: <empty>" +
    ")";
            this.envSource.UseVisualStyleBackColor = true;
            // 
            // customSource
            // 
            this.customSource.AutoSize = true;
            this.sourceBoxPanel.SetColumnSpan(this.customSource, 2);
            this.customSource.Location = new System.Drawing.Point(8, 40);
            this.customSource.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
            this.customSource.Name = "customSource";
            this.customSource.Size = new System.Drawing.Size(211, 17);
            this.customSource.TabIndex = 4;
            this.customSource.TabStop = true;
            this.customSource.Text = "Read rust sources from custom location";
            this.customSource.UseVisualStyleBackColor = true;
            // 
            // customSourcePath
            // 
            this.customSourcePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.customSourcePath.Location = new System.Drawing.Point(27, 63);
            this.customSourcePath.Margin = new System.Windows.Forms.Padding(27, 3, 3, 4);
            this.customSourcePath.Name = "customSourcePath";
            this.customSourcePath.Size = new System.Drawing.Size(311, 20);
            this.customSourcePath.TabIndex = 1;
            // 
            // customSourceButton
            // 
            this.customSourceButton.Location = new System.Drawing.Point(344, 62);
            this.customSourceButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 4);
            this.customSourceButton.Name = "customSourceButton";
            this.customSourceButton.Size = new System.Drawing.Size(26, 22);
            this.customSourceButton.TabIndex = 8;
            this.customSourceButton.Text = "...";
            this.customSourceButton.UseVisualStyleBackColor = true;
            // 
            // RustOptionsPageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.racerBox);
            this.Controls.Add(this.sourceBox);
            this.Name = "RustOptionsPageControl";
            this.Size = new System.Drawing.Size(379, 350);
            this.racerBox.ResumeLayout(false);
            this.racerBoxPanel.ResumeLayout(false);
            this.racerBoxPanel.PerformLayout();
            this.sourceBox.ResumeLayout(false);
            this.sourceBoxPanel.ResumeLayout(false);
            this.sourceBoxPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox racerBox;
        private System.Windows.Forms.RadioButton bundledRacer;
        private System.Windows.Forms.RadioButton customRacer;
        private System.Windows.Forms.TextBox customRacerPath;
        private System.Windows.Forms.TableLayoutPanel racerBoxPanel;
        private System.Windows.Forms.Button customRacerButton;
        private System.Windows.Forms.GroupBox sourceBox;
        private System.Windows.Forms.TableLayoutPanel sourceBoxPanel;
        private System.Windows.Forms.RadioButton envSource;
        private System.Windows.Forms.RadioButton customSource;
        private System.Windows.Forms.TextBox customSourcePath;
        private System.Windows.Forms.Button customSourceButton;
    }
}
