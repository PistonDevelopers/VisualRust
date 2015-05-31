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
            this.label1 = new System.Windows.Forms.Label();
            this.debuggerLocation = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.extraArgs = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.browseDebugger = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 4);
            this.label1.Margin = new System.Windows.Forms.Padding(8, 4, 3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "GDB debugger:";
            // 
            // debuggerLocation
            // 
            this.debuggerLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.debuggerLocation.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.debuggerLocation.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.debuggerLocation.Location = new System.Drawing.Point(141, 3);
            this.debuggerLocation.Name = "debuggerLocation";
            this.debuggerLocation.Size = new System.Drawing.Size(372, 23);
            this.debuggerLocation.TabIndex = 1;
            this.toolTip.SetToolTip(this.debuggerLocation, "Location of the debugger.");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 33);
            this.label2.Margin = new System.Windows.Forms.Padding(8, 4, 3, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(127, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Additional parameters:";
            // 
            // extraArgs
            // 
            this.extraArgs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.extraArgs.Location = new System.Drawing.Point(141, 32);
            this.extraArgs.Name = "extraArgs";
            this.extraArgs.Size = new System.Drawing.Size(372, 23);
            this.extraArgs.TabIndex = 4;
            this.toolTip.SetToolTip(this.extraArgs, "Additional parameters for debugger\'s command line.");
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 46.56085F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.extraArgs, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.debuggerLocation, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.browseDebugger, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(548, 61);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // browseDebugger
            // 
            this.browseDebugger.Location = new System.Drawing.Point(519, 2);
            this.browseDebugger.Margin = new System.Windows.Forms.Padding(3, 2, 3, 4);
            this.browseDebugger.Name = "browseDebugger";
            this.browseDebugger.Size = new System.Drawing.Size(26, 22);
            this.browseDebugger.TabIndex = 2;
            this.browseDebugger.Text = "...";
            this.browseDebugger.UseVisualStyleBackColor = true;
            this.browseDebugger.Click += new System.EventHandler(this.browseDebugger_Click);
            // 
            // DebuggingOptionsPageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "DebuggingOptionsPageControl";
            this.Size = new System.Drawing.Size(573, 84);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox debuggerLocation;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox extraArgs;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button browseDebugger;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
