namespace VisualRust.Project.Forms
{
    partial class DebugPropertyControl
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
            this.label1 = new System.Windows.Forms.Label();
            this.commandLineArgs = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.workDir = new System.Windows.Forms.TextBox();
            this.browseWorkDir = new System.Windows.Forms.Button();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.externalProg = new System.Windows.Forms.TextBox();
            this.browseProg = new System.Windows.Forms.Button();
            this.startActionGrp = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.startActionGrp.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 91);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Command line arguments:";
            // 
            // commandLineArgs
            // 
            this.commandLineArgs.AllowDrop = true;
            this.commandLineArgs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.commandLineArgs.Location = new System.Drawing.Point(161, 91);
            this.commandLineArgs.Multiline = true;
            this.commandLineArgs.Name = "commandLineArgs";
            this.commandLineArgs.Size = new System.Drawing.Size(458, 41);
            this.commandLineArgs.TabIndex = 2;
            this.commandLineArgs.KeyDown += new System.Windows.Forms.KeyEventHandler(this.commandLineArgs_KeyDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 146);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(105, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "Working directory:";
            // 
            // workDir
            // 
            this.workDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.workDir.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.workDir.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.workDir.Location = new System.Drawing.Point(161, 143);
            this.workDir.Name = "workDir";
            this.workDir.Size = new System.Drawing.Size(458, 23);
            this.workDir.TabIndex = 7;
            // 
            // browseWorkDir
            // 
            this.browseWorkDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseWorkDir.Location = new System.Drawing.Point(625, 144);
            this.browseWorkDir.Margin = new System.Windows.Forms.Padding(3, 2, 3, 4);
            this.browseWorkDir.Name = "browseWorkDir";
            this.browseWorkDir.Size = new System.Drawing.Size(26, 22);
            this.browseWorkDir.TabIndex = 8;
            this.browseWorkDir.Text = "...";
            this.browseWorkDir.UseVisualStyleBackColor = true;
            this.browseWorkDir.Click += new System.EventHandler(this.browseWorkDir_Click);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.Location = new System.Drawing.Point(6, 22);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(111, 19);
            this.radioButton1.TabIndex = 1;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Start this project";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(6, 48);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(145, 19);
            this.radioButton2.TabIndex = 2;
            this.radioButton2.Text = "Start external program:";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // externalProg
            // 
            this.externalProg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.externalProg.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.externalProg.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.externalProg.Enabled = false;
            this.externalProg.Location = new System.Drawing.Point(158, 48);
            this.externalProg.Name = "externalProg";
            this.externalProg.Size = new System.Drawing.Size(458, 23);
            this.externalProg.TabIndex = 3;
            // 
            // browseProg
            // 
            this.browseProg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseProg.Enabled = false;
            this.browseProg.Location = new System.Drawing.Point(621, 49);
            this.browseProg.Margin = new System.Windows.Forms.Padding(3, 2, 3, 4);
            this.browseProg.Name = "browseProg";
            this.browseProg.Size = new System.Drawing.Size(26, 22);
            this.browseProg.TabIndex = 4;
            this.browseProg.Text = "...";
            this.browseProg.UseVisualStyleBackColor = true;
            this.browseProg.Click += new System.EventHandler(this.browseProg_Click);
            // 
            // startActionGrp
            // 
            this.startActionGrp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.startActionGrp.Controls.Add(this.label4);
            this.startActionGrp.Controls.Add(this.radioButton1);
            this.startActionGrp.Controls.Add(this.externalProg);
            this.startActionGrp.Controls.Add(this.browseProg);
            this.startActionGrp.Controls.Add(this.radioButton2);
            this.startActionGrp.Location = new System.Drawing.Point(3, 3);
            this.startActionGrp.Name = "startActionGrp";
            this.startActionGrp.Size = new System.Drawing.Size(666, 80);
            this.startActionGrp.TabIndex = 0;
            this.startActionGrp.Text = "Start action";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 15);
            this.label4.TabIndex = 0;
            this.label4.Text = "Start action:";
            // 
            // DebugPropertyControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.startActionGrp);
            this.Controls.Add(this.browseWorkDir);
            this.Controls.Add(this.workDir);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.commandLineArgs);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "DebugPropertyControl";
            this.Size = new System.Drawing.Size(669, 186);
            this.startActionGrp.ResumeLayout(false);
            this.startActionGrp.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox commandLineArgs;
        private System.Windows.Forms.TextBox workDir;
        private System.Windows.Forms.Button browseWorkDir;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.TextBox externalProg;
        private System.Windows.Forms.Button browseProg;
        private System.Windows.Forms.Panel startActionGrp;
        private System.Windows.Forms.Label label4;
    }
}
