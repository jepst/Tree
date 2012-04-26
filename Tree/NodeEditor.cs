using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Tree
{
    public class NodeEditor : UserControl
    {
        public event EventHandler SelectionChanged = null;

        private TableLayoutPanel tableLayoutPanel;
        private GroupBox labelEditGroupBox;
        private RichTextBox2 labelEditRichEditBox;
        private List<Elem> m_selected;
        private GroupBox lexEditGroupBox;
        private RichTextBox2 lexEditRichEditBox;
        private GroupBox nodeOperationsGroupBox;
        private CheckBox triangleCheckBox;
        private SyntaxTreeViewer m_stv;
        public bool lastTouchedWasLex;
        private bool updating;
        private GroupBox traceGroupBox;
        private TracePane tracePane;
        //private System.ComponentModel.IContainer components;
        private ToolStrip nodeToolStrip;
        private ToolStripButton createChildButton;
        private ToolStripButton createParentButton;
        private ToolStripButton deleteButton;
        private bool alreadyChanged;

        public NodeEditor()
        {
            InitializeComponent();
            labelEditRichEditBox.ContextMenuStrip = new RichTextContextMenuStrip(labelEditRichEditBox);
            lexEditRichEditBox.ContextMenuStrip = new RichTextContextMenuStrip(lexEditRichEditBox);
            updating = false;
            ItemSelect(new List<Elem>(),false);
            /* This seems dangerous, but it works for now.
             * in fact I'm not a real fan of the way I'm 
             * pushing these references around, but I'm not
             * sure how else to make a less tight binding
             * between the controls
             */
        }

        private void Plus(Control c)
        {
            c.Show();
        }

        private void Minus(Control c)
        {
            c.Hide();
        }

        public RichTextBox2 GetEditBox(bool lexical)
        {
            if (lexical)
                return lexEditRichEditBox;
            else
                return labelEditRichEditBox;
        }

        public void SetSTV(SyntaxTreeViewer stv)
        {
            this.m_stv = stv;
            stv.TreeItemSelected += new EventHandler(stv_TreeItemSelected);
            stv.KeyPress += new KeyPressEventHandler(stv_KeyPress);
            this.tracePane.SetSTV(stv);
        }

        void stv_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsLetterOrDigit(e.KeyChar) && labelEditGroupBox.Enabled)
            {
                e.Handled = true;
                labelEditRichEditBox.SelectedText = e.KeyChar.ToString();
                labelEditRichEditBox.Focus();
            }
        }

        void stv_TreeItemSelected(object sender, EventArgs e)
        {
            TreeItemSelectedEventArgs te = (TreeItemSelectedEventArgs)e;
            ItemSelect(te.Selected, te.Refocus);
            FireSelectionChanged();
        }

        public void ItemSelect(List<Elem> selected, bool refocus)
        {
            m_selected = selected;

            try
            {
                updating = true;
                alreadyChanged = false;
                tableLayoutPanel.SuspendLayout();

                labelEditRichEditBox.Reset();
                lexEditRichEditBox.Reset();

                ItemSelectLexEdit();
                ItemSelectNodeEdit(refocus);
                ItemSelectNodeActions();
                ItemSelectTracePane();
            }
            finally
            {
                updating = false;
                tableLayoutPanel.ResumeLayout(true);
                tableLayoutPanel.PerformLayout();
            }
        }

        public void ItemSelectTracePane()
        {
            bool enable = m_selected.Count == 1;
            traceGroupBox.Enabled = enable;
            if (enable)
            {
                tracePane.SetTraces(m_selected[0]);
            }
            else tracePane.SetTraces(null);
        }

        public void ItemSelectNodeActions()
        {
            /*todo: allow multi-delete?*/
            bool enable = m_selected.Count == 1;
            if (enable)
            {
                Elem elem = m_selected[0];
                createChildButton.Enabled = elem.GetNode().CanAddChildHere();
                deleteButton.Enabled = elem.GetNode().CanBeDeleted();
            }
            nodeOperationsGroupBox.Enabled = enable;
        }

        public void ItemSelectLexEdit()
        {
            bool enable = m_selected.Count == 1 && m_selected[0].NumChildren() == 0;
            lexEditGroupBox.Enabled = enable;
            if (enable)
            {
                if (m_selected[0].NumChildren() == 0)
                {
                    triangleCheckBox.Checked = m_selected[0].GetNode().GetDisplayType() == NodeDisplayType.Triangle;

                    //lexEditRichEditBox.Reset();
                    lexEditRichEditBox.Rtf = m_selected[0].GetNode().GetLexicalRtf();
                    if (lexEditRichEditBox.TextLength == 0)
                    {
                        lexEditRichEditBox.SelectAll();
                        lexEditRichEditBox.SelectionColor = m_stv.GetCurrentOptions().lexicalfont.Color;
                        lexEditRichEditBox.SelectionFont = m_stv.GetCurrentOptions().lexicalfont.Font;

                    }
                    lexEditRichEditBox.Modified = false;
                }
            }
            else
                triangleCheckBox.Checked = false;
        }

        public void ItemSelectNodeEdit(bool refocus)
        {
            bool enable;
            enable = m_selected.Count == 1;
            labelEditGroupBox.Enabled = enable;
            if (enable)
            {
                //labelEditRichEditBox.Reset();
                labelEditRichEditBox.Rtf = m_selected[0].GetNode().GetLabelRtf();
                if (labelEditRichEditBox.TextLength == 0)
                {
                    labelEditRichEditBox.SelectAll();
                    labelEditRichEditBox.SelectionColor = m_stv.GetCurrentOptions().labelfont.Color;
                    labelEditRichEditBox.SelectionFont = m_stv.GetCurrentOptions().labelfont.Font;
                }
                labelEditRichEditBox.Modified = false;
                if (refocus)
                    labelEditRichEditBox.Focus();
                labelEditRichEditBox.SelectAll();
            }
        }


        private void InitializeComponent()
        {
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.labelEditGroupBox = new System.Windows.Forms.GroupBox();
            this.lexEditGroupBox = new System.Windows.Forms.GroupBox();
            this.triangleCheckBox = new System.Windows.Forms.CheckBox();
            this.nodeOperationsGroupBox = new System.Windows.Forms.GroupBox();
            this.nodeToolStrip = new System.Windows.Forms.ToolStrip();
            this.createChildButton = new System.Windows.Forms.ToolStripButton();
            this.createParentButton = new System.Windows.Forms.ToolStripButton();
            this.deleteButton = new System.Windows.Forms.ToolStripButton();
            this.traceGroupBox = new System.Windows.Forms.GroupBox();
            this.labelEditRichEditBox = new Tree.RichTextBox2();
            this.lexEditRichEditBox = new Tree.RichTextBox2();
            this.tracePane = new Tree.TracePane();
            this.tableLayoutPanel.SuspendLayout();
            this.labelEditGroupBox.SuspendLayout();
            this.lexEditGroupBox.SuspendLayout();
            this.nodeOperationsGroupBox.SuspendLayout();
            this.nodeToolStrip.SuspendLayout();
            this.traceGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel.Controls.Add(this.labelEditGroupBox, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.lexEditGroupBox, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.nodeOperationsGroupBox, 0, 2);
            this.tableLayoutPanel.Controls.Add(this.traceGroupBox, 0, 3);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 4;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.Size = new System.Drawing.Size(183, 551);
            this.tableLayoutPanel.TabIndex = 0;
            this.tableLayoutPanel.Layout += new System.Windows.Forms.LayoutEventHandler(this.tableLayoutPanel1_Layout);
            // 
            // labelEditGroupBox
            // 
            this.labelEditGroupBox.Controls.Add(this.labelEditRichEditBox);
            this.labelEditGroupBox.Location = new System.Drawing.Point(3, 3);
            this.labelEditGroupBox.Name = "labelEditGroupBox";
            this.labelEditGroupBox.Size = new System.Drawing.Size(175, 65);
            this.labelEditGroupBox.TabIndex = 1;
            this.labelEditGroupBox.TabStop = false;
            this.labelEditGroupBox.Text = "Label";
            // 
            // lexEditGroupBox
            // 
            this.lexEditGroupBox.Controls.Add(this.triangleCheckBox);
            this.lexEditGroupBox.Controls.Add(this.lexEditRichEditBox);
            this.lexEditGroupBox.Location = new System.Drawing.Point(3, 74);
            this.lexEditGroupBox.Name = "lexEditGroupBox";
            this.lexEditGroupBox.Size = new System.Drawing.Size(175, 64);
            this.lexEditGroupBox.TabIndex = 2;
            this.lexEditGroupBox.TabStop = false;
            this.lexEditGroupBox.Text = "Lexical item";
            // 
            // triangleCheckBox
            // 
            this.triangleCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.triangleCheckBox.Dock = System.Windows.Forms.DockStyle.Right;
            this.triangleCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.triangleCheckBox.Location = new System.Drawing.Point(145, 18);
            this.triangleCheckBox.Name = "triangleCheckBox";
            this.triangleCheckBox.Size = new System.Drawing.Size(27, 43);
            this.triangleCheckBox.TabIndex = 1;
            this.triangleCheckBox.Text = "/\\";
            this.triangleCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.triangleCheckBox.UseVisualStyleBackColor = true;
            this.triangleCheckBox.CheckedChanged += new System.EventHandler(this.triangleCheckBox_CheckedChanged);
            // 
            // nodeOperationsGroupBox
            // 
            this.nodeOperationsGroupBox.Controls.Add(this.nodeToolStrip);
            this.nodeOperationsGroupBox.Location = new System.Drawing.Point(3, 144);
            this.nodeOperationsGroupBox.Name = "nodeOperationsGroupBox";
            this.nodeOperationsGroupBox.Size = new System.Drawing.Size(175, 53);
            this.nodeOperationsGroupBox.TabIndex = 4;
            this.nodeOperationsGroupBox.TabStop = false;
            this.nodeOperationsGroupBox.Text = "Node";
            // 
            // nodeToolStrip
            // 
            this.nodeToolStrip.CanOverflow = false;
            this.nodeToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.nodeToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createChildButton,
            this.createParentButton,
            this.deleteButton});
            this.nodeToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.nodeToolStrip.Location = new System.Drawing.Point(3, 18);
            this.nodeToolStrip.Name = "nodeToolStrip";
            this.nodeToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.nodeToolStrip.Size = new System.Drawing.Size(169, 31);
            this.nodeToolStrip.Stretch = true;
            this.nodeToolStrip.TabIndex = 3;
            // 
            // createChildButton
            // 
            this.createChildButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.createChildButton.Image = global::Tree.Properties.Resources.nodeInsertChild;
            this.createChildButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.createChildButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.createChildButton.Name = "createChildButton";
            this.createChildButton.Size = new System.Drawing.Size(28, 28);
            this.createChildButton.ToolTipText = "Insert child";
            this.createChildButton.Click += new System.EventHandler(this.createChildButton_Click);
            // 
            // createParentButton
            // 
            this.createParentButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.createParentButton.Image = global::Tree.Properties.Resources.nodeInsertParent;
            this.createParentButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.createParentButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.createParentButton.Name = "createParentButton";
            this.createParentButton.Size = new System.Drawing.Size(28, 28);
            this.createParentButton.ToolTipText = "Insert parent";
            this.createParentButton.Click += new System.EventHandler(this.createParentButton_Click);
            // 
            // deleteButton
            // 
            this.deleteButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.deleteButton.Image = global::Tree.Properties.Resources.nodeDelete;
            this.deleteButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.deleteButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(28, 28);
            this.deleteButton.ToolTipText = "Delete node";
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // traceGroupBox
            // 
            this.traceGroupBox.Controls.Add(this.tracePane);
            this.traceGroupBox.Location = new System.Drawing.Point(3, 203);
            this.traceGroupBox.Name = "traceGroupBox";
            this.traceGroupBox.Size = new System.Drawing.Size(175, 100);
            this.traceGroupBox.TabIndex = 5;
            this.traceGroupBox.TabStop = false;
            this.traceGroupBox.Text = "Traces";
            // 
            // labelEditRichEditBox
            // 
            this.labelEditRichEditBox.Bold = Tree.Ternary.No;
            this.labelEditRichEditBox.DetectUrls = false;
            this.labelEditRichEditBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelEditRichEditBox.Italic = Tree.Ternary.No;
            this.labelEditRichEditBox.Location = new System.Drawing.Point(3, 18);
            this.labelEditRichEditBox.MaxLength = 1024;
            this.labelEditRichEditBox.Name = "labelEditRichEditBox";
            this.labelEditRichEditBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.labelEditRichEditBox.Size = new System.Drawing.Size(169, 44);
            this.labelEditRichEditBox.Subscript = Tree.Ternary.No;
            this.labelEditRichEditBox.Superscript = Tree.Ternary.No;
            this.labelEditRichEditBox.TabIndex = 0;
            this.labelEditRichEditBox.Text = "";
            this.labelEditRichEditBox.Underline = Tree.Ternary.No;
            this.labelEditRichEditBox.WordWrap = false;
            this.labelEditRichEditBox.SelectionChanged += new System.EventHandler(this.labelEditRichEditBox_SelectionChanged);
            this.labelEditRichEditBox.TextChanged += new System.EventHandler(this.labelEditRichEditBox_TextChanged);
            // 
            // lexEditRichEditBox
            // 
            this.lexEditRichEditBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lexEditRichEditBox.Bold = Tree.Ternary.No;
            this.lexEditRichEditBox.DetectUrls = false;
            this.lexEditRichEditBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lexEditRichEditBox.Italic = Tree.Ternary.No;
            this.lexEditRichEditBox.Location = new System.Drawing.Point(3, 18);
            this.lexEditRichEditBox.MaxLength = 1024;
            this.lexEditRichEditBox.Multiline = false;
            this.lexEditRichEditBox.Name = "lexEditRichEditBox";
            this.lexEditRichEditBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.lexEditRichEditBox.Size = new System.Drawing.Size(136, 40);
            this.lexEditRichEditBox.Subscript = Tree.Ternary.No;
            this.lexEditRichEditBox.Superscript = Tree.Ternary.No;
            this.lexEditRichEditBox.TabIndex = 0;
            this.lexEditRichEditBox.Text = "";
            this.lexEditRichEditBox.Underline = Tree.Ternary.No;
            this.lexEditRichEditBox.WordWrap = false;
            this.lexEditRichEditBox.SelectionChanged += new System.EventHandler(this.lexEditRichEditBox_SelectionChanged);
            this.lexEditRichEditBox.TextChanged += new System.EventHandler(this.lexEditRichEditBox_TextChanged);
            // 
            // tracePane
            // 
            this.tracePane.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tracePane.Location = new System.Drawing.Point(3, 18);
            this.tracePane.Name = "tracePane";
            this.tracePane.Size = new System.Drawing.Size(169, 79);
            this.tracePane.TabIndex = 0;
            this.tracePane.OnTraceModified += new System.EventHandler(this.tracePane_OnTraceModified);
            // 
            // NodeEditor
            // 
            this.Controls.Add(this.tableLayoutPanel);
            this.Name = "NodeEditor";
            this.Size = new System.Drawing.Size(183, 551);
            this.tableLayoutPanel.ResumeLayout(false);
            this.labelEditGroupBox.ResumeLayout(false);
            this.lexEditGroupBox.ResumeLayout(false);
            this.nodeOperationsGroupBox.ResumeLayout(false);
            this.nodeOperationsGroupBox.PerformLayout();
            this.nodeToolStrip.ResumeLayout(false);
            this.nodeToolStrip.PerformLayout();
            this.traceGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void tableLayoutPanel1_Layout(object sender, LayoutEventArgs e)
        {
            tableLayoutPanel.SuspendLayout();
            foreach (Control n in tableLayoutPanel.Controls)
            {
                n.Width = this.tableLayoutPanel.Width -
                    this.tableLayoutPanel.Margin.Left
                    - this.tableLayoutPanel.Margin.Right;

            }
            tableLayoutPanel.ResumeLayout(true);
        }

        private void labelEditRichEditBox_TextChanged(object sender, EventArgs e)
        {
            if (!updating)
                if (m_selected.Count == 1 && labelEditRichEditBox.Enabled)
                {
                    Elem elem = m_selected[0];
                    Node node = elem.GetNode();
                    elem.FreeCache();
                    string labelRtf = labelEditRichEditBox.Rtf;
                    string labelText = labelEditRichEditBox.Text;
                    if (labelEditRichEditBox.Modified && node.GetLabelRtf() != labelRtf)
                    {
                        SelectivelyAddUndo();
                        node.SetLabelRtfAndText(labelEditRichEditBox.Rtf, labelEditRichEditBox.Text);
                        tracePane.RefreshData();
                        m_stv.UpdateEverything(true);
                    }
                }
        }

        private void lexEditRichEditBox_TextChanged(object sender, EventArgs e)
        {
            if (!updating)
                if (m_selected.Count == 1 && lexEditRichEditBox.Enabled)
                {
                    Elem elem = m_selected[0];
                    Node node = elem.GetNode();
                    elem.FreeCache();
                    string lexRtf = lexEditRichEditBox.Rtf;
                    string lexText = lexEditRichEditBox.Text;
                    if (lexEditRichEditBox.Modified && node.GetLexicalRtf() != lexRtf)
                    {
                        SelectivelyAddUndo();
                        node.SetLexicalRtfAndText(lexEditRichEditBox.Rtf, lexEditRichEditBox.Text);
                        ItemSelectNodeActions();
                        tracePane.RefreshData();
                        m_stv.UpdateEverything(true);
                    }
                }

        }

        private void triangleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (m_selected.Count == 1 && lexEditGroupBox.Enabled && !updating)
            {
                m_selected[0].GetNode().SetDisplayType(triangleCheckBox.Checked ?
                    NodeDisplayType.Triangle : NodeDisplayType.Normal);
                m_selected[0].FreeCache();
                ItemSelectNodeActions();
                lexEditRichEditBox.Focus();
                m_stv.UpdateEverything(true);
            }

        }

        private void SelectivelyAddUndo()
        {
            if (!alreadyChanged)
            {
                alreadyChanged = true;
                m_stv.UndoAdd();
            }
        }


        private void createChildButton_Click(object sender, EventArgs e)
        {
            m_stv.DoAddChild(true);
        }

        private void createParentButton_Click(object sender, EventArgs e)
        {
            m_stv.DoAddParent(true);
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            m_stv.DoDeleteNode(true);
        }


        private void labelEditRichEditBox_SelectionChanged(object sender, EventArgs e)
        {
            lastTouchedWasLex = false;
            FireSelectionChanged();
        }

        private void lexEditRichEditBox_SelectionChanged(object sender, EventArgs e)
        {
            lastTouchedWasLex = true;
            FireSelectionChanged();
        }

        private void FireSelectionChanged()
        {
            if (!updating)
                if (SelectionChanged != null)
                    SelectionChanged(this, new EventArgs());
        }

        private void tracePane_OnTraceModified(object sender, EventArgs e)
        {
            m_stv.UpdateTraces();
            m_stv.UpdateEverything(true);
            tracePane.RefreshData();
        }

    }
}
