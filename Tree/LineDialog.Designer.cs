namespace Tree
{
    partial class LineDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.GroupBox groupBox1;
            System.Windows.Forms.Label shapeLabel;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label5;
            this.samplePanel = new System.Windows.Forms.Panel();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.styleComboBox = new System.Windows.Forms.ComboBox();
            this.colorComboBox = new System.Windows.Forms.ComboBox();
            this.widthComboBox = new System.Windows.Forms.ComboBox();
            this.shapeComboBox = new System.Windows.Forms.ComboBox();
            this.paddinglNumeric = new System.Windows.Forms.NumericUpDown();
            this.decorationGroupBox = new System.Windows.Forms.GroupBox();
            this.subtreeRadioButton = new System.Windows.Forms.RadioButton();
            this.nodeRadioButton = new System.Windows.Forms.RadioButton();
            this.noneRadioButton = new System.Windows.Forms.RadioButton();
            this.traceGroupBox = new System.Windows.Forms.GroupBox();
            this.arrowComboBox = new System.Windows.Forms.ComboBox();
            this.traceLineRadioButton = new System.Windows.Forms.RadioButton();
            this.traceCurveRadioButton = new System.Windows.Forms.RadioButton();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            shapeLabel = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.paddinglNumeric)).BeginInit();
            this.decorationGroupBox.SuspendLayout();
            this.traceGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            label1.Location = new System.Drawing.Point(13, 13);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(100, 17);
            label1.TabIndex = 0;
            label1.Text = "&Style";
            // 
            // label2
            // 
            label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            label2.Location = new System.Drawing.Point(176, 13);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(46, 17);
            label2.TabIndex = 1;
            label2.Text = "&Color";
            // 
            // label3
            // 
            label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            label3.Location = new System.Drawing.Point(342, 13);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(100, 17);
            label3.TabIndex = 2;
            label3.Text = "&Width";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(this.samplePanel);
            groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            groupBox1.Location = new System.Drawing.Point(207, 79);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(237, 120);
            groupBox1.TabIndex = 12;
            groupBox1.TabStop = false;
            groupBox1.Text = "Sample";
            // 
            // samplePanel
            // 
            this.samplePanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.samplePanel.Location = new System.Drawing.Point(37, 34);
            this.samplePanel.Name = "samplePanel";
            this.samplePanel.Size = new System.Drawing.Size(169, 56);
            this.samplePanel.TabIndex = 0;
            this.samplePanel.Paint += new System.Windows.Forms.PaintEventHandler(this.samplePanel_Paint);
            // 
            // shapeLabel
            // 
            shapeLabel.AutoSize = true;
            shapeLabel.Location = new System.Drawing.Point(6, 119);
            shapeLabel.Name = "shapeLabel";
            shapeLabel.Size = new System.Drawing.Size(49, 17);
            shapeLabel.TabIndex = 3;
            shapeLabel.Text = "S&hape";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(107, 119);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(60, 17);
            label4.TabIndex = 5;
            label4.Text = "&Padding";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(12, 104);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(44, 17);
            label5.TabIndex = 14;
            label5.Text = "&Arrow";
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(290, 237);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 5;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(371, 237);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // styleComboBox
            // 
            this.styleComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.styleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.styleComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.styleComboBox.FormattingEnabled = true;
            this.styleComboBox.Location = new System.Drawing.Point(16, 33);
            this.styleComboBox.Name = "styleComboBox";
            this.styleComboBox.Size = new System.Drawing.Size(157, 23);
            this.styleComboBox.TabIndex = 0;
            this.styleComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.styleComboBox_DrawItem);
            this.styleComboBox.SelectedIndexChanged += new System.EventHandler(this.styleComboBox_SelectedIndexChanged);
            // 
            // colorComboBox
            // 
            this.colorComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.colorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.colorComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.colorComboBox.FormattingEnabled = true;
            this.colorComboBox.Location = new System.Drawing.Point(179, 33);
            this.colorComboBox.Name = "colorComboBox";
            this.colorComboBox.Size = new System.Drawing.Size(157, 23);
            this.colorComboBox.TabIndex = 1;
            this.colorComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.colorComboBox_DrawItem);
            this.colorComboBox.SelectedIndexChanged += new System.EventHandler(this.colorComboBox_SelectedIndexChanged);
            // 
            // widthComboBox
            // 
            this.widthComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.widthComboBox.FormattingEnabled = true;
            this.widthComboBox.Location = new System.Drawing.Point(343, 33);
            this.widthComboBox.Name = "widthComboBox";
            this.widthComboBox.Size = new System.Drawing.Size(101, 24);
            this.widthComboBox.TabIndex = 2;
            this.widthComboBox.Validating += new System.ComponentModel.CancelEventHandler(this.widthComboBox_Validating);
            this.widthComboBox.SelectedIndexChanged += new System.EventHandler(this.widthComboBox_SelectedIndexChanged);
            this.widthComboBox.TextUpdate += new System.EventHandler(this.widthComboBox_TextUpdate);
            // 
            // shapeComboBox
            // 
            this.shapeComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.shapeComboBox.DropDownHeight = 120;
            this.shapeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.shapeComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.shapeComboBox.FormattingEnabled = true;
            this.shapeComboBox.IntegralHeight = false;
            this.shapeComboBox.ItemHeight = 30;
            this.shapeComboBox.Location = new System.Drawing.Point(7, 139);
            this.shapeComboBox.MaxDropDownItems = 3;
            this.shapeComboBox.Name = "shapeComboBox";
            this.shapeComboBox.Size = new System.Drawing.Size(72, 36);
            this.shapeComboBox.TabIndex = 4;
            this.shapeComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.shapeComboBox_DrawItem);
            this.shapeComboBox.SelectedIndexChanged += new System.EventHandler(this.shapeComboBox_SelectedIndexChanged);
            // 
            // paddinglNumeric
            // 
            this.paddinglNumeric.Location = new System.Drawing.Point(110, 139);
            this.paddinglNumeric.Name = "paddinglNumeric";
            this.paddinglNumeric.Size = new System.Drawing.Size(69, 22);
            this.paddinglNumeric.TabIndex = 6;
            // 
            // decorationGroupBox
            // 
            this.decorationGroupBox.Controls.Add(this.subtreeRadioButton);
            this.decorationGroupBox.Controls.Add(this.nodeRadioButton);
            this.decorationGroupBox.Controls.Add(this.noneRadioButton);
            this.decorationGroupBox.Controls.Add(label4);
            this.decorationGroupBox.Controls.Add(this.paddinglNumeric);
            this.decorationGroupBox.Controls.Add(shapeLabel);
            this.decorationGroupBox.Controls.Add(this.shapeComboBox);
            this.decorationGroupBox.Location = new System.Drawing.Point(16, 79);
            this.decorationGroupBox.Name = "decorationGroupBox";
            this.decorationGroupBox.Size = new System.Drawing.Size(185, 181);
            this.decorationGroupBox.TabIndex = 4;
            this.decorationGroupBox.TabStop = false;
            this.decorationGroupBox.Text = "&Decoration";
            // 
            // subtreeRadioButton
            // 
            this.subtreeRadioButton.AutoSize = true;
            this.subtreeRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.subtreeRadioButton.Location = new System.Drawing.Point(6, 78);
            this.subtreeRadioButton.Name = "subtreeRadioButton";
            this.subtreeRadioButton.Size = new System.Drawing.Size(130, 22);
            this.subtreeRadioButton.TabIndex = 9;
            this.subtreeRadioButton.TabStop = true;
            this.subtreeRadioButton.Text = "Around s&ubtree";
            this.subtreeRadioButton.UseVisualStyleBackColor = true;
            this.subtreeRadioButton.CheckedChanged += new System.EventHandler(this.subtreeRadioButton_CheckedChanged);
            // 
            // nodeRadioButton
            // 
            this.nodeRadioButton.AutoSize = true;
            this.nodeRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.nodeRadioButton.Location = new System.Drawing.Point(7, 50);
            this.nodeRadioButton.Name = "nodeRadioButton";
            this.nodeRadioButton.Size = new System.Drawing.Size(114, 22);
            this.nodeRadioButton.TabIndex = 8;
            this.nodeRadioButton.TabStop = true;
            this.nodeRadioButton.Text = "Around &node";
            this.nodeRadioButton.UseVisualStyleBackColor = true;
            this.nodeRadioButton.CheckedChanged += new System.EventHandler(this.nodeRadioButton_CheckedChanged);
            // 
            // noneRadioButton
            // 
            this.noneRadioButton.AutoSize = true;
            this.noneRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.noneRadioButton.Location = new System.Drawing.Point(7, 22);
            this.noneRadioButton.Name = "noneRadioButton";
            this.noneRadioButton.Size = new System.Drawing.Size(66, 22);
            this.noneRadioButton.TabIndex = 7;
            this.noneRadioButton.TabStop = true;
            this.noneRadioButton.Text = "Non&e";
            this.noneRadioButton.UseVisualStyleBackColor = true;
            this.noneRadioButton.CheckedChanged += new System.EventHandler(this.noneRadioButton_CheckedChanged);
            // 
            // traceGroupBox
            // 
            this.traceGroupBox.Controls.Add(label5);
            this.traceGroupBox.Controls.Add(this.arrowComboBox);
            this.traceGroupBox.Controls.Add(this.traceLineRadioButton);
            this.traceGroupBox.Controls.Add(this.traceCurveRadioButton);
            this.traceGroupBox.Location = new System.Drawing.Point(10, 78);
            this.traceGroupBox.Name = "traceGroupBox";
            this.traceGroupBox.Size = new System.Drawing.Size(190, 181);
            this.traceGroupBox.TabIndex = 4;
            this.traceGroupBox.TabStop = false;
            this.traceGroupBox.Text = "&Trace";
            // 
            // arrowComboBox
            // 
            this.arrowComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.arrowComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.arrowComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.arrowComboBox.FormattingEnabled = true;
            this.arrowComboBox.Location = new System.Drawing.Point(12, 124);
            this.arrowComboBox.Name = "arrowComboBox";
            this.arrowComboBox.Size = new System.Drawing.Size(115, 23);
            this.arrowComboBox.TabIndex = 13;
            this.arrowComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.arrowComboBox_DrawItem);
            this.arrowComboBox.SelectedIndexChanged += new System.EventHandler(this.arrowComboBox_SelectedIndexChanged);
            // 
            // traceLineRadioButton
            // 
            this.traceLineRadioButton.AutoSize = true;
            this.traceLineRadioButton.Location = new System.Drawing.Point(15, 63);
            this.traceLineRadioButton.Name = "traceLineRadioButton";
            this.traceLineRadioButton.Size = new System.Drawing.Size(53, 21);
            this.traceLineRadioButton.TabIndex = 1;
            this.traceLineRadioButton.TabStop = true;
            this.traceLineRadioButton.Text = "&Line";
            this.traceLineRadioButton.UseVisualStyleBackColor = true;
            this.traceLineRadioButton.CheckedChanged += new System.EventHandler(this.traceLineRadioButton_CheckedChanged);
            // 
            // traceCurveRadioButton
            // 
            this.traceCurveRadioButton.AutoSize = true;
            this.traceCurveRadioButton.Location = new System.Drawing.Point(13, 35);
            this.traceCurveRadioButton.Name = "traceCurveRadioButton";
            this.traceCurveRadioButton.Size = new System.Drawing.Size(63, 21);
            this.traceCurveRadioButton.TabIndex = 0;
            this.traceCurveRadioButton.TabStop = true;
            this.traceCurveRadioButton.Text = "C&urve";
            this.traceCurveRadioButton.UseVisualStyleBackColor = true;
            this.traceCurveRadioButton.CheckedChanged += new System.EventHandler(this.traceCurveRadioButton_CheckedChanged);
            // 
            // LineDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(458, 272);
            this.Controls.Add(groupBox1);
            this.Controls.Add(label3);
            this.Controls.Add(this.widthComboBox);
            this.Controls.Add(label2);
            this.Controls.Add(this.colorComboBox);
            this.Controls.Add(label1);
            this.Controls.Add(this.styleComboBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.traceGroupBox);
            this.Controls.Add(this.decorationGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LineDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Shown += new System.EventHandler(this.LineDialog_Shown);
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.paddinglNumeric)).EndInit();
            this.decorationGroupBox.ResumeLayout(false);
            this.decorationGroupBox.PerformLayout();
            this.traceGroupBox.ResumeLayout(false);
            this.traceGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.ComboBox styleComboBox;
        private System.Windows.Forms.ComboBox colorComboBox;
        private System.Windows.Forms.ComboBox widthComboBox;
        private System.Windows.Forms.Panel samplePanel;
        private System.Windows.Forms.ComboBox shapeComboBox;
        private System.Windows.Forms.NumericUpDown paddinglNumeric;
        private System.Windows.Forms.GroupBox decorationGroupBox;
        private System.Windows.Forms.RadioButton subtreeRadioButton;
        private System.Windows.Forms.RadioButton nodeRadioButton;
        private System.Windows.Forms.RadioButton noneRadioButton;
        private System.Windows.Forms.GroupBox traceGroupBox;
        private System.Windows.Forms.RadioButton traceLineRadioButton;
        private System.Windows.Forms.RadioButton traceCurveRadioButton;
        private System.Windows.Forms.ComboBox arrowComboBox;
    }
}