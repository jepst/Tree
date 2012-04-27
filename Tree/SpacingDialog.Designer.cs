namespace Tree
{
    partial class SpacingDialog
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
            System.Windows.Forms.GroupBox groupBox1;
            System.Windows.Forms.Label label1;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.GroupBox groupBox2;
            System.Windows.Forms.Label label3;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.GroupBox groupBox3;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label6;
            this.marginHorizontalNumeric = new System.Windows.Forms.NumericUpDown();
            this.marginVerticalNumeric = new System.Windows.Forms.NumericUpDown();
            this.paddingHorizontalNumeric = new System.Windows.Forms.NumericUpDown();
            this.paddingVerticalNumeric = new System.Windows.Forms.NumericUpDown();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.traceHorizontal = new System.Windows.Forms.NumericUpDown();
            this.traceVertical = new System.Windows.Forms.NumericUpDown();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            groupBox2 = new System.Windows.Forms.GroupBox();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            groupBox3 = new System.Windows.Forms.GroupBox();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.marginHorizontalNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.marginVerticalNumeric)).BeginInit();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.paddingHorizontalNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.paddingVerticalNumeric)).BeginInit();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.traceHorizontal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.traceVertical)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(this.marginHorizontalNumeric);
            groupBox1.Controls.Add(this.marginVerticalNumeric);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(label2);
            groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(228, 81);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "&Internode space";
            // 
            // marginHorizontalNumeric
            // 
            this.marginHorizontalNumeric.Location = new System.Drawing.Point(153, 21);
            this.marginHorizontalNumeric.Name = "marginHorizontalNumeric";
            this.marginHorizontalNumeric.Size = new System.Drawing.Size(69, 22);
            this.marginHorizontalNumeric.TabIndex = 0;
            // 
            // marginVerticalNumeric
            // 
            this.marginVerticalNumeric.Location = new System.Drawing.Point(153, 49);
            this.marginVerticalNumeric.Name = "marginVerticalNumeric";
            this.marginVerticalNumeric.Size = new System.Drawing.Size(69, 22);
            this.marginVerticalNumeric.TabIndex = 1;
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(16, 26);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(103, 17);
            label1.TabIndex = 0;
            label1.Text = "Horizontal";
            // 
            // label2
            // 
            label2.Location = new System.Drawing.Point(16, 51);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(103, 17);
            label2.TabIndex = 1;
            label2.Text = "Veritcal";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(this.paddingHorizontalNumeric);
            groupBox2.Controls.Add(this.paddingVerticalNumeric);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(label4);
            groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            groupBox2.Location = new System.Drawing.Point(12, 99);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(228, 81);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "I&ntranode padding";
            // 
            // paddingHorizontalNumeric
            // 
            this.paddingHorizontalNumeric.Location = new System.Drawing.Point(153, 21);
            this.paddingHorizontalNumeric.Name = "paddingHorizontalNumeric";
            this.paddingHorizontalNumeric.Size = new System.Drawing.Size(69, 22);
            this.paddingHorizontalNumeric.TabIndex = 0;
            // 
            // paddingVerticalNumeric
            // 
            this.paddingVerticalNumeric.Location = new System.Drawing.Point(153, 49);
            this.paddingVerticalNumeric.Name = "paddingVerticalNumeric";
            this.paddingVerticalNumeric.Size = new System.Drawing.Size(69, 22);
            this.paddingVerticalNumeric.TabIndex = 1;
            // 
            // label3
            // 
            label3.Location = new System.Drawing.Point(16, 26);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(103, 17);
            label3.TabIndex = 0;
            label3.Text = "Horizontal";
            // 
            // label4
            // 
            label4.Location = new System.Drawing.Point(16, 49);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(103, 17);
            label4.TabIndex = 1;
            label4.Text = "Vertical";
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(296, 12);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(296, 41);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(this.traceHorizontal);
            groupBox3.Controls.Add(this.traceVertical);
            groupBox3.Controls.Add(label5);
            groupBox3.Controls.Add(label6);
            groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            groupBox3.Location = new System.Drawing.Point(12, 186);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size(228, 81);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "&Trace spacing";
            // 
            // traceHorizontal
            // 
            this.traceHorizontal.Location = new System.Drawing.Point(153, 21);
            this.traceHorizontal.Name = "traceHorizontal";
            this.traceHorizontal.Size = new System.Drawing.Size(69, 22);
            this.traceHorizontal.TabIndex = 0;
            // 
            // traceVertical
            // 
            this.traceVertical.Location = new System.Drawing.Point(153, 49);
            this.traceVertical.Name = "traceVertical";
            this.traceVertical.Size = new System.Drawing.Size(69, 22);
            this.traceVertical.TabIndex = 1;
            // 
            // label5
            // 
            label5.Location = new System.Drawing.Point(16, 26);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(103, 17);
            label5.TabIndex = 0;
            label5.Text = "Horizontal";
            // 
            // label6
            // 
            label6.Location = new System.Drawing.Point(16, 49);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(103, 17);
            label6.TabIndex = 1;
            label6.Text = "Vertical";
            // 
            // SpacingDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(383, 296);
            this.Controls.Add(groupBox3);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(groupBox2);
            this.Controls.Add(groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SpacingDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Spacing";
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.marginHorizontalNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.marginVerticalNumeric)).EndInit();
            groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.paddingHorizontalNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.paddingVerticalNumeric)).EndInit();
            groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.traceHorizontal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.traceVertical)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.NumericUpDown marginHorizontalNumeric;
        private System.Windows.Forms.NumericUpDown marginVerticalNumeric;
        private System.Windows.Forms.NumericUpDown paddingHorizontalNumeric;
        private System.Windows.Forms.NumericUpDown paddingVerticalNumeric;
        private System.Windows.Forms.NumericUpDown traceHorizontal;
        private System.Windows.Forms.NumericUpDown traceVertical;
    }
}