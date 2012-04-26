using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Tree
{
    public partial class InputBox : Form
    {
        public InputBox()
        {
            InitializeComponent();
        }

        public String Input
        {
            get
            {
                return InputTextBox.Text;
            }
            set
            {
                InputTextBox.Text = value;
            }
        }

        public String Prompt
        {
            get
            {
                return PromptLabel.Text;
            }
            set
            {
                PromptLabel.Text = value;
            }
        }
   }
}