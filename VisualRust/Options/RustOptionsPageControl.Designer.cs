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
            this.RacerPathLabel = new System.Windows.Forms.Label();
            this.RacerPathTextBox = new System.Windows.Forms.TextBox();
            this.SetRacerPathButton = new System.Windows.Forms.Button();
            this.SetRustSrcFolderPathButton = new System.Windows.Forms.Button();
            this.RustSrcFolderPathLabel = new System.Windows.Forms.Label();
            this.RustSrcFolderPathTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // RacerPathLabel
            // 
            this.RacerPathLabel.AutoSize = true;
            this.RacerPathLabel.Location = new System.Drawing.Point(3, 0);
            this.RacerPathLabel.Name = "RacerPathLabel";
            this.RacerPathLabel.Size = new System.Drawing.Size(93, 13);
            this.RacerPathLabel.TabIndex = 0;
            this.RacerPathLabel.Text = "Path to Racer.exe";
            // 
            // RacerPathTextBox
            // 
            this.RacerPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RacerPathTextBox.Location = new System.Drawing.Point(6, 17);
            this.RacerPathTextBox.Name = "RacerPathTextBox";
            this.RacerPathTextBox.Size = new System.Drawing.Size(338, 20);
            this.RacerPathTextBox.TabIndex = 1;
            this.RacerPathTextBox.TextChanged += new System.EventHandler(this.RacerPathTextBox_TextChanged);
            // 
            // SetRacerPathButton
            // 
            this.SetRacerPathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SetRacerPathButton.Location = new System.Drawing.Point(350, 16);
            this.SetRacerPathButton.Name = "SetRacerPathButton";
            this.SetRacerPathButton.Size = new System.Drawing.Size(26, 22);
            this.SetRacerPathButton.TabIndex = 2;
            this.SetRacerPathButton.Text = "...";
            this.SetRacerPathButton.UseVisualStyleBackColor = true;
            this.SetRacerPathButton.Click += new System.EventHandler(this.SetRacerPathButton_Click);
            // 
            // SetRustSrcFolderPathButton
            // 
            this.SetRustSrcFolderPathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SetRustSrcFolderPathButton.Location = new System.Drawing.Point(350, 67);
            this.SetRustSrcFolderPathButton.Name = "SetRustSrcFolderPathButton";
            this.SetRustSrcFolderPathButton.Size = new System.Drawing.Size(26, 22);
            this.SetRustSrcFolderPathButton.TabIndex = 3;
            this.SetRustSrcFolderPathButton.Text = "...";
            this.SetRustSrcFolderPathButton.UseVisualStyleBackColor = true;
            this.SetRustSrcFolderPathButton.Click += new System.EventHandler(this.SetRustSrcFolderPathButton_Click);
            // 
            // RustSrcFolderPathLabel
            // 
            this.RustSrcFolderPathLabel.AutoSize = true;
            this.RustSrcFolderPathLabel.Location = new System.Drawing.Point(3, 52);
            this.RustSrcFolderPathLabel.Name = "RustSrcFolderPathLabel";
            this.RustSrcFolderPathLabel.Size = new System.Drawing.Size(125, 13);
            this.RustSrcFolderPathLabel.TabIndex = 4;
            this.RustSrcFolderPathLabel.Text = "Path to rust source folder";
            // 
            // RustSrcFolderPathTextBox
            // 
            this.RustSrcFolderPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RustSrcFolderPathTextBox.Location = new System.Drawing.Point(6, 68);
            this.RustSrcFolderPathTextBox.Name = "RustSrcFolderPathTextBox";
            this.RustSrcFolderPathTextBox.Size = new System.Drawing.Size(338, 20);
            this.RustSrcFolderPathTextBox.TabIndex = 5;
            this.RustSrcFolderPathTextBox.TextChanged += new System.EventHandler(this.RustSrcFolderPathTextBox_TextChanged);
            // 
            // RustOptionsPageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.RustSrcFolderPathTextBox);
            this.Controls.Add(this.RustSrcFolderPathLabel);
            this.Controls.Add(this.SetRustSrcFolderPathButton);
            this.Controls.Add(this.SetRacerPathButton);
            this.Controls.Add(this.RacerPathTextBox);
            this.Controls.Add(this.RacerPathLabel);
            this.Name = "RustOptionsPageControl";
            this.Size = new System.Drawing.Size(379, 185);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label RacerPathLabel;
        private System.Windows.Forms.TextBox RacerPathTextBox;
        private System.Windows.Forms.Button SetRacerPathButton;
        private System.Windows.Forms.Button SetRustSrcFolderPathButton;
        private System.Windows.Forms.Label RustSrcFolderPathLabel;
        private System.Windows.Forms.TextBox RustSrcFolderPathTextBox;
    }
}
