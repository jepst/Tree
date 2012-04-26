using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Tree
{
    public partial class TracePane : UserControl
    {
        public event EventHandler OnTraceModified = null;
        private List<Trace> traces;
        private Elem elem;
        public SyntaxTreeViewer stv;
        public TracePane()
        {
            InitializeComponent();
            traces = new List<Trace>();
            elem = null;
            UpdateGUI();
        }
        public void SetSTV(SyntaxTreeViewer stv)
        {
            this.stv = stv;
        }
        /*
        private ContextMenuStrip CreateContextMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Opening += new CancelEventHandler(menu_Opening);
            menu.SuspendLayout();
            menu.Name = "traceMenu";

            ToolStripMenuItem deleteTrace = new ToolStripMenuItem();
            deleteTrace.Text = "&Delete trace";
            deleteTrace.Click += new EventHandler(deleteTrace_Click);

            ToolStripMenuItem editTrace = new ToolStripMenuItem();
            editTrace.Text = "&Edit trace";
            editTrace.Click += new EventHandler(editTrace_Click);

            menu.Items.AddRange(new ToolStripItem[] {
                deleteTrace,
                editTrace
                });

            menu.ResumeLayout(false);
            return menu;
        }

        
        void menu_Opening(object sender, CancelEventArgs e)
        {
            if (traceListBox.Items.Count == 0 || elem == null || traceListBox.SelectedItem == null)
                e.Cancel = true;
        }

        public void editTrace_Click(object sender, EventArgs e)
        {
        }
        public void deleteTrace_Click(object sender, EventArgs e)
        {
            if (elem!=null)
            {
                Node node = elem.GetNode();
                Trace o = traceListBox.SelectedItem as Trace;
                if (o!=null)
                    node.RemoveTrace(o);
                FireTraceModified();
            }
        }*/

        private void FireTraceModified()
        {
            if (OnTraceModified != null)
                OnTraceModified(this, new EventArgs());
        }

        public void RefreshData()
        {
            SetTraces(elem);
        }
        public void SetTraces(Elem elem)
        {
            this.elem = elem;
            if (elem == null)
            {
                traceListBox.DataSource = null;
                return;
            }
            Node node = elem.GetNode();
            traces.Clear();
            traceListBox.DataSource = null;
            foreach (Trace t in node.Traces())
            {
                traces.Add(t);
            }
            traceListBox.DataSource = traces;
            if (traceListBox.Items.Count > 0)
                traceListBox.SelectedIndex = 0;
        }

        private void UpdateGUI()
        {
            bool sel = traceListBox.SelectedIndex >= 0;
            deleteTraceToolStripButton.Enabled = sel;
            editTraceToolStripButton.Enabled = sel;
        }

        private void addTraceToolStripButton_Click(object sender, EventArgs e)
        {
            if (elem != null)
                this.stv.DoCreateTrace(true);
        }

        private void deleteTraceToolStripButton_Click(object sender, EventArgs e)
        {
            if (elem != null)
            {
                stv.UndoAdd();
                Trace o = traceListBox.SelectedItem as Trace;
                if (o != null)
                    elem.GetNode().RemoveTrace(o);
                FireTraceModified();
            }
        }

        private void editTraceToolStripButton_Click(object sender, EventArgs e)
        {
            Trace o = traceListBox.SelectedItem as Trace;
            if (o != null)
            {
                using (LineDialog ld = new LineDialog())
                {
                    ld.Style = o.penstyle;
                    ld.TraceStyle = o.tracestyle;
                    ld.ShapeMode = LineDialog.LineDialogMode.Trace;

                    if (ld.ShowDialog(this) == DialogResult.OK)
                    {
                        stv.UndoAdd();
                        o.penstyle = ld.Style;
                        o.tracestyle = ld.TraceStyle;
                        stv.UpdateTraces();
                        stv.UpdateEverything(true);
                    }
                }
            }
        }

        private void traceListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGUI();
        }

    }
}
