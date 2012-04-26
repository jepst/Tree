namespace Tree
{
    partial class TracePane
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
            this.traceListBox = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.traceToolStrip = new System.Windows.Forms.ToolStrip();
            this.addTraceToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.deleteTraceToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.editTraceToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.traceToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // traceListBox
            // 
            this.traceListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.traceListBox.FormattingEnabled = true;
            this.traceListBox.ItemHeight = 16;
            this.traceListBox.Location = new System.Drawing.Point(3, 32);
            this.traceListBox.Name = "traceListBox";
            this.traceListBox.Size = new System.Drawing.Size(144, 100);
            this.traceListBox.TabIndex = 0;
            this.traceListBox.SelectedIndexChanged += new System.EventHandler(this.traceListBox_SelectedIndexChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.traceListBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.traceToolStrip, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(150, 150);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // traceToolStrip
            // 
            this.traceToolStrip.CanOverflow = false;
            this.traceToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.traceToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.traceToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addTraceToolStripButton,
            this.deleteTraceToolStripButton,
            this.editTraceToolStripButton});
            this.traceToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.traceToolStrip.Location = new System.Drawing.Point(0, 0);
            this.traceToolStrip.Name = "traceToolStrip";
            this.traceToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.traceToolStrip.Size = new System.Drawing.Size(110, 29);
            this.traceToolStrip.Stretch = true;
            this.traceToolStrip.TabIndex = 3;
            // 
            // addTraceToolStripButton
            // 
            this.addTraceToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.addTraceToolStripButton.Image = global::Tree.Properties.Resources.traceNew;
            this.addTraceToolStripButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.addTraceToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addTraceToolStripButton.Name = "addTraceToolStripButton";
            this.addTraceToolStripButton.Size = new System.Drawing.Size(26, 26);
            this.addTraceToolStripButton.ToolTipText = "Add trace";
            this.addTraceToolStripButton.Click += new System.EventHandler(this.addTraceToolStripButton_Click);
            // 
            // deleteTraceToolStripButton
            // 
            this.deleteTraceToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.deleteTraceToolStripButton.Image = global::Tree.Properties.Resources.traceDelete;
            this.deleteTraceToolStripButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.deleteTraceToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.deleteTraceToolStripButton.Name = "deleteTraceToolStripButton";
            this.deleteTraceToolStripButton.Size = new System.Drawing.Size(26, 26);
            this.deleteTraceToolStripButton.ToolTipText = "Delete trace";
            this.deleteTraceToolStripButton.Click += new System.EventHandler(this.deleteTraceToolStripButton_Click);
            // 
            // editTraceToolStripButton
            // 
            this.editTraceToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.editTraceToolStripButton.Image = global::Tree.Properties.Resources.traceEdit;
            this.editTraceToolStripButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.editTraceToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.editTraceToolStripButton.Name = "editTraceToolStripButton";
            this.editTraceToolStripButton.Size = new System.Drawing.Size(26, 26);
            this.editTraceToolStripButton.ToolTipText = "Edit trace";
            this.editTraceToolStripButton.Click += new System.EventHandler(this.editTraceToolStripButton_Click);
            // 
            // TracePane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "TracePane";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.traceToolStrip.ResumeLayout(false);
            this.traceToolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox traceListBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolStrip traceToolStrip;
        private System.Windows.Forms.ToolStripButton addTraceToolStripButton;
        private System.Windows.Forms.ToolStripButton deleteTraceToolStripButton;
        private System.Windows.Forms.ToolStripButton editTraceToolStripButton;
    }
}
