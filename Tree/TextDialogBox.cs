using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Tree
{
    public partial class TextDialogBox : Form
    {
        public TextDialogBox()
        {
            InitializeComponent();
        }

        public String Content
        {
            get
            {
                return textBox.Text;
            }
            set
            {
                textBox.Text = value;
                textBox.SelectionStart = 0;
                textBox.SelectionLength = 0;
            }
        }
   }
}