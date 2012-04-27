using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Tree
{
    public partial class SpacingDialog : Form
    {
        public SpacingDialog()
        {
            InitializeComponent();
        }

        public int MarginHorizontal
        {
            get
            {
                return (int)marginHorizontalNumeric.Value;
            }
            set
            {
                marginHorizontalNumeric.Value = value;
            }
        }
        public int MarginVertical
        {
            get
            {
                return (int)marginVerticalNumeric.Value;
            }
            set
            {
                marginVerticalNumeric.Value = value;
            }
        }
        public int PaddingHorizontal
        {
            get
            {
                return (int)paddingHorizontalNumeric.Value;
            }
            set
            {
                paddingHorizontalNumeric.Value = value;
            }
        }
        public int PaddingVertical
        {
            get
            {
                return (int)paddingVerticalNumeric.Value;
            }
            set
            {
                paddingVerticalNumeric.Value = value;
            }
        }
        public int TraceHorizontal
        {
            get
            {
                return (int)traceHorizontal.Value;
            }
            set
            {
                traceHorizontal.Value = value;
            }
        }
        public int TraceVertical
        {
            get
            {
                return (int)traceVertical.Value;
            }
            set
            {
                traceVertical.Value = value;
            }
        }

     
    }
}