using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Tree
{

    /*
     * via http://www.codeproject.com/KB/wtl/transparent.aspx
     */
    public partial class HintBox : Form    
    {
        private Control m_parent;
        public HintBox(Control parent)
        {
            InitializeComponent();
            m_parent = parent;
            parent.TopLevelControl.Move += new EventHandler(parent_Move);
            parent.Resize += new EventHandler(parent_Resize);
        }

        public String Label
        {
            set
            {
                labelBox.Text = value;
                //labelBox.SelectionStart = labelBox.SelectionLength= 0;
            }
        }
        protected override void OnShown(EventArgs e)
        {
            AdjustSize();
            base.OnShown(e);
        }

        void parent_Resize(object sender, EventArgs e)
        {
            AdjustSize();
        }

        void parent_Move(object sender, EventArgs e)
        {
            AdjustSize();
        }


        protected override void OnLayout(LayoutEventArgs levent)
        {
            AdjustSize();
            base.OnLayout(levent);
        }
        public void AdjustSize()
        {
            if (Visible)
            {
                Rectangle rect = new Rectangle(m_parent.PointToScreen(m_parent.Location),
                    m_parent.Size);

                rect.Inflate(-30, -30);
                labelBox.MaximumSize = rect.Size;
                rect.Inflate(10, 10);
                rect.Height = labelBox.Size.Height + 40;

                this.Location = rect.Location;
                this.Size = rect.Size;
                
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetLayeredWindowAttributes(IntPtr hwnd, Int32 color,byte alpha,Int32 dwFlags);

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00000020 | 0x00080000; //WS_EX_TRANSPARENT | WS_EX_LAYERED
                return cp;
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SetLayeredWindowAttributes(this.Handle, 0,128,2);
        }
    }
}