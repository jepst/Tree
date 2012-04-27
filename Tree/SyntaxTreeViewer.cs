using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Tree
{
    public class SyntaxTreeViewer : UserControl
    {
        public event EventHandler TreeStructureChanged;
        public event EventHandler TreeItemSelected;

        private ContextMenuStrip basicMenuStrip;
        private ToolStripMenuItem basicMenuStripAddChild;
        private ToolStripMenuItem basicMenuStripAddParent;
        private ToolStripMenuItem basicMenuStripDeleteItem;
        private ToolStripMenuItem basicMenuStripCut;
        private ToolStripMenuItem basicMenuStripCopy;
        private ToolStripMenuItem basicMenuStripPaste;
        private ToolStripMenuItem basicMenuStripAddTrace;
        private ToolStripMenuItem basicMenuStripDecorate;

        private SyntaxTree m_syntax;
        private Dummy m_richtext;
        private Elem m_elemRoot;
        private int m_height;
        private List<List<Elem>> m_elemByLevel;
        private Size[] m_levelDimensions;
        private List<Elem> m_selected;
        private Rectangle m_maximumExtents;

        private MouseMode m_mousemode;
        private RequestForElem m_requestforelem;
        private Point m_mousedownat;
        private Rectangle m_selectionrect;

        private float zoom;
        private UndoManager undo;

        private Pen m_gdi_linepen;
        private Pen m_gdi_redpen;
        private SolidBrush m_gdi_highlightbgpen;
        private HintBox m_hintbox;

        public SyntaxTreeViewer()
            : base()
        {
            //SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            //SetStyle(ControlStyles.UserPaint, true);
            //SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            InitializeComponent();
            InitDummyComponent();
            InitMenus();

            undo = new UndoManager(this);
            ResizeRedraw = true;
            m_mousemode = MouseMode.None;
            zoom = 1.0f;

            m_syntax = new SyntaxTree();

            if (m_syntax != null)
            {
                UpdateTreeData();
                FireStructureChanged();
            }

        }

        private Elem GetLefterNode(Elem elem)
        {
            for (int i = 0; i < m_elemByLevel[elem.level].Count; i++)
            {
                if (m_elemByLevel[elem.level][i] == elem && i != 0)
                    return m_elemByLevel[elem.level][i - 1];
            }
            return null;
        }
        private Elem GetRighterNode(Elem elem)
        {
            for (int i = 0; i < m_elemByLevel[elem.level].Count - 1; i++)
            {
                if (m_elemByLevel[elem.level][i] == elem)
                    return m_elemByLevel[elem.level][i + 1];
            }
            return null;
        }

        private void ScrollToElem(Elem elem)
        {
            if (elem == null)
                return;
            Point p = elem.Location;

            if (!ClientRectangle.Contains(AdjustForScroll(elem.Rect)))
            {
                AutoScrollPosition = new Point(p.X - ClientSize.Width / 2, p.Y - ClientSize.Height / 2);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Left ||
                keyData == Keys.Right ||
                keyData == Keys.Up ||
                keyData == Keys.Down)
            {
                if (m_selected.Count != 1)
                {
                    m_selected.Clear();
                    m_selected.Add(m_elemRoot);
                    Invalidate();
                    ScrollToElem(m_elemRoot);
                    FireSelectionChange(false);
                }
                else
                {
                    Elem t = null;
                    switch (keyData)
                    {
                        case Keys.Up:
                            t = m_selected[0].Parent;
                            break;
                        case Keys.Down:
                            t = WlkLeftmostChild(m_selected[0]);
                            break;
                        case Keys.Right:
                            t = GetRighterNode(m_selected[0]);
                            break;
                        case Keys.Left:
                            t = GetLefterNode(m_selected[0]);
                            break;
                    }
                    if (t != null)
                    {
                        Invalidate(AdjustForScroll(m_selected[0].Rect));
                        m_selected[0] = t;
                        Invalidate(AdjustForScroll(m_selected[0].Rect));
                        ScrollToElem(m_selected[0]);
                        FireSelectionChange();
                    }
                }
                return true;
            }
            else
                return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Dummy.ReleaseDummy();
                ReleaseGDIResources();
                basicMenuStrip.Dispose();

                m_elemRoot.Dispose(); m_elemRoot = null;
            }
            base.Dispose(disposing);
        }

        public bool UndoUndo(bool b)
        {
            return undo.DoUndo(b);
        }

        public bool UndoRedo(bool b)
        {
            return undo.DoRedo(b);
        }

        public void UndoAdd()
        {
            undo.Add();
        }

        public List<Elem> GetSelection()
        {
            return m_selected;
        }

        public void ReleaseGDIResources()
        {
            if (m_gdi_linepen != null)
            {
                m_gdi_linepen.Dispose();
                m_gdi_linepen = null;
            }
            if (m_gdi_redpen != null)
            {
                m_gdi_redpen.Dispose();
                m_gdi_redpen = null;
            }
            if (m_gdi_highlightbgpen != null)
            {
                m_gdi_highlightbgpen.Dispose();
                m_gdi_highlightbgpen = null;
            }
            if (m_hintbox != null)
            {
                m_hintbox.Dispose();
                m_hintbox = null;
            }
        }

        private void InitMenus()
        {
            basicMenuStrip = new ContextMenuStrip();
            basicMenuStrip.SuspendLayout();
            basicMenuStrip.Name = "basicMenuStrip";
            basicMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(basicMenuStrip_Opening);

            basicMenuStripAddChild = new ToolStripMenuItem();
            basicMenuStripAddChild.Text = "Insert &child";
            basicMenuStripAddChild.Click += new EventHandler(basicMenuStripAddChild_Click);

            basicMenuStripAddParent = new ToolStripMenuItem();
            basicMenuStripAddParent.Text = "Insert &parent";
            basicMenuStripAddParent.Click += new EventHandler(basicMenuStripAddParent_Click);

            basicMenuStripDeleteItem = new ToolStripMenuItem();
            basicMenuStripDeleteItem.Text = "&Delete";
            basicMenuStripDeleteItem.Click += new EventHandler(basicMenuStripDeleteItem_Click);
            //---------------------
            basicMenuStripCut = new ToolStripMenuItem();
            basicMenuStripCut.Text = "C&ut";
            basicMenuStripCut.Click += new EventHandler(basicMenuStripCut_Click);

            basicMenuStripCopy = new ToolStripMenuItem();
            basicMenuStripCopy.Text = "C&opy";
            basicMenuStripCopy.Click += new EventHandler(basicMenuStripCopy_Click);

            basicMenuStripPaste = new ToolStripMenuItem();
            basicMenuStripPaste.Text = "P&aste";
            basicMenuStripPaste.Click += new EventHandler(basicMenuStripPaste_Click);

            basicMenuStripAddTrace = new ToolStripMenuItem();
            basicMenuStripAddTrace.Text = "Add &trace from here...";
            basicMenuStripAddTrace.Click += new EventHandler(basicMenuStripAddTrace_Click);

            basicMenuStripDecorate = new ToolStripMenuItem();
            basicMenuStripDecorate.Text = "D&ecoration...";
            basicMenuStripDecorate.Click += new EventHandler(basicMenuStripDecorate_Click);

            basicMenuStrip.Items.AddRange(new ToolStripItem[] {
                    basicMenuStripAddChild,
                    basicMenuStripAddParent,
                    basicMenuStripDeleteItem,
                new ToolStripSeparator(),
                basicMenuStripCut,
                basicMenuStripCopy,
                basicMenuStripPaste,
                new ToolStripSeparator(),
                basicMenuStripAddTrace,
                basicMenuStripDecorate,
                });

            basicMenuStrip.ResumeLayout(false);
        }


        private void InitDummyComponent()
        {
            m_richtext = Dummy.Init(this);
        }

        public bool Dirty
        {
            get
            {
                return m_syntax.Dirty;
            }
            set
            {
                m_syntax.Dirty = value;
            }
        }

        public void UpdateEverything(bool gentle)
        {
            if (gentle)
                UpdateTreePositions();
            else
                UpdateTreeData();

            FireStructureChanged();
            Invalidate();
        }


        private void UpdateTreeData()
        {
            m_height = 0;

            bool somethingSelected = false;
            if (m_selected == null)
                m_selected = new List<Elem>();
            else
            {
                if (m_selected.Count > 0)
                    somethingSelected = true;
                m_selected.Clear();
            }

            if (m_elemByLevel == null)
                m_elemByLevel = new List<List<Elem>>();
            else
                m_elemByLevel.Clear();
            if (m_elemRoot != null)
            {
                m_elemRoot.Dispose();
                m_elemRoot = null;
            }
            m_elemRoot = ScanWorker(m_syntax.Root, m_elemByLevel, 1);
            m_levelDimensions = new Size[m_height];
            for (int i = 0; i < m_height; i++)
                m_levelDimensions[i] = GetLevelDimensions(i);

            UpdateTraces();


            OptTreeDraw1(m_elemRoot);
            if (somethingSelected)
                FireSelectionChange();
        }

        public void UpdateTraces()
        {
            foreach (Elem elem in Elements())
                elem.ClearTraceData();
            UpdateTraces(m_elemRoot);
        }

        private void UpdateTraces(Elem elem)
        {
            foreach (Elem e in elem.Children())
                UpdateTraces(e);
            UpdateTracesWorker(elem);
        }

        private void InitElemTrace(ElemTrace et)
        {
            Elem a = et.source;
            Elem b = et.destination;
            Elem olda = null;
            Elem oldb = null;
            if (a.level < b.level)
            {
                Elem c = b;
                b = a;
                a = c;
            }

            bool leftofaexceeded = false;
            bool rightofaexceeded = false;
            bool leftofbexceeded = false;
            bool rightofbexceeded = false;
            int avertices = 0;
            int bvertices = 0;
            while (a.level > b.level)
                if (a.Parent != null)
                {
                    if (!leftofaexceeded)
                        foreach (Elem x in a.LeftSiblings())
                            if (x.altitude > avertices)
                            {
                                leftofaexceeded = true;
                                break;
                            }
                    if (!rightofaexceeded)
                        foreach (Elem x in a.RightSiblings())
                            if (x.altitude > avertices)
                            {
                                rightofaexceeded = true;
                                break;
                            }

                    avertices++;
                    olda = a;
                    a = a.Parent;
                }
                else
                    throw new TreeException("Apparently looking for common ancestor of unrelated nodes");
            while (a != b)
            {
                if (a.Parent != null && b.Parent != null && a.Parent == b.Parent)
                {
                    foreach (Elem x in a.Parent.Children())
                    {
                        int xn = x.GetNumber();
                        if (xn > a.GetNumber() && xn < b.GetNumber())
                        {
                            if (x.altitude > avertices)
                                rightofaexceeded = true;
                            if (x.altitude > bvertices)
                                leftofbexceeded = true;
                        }
                        if (xn < a.GetNumber() && xn > b.GetNumber())
                        {
                            if (x.altitude > avertices)
                                leftofaexceeded = true;
                            if (x.altitude > bvertices)
                                rightofbexceeded = true;
                        }
                    }
                }
                else
                {
                    if (a.Parent != null)
                    {
                        if (!leftofaexceeded)
                            foreach (Elem x in a.LeftSiblings())
                                if (x.altitude > avertices)
                                {
                                    leftofaexceeded = true;
                                    break;
                                }
                        if (!rightofaexceeded)
                            foreach (Elem x in a.RightSiblings())
                                if (x.altitude > avertices)
                                {
                                    rightofaexceeded = true;
                                    break;
                                }
                    }
                    else
                        throw new TreeException("Funny looking tree A");

                    if (b.Parent != null)
                    {

                        if (!leftofbexceeded)
                            foreach (Elem x in b.LeftSiblings())
                                if (x.altitude > bvertices)
                                {
                                    leftofbexceeded = true;
                                    break;
                                }
                        if (!rightofbexceeded)
                            foreach (Elem x in b.RightSiblings())
                                if (x.altitude > bvertices)
                                {
                                    rightofbexceeded = true;
                                    break;
                                }
                    }
                    else
                        throw new TreeException("Funny looking tree B");
                }
                avertices++;
                olda = a;
                a = a.Parent;
                bvertices++;
                oldb = b;
                b = b.Parent;
            }
            if (olda != null && oldb != null)
            {
                int apos = a.GetNode().FindChild(olda.GetNode());
                int bpos = a.GetNode().FindChild(oldb.GetNode());
                if (apos < bpos)
                    et.longextent = rightofaexceeded || leftofbexceeded;
                else
                    et.longextent = leftofaexceeded || rightofbexceeded;
            }
            else
                et.longextent = false;
            et.ancestor = a;
        }


        private void UpdateTracesWorker(Elem elem)
        {
            //elem.m_traces.Clear()
            Node node = elem.GetNode();
            if (node.NumTraces() > 0)
            {
                foreach (Trace trace in node.Traces())
                {
                    if (trace.source == node)
                    {
                        foreach (Elem possibledest in Elements())
                        {
                            if (trace.destination == possibledest.GetNode())
                            {
                                ElemTrace newtrace = new ElemTrace(elem, possibledest, trace);
                                InitElemTrace(newtrace);
                                newtrace.sourcexth = elem.tracecount++;
                                newtrace.destinationxth = possibledest.tracecount++;
                                newtrace.ancestor.Add(newtrace);
                                goto foo;
                            }
                        }
                        throw new TreeException("Unfound destination");

                    }
                foo: ;
                }
            }
        }

        private void WlkTreeLayout()
        {
            foreach (Elem elem in Elements())
            {
                elem.mod = 0;
                elem.thread = null;
                elem.ancestor = elem;
                elem.prelim = 0;
                elem.change = 0;
                elem.shift = 0;
            }
            WlkFirstWalk(m_elemRoot);

            int maxExtent = 0, minExtent = 0;
            WlkFindExtents(m_elemRoot, -m_elemRoot.prelim, 0, ref maxExtent, ref minExtent);

        }

        private void WlkTreeDraw(Size totalSize, Point origin)
        {
            WlkSecondWalk(m_elemRoot, -m_elemRoot.prelim, 0,
                new Point(origin.X - totalSize.Width / 2 + (-m_elemRoot.shift /*really minExtent*/),
                origin.Y));
        }

        private void WlkFindExtents(Elem node, int m, int depth, ref int max, ref int min)
        {
            int thismin = m + node.prelim;
            int thismax = m + node.prelim + node.Dimensions.Width;
            int maxheight = 0;

            foreach (Elem elem in node.Children())
            {
                int localmin = 0, localmax = 0;
                WlkFindExtents(elem, node.mod + m, depth + 1, ref localmax, ref localmin);
                thismin = Math.Min(thismin, localmin);
                thismax = Math.Max(thismax, localmax);
                maxheight = Math.Max(maxheight, elem.BranchDimensions.Height);
            }
            if (maxheight == 0)
                maxheight = m_levelDimensions[depth].Height;
            else
                maxheight += m_levelDimensions[depth].Height + MarginVertical();
            max = thismax;
            min = thismin;
            node.shift = min;
            node.BranchDimensions = new Size(max - min, maxheight);
        }


        private void WlkSecondWalk(Elem v, int m, int level, Point origin)
        {
            int myy = v.Parent == null ? origin.Y : v.Parent.Location.Y +
                m_levelDimensions[level - 1].Height + MarginVertical();
            v.Location = new Point(v.prelim + m + origin.X,
                myy);
            foreach (Elem w in v.Children())
                WlkSecondWalk(w, m + v.mod, level + 1, origin);

        }

        private Elem WlkRightmostChild(Elem e)
        {//this can be made much more efficient with direct acess to Node::m_children
            Elem rm = null;
            foreach (Elem elem in e.Children())
            {
                rm = elem;
            }
            return rm;
        }

        private Elem WlkLeftmostChild(Elem e)
        {//this can be made much more efficient with direct acess to Node::m_children
            foreach (Elem elem in e.Children())
            {
                return elem;
            }
            return null;
        }

        private Elem WlkGetLeftMostSibling(Elem e)
        {/*optimize this too*/
            if (e.Parent != null)
                foreach (Elem elem in e.Parent.Children())
                    return elem;
            return null;
        }

        private void WlkExecuteShifts(Elem v)
        {
            int shift = 0;
            int change = 0;
            List<Elem> reverse = new List<Elem>(v.NumChildren());
            foreach (Elem e in v.Children()) /*and this monstrosity. ug!*/
                reverse.Add(e);
            reverse.Reverse();
            foreach (Elem w in reverse)
            {
                w.prelim = w.prelim + shift;
                w.mod = w.mod + shift;
                change = change + w.change;
                shift = shift + w.shift + change;
            }
        }

        private Elem WlkGetLeftSibling(Elem e)
        {/*and this*/
            if (e.Parent != null)
            {
                Elem last = null;
                foreach (Elem elem in e.Parent.Children())
                {
                    if (elem == e)
                        break;
                    last = elem;
                }
                return last;
            }
            return null;
        }

        private Elem WlkGetRightSibling(Elem e)
        {/*and this*/
            if (e.Parent != null)
            {
                bool last = false;
                foreach (Elem elem in e.Parent.Children())
                {
                    if (last)
                        return elem;
                    if (elem == e)
                        last = true;
                }
            }
            return null;
        }

        private void WlkFirstWalk(Elem v)
        {
            if (v.NumChildren() == 0)
            {
                v.prelim = 0;
                Elem left = WlkGetLeftSibling(v);
                if (left != null)
                    v.prelim = left.prelim + MarginHorizontal() + left.Dimensions.Width;
            }
            else
            {
                Elem leftmostChild = WlkLeftmostChild(v);
                Elem rightmostChild = WlkRightmostChild(v);
                Elem defaultAncestor = leftmostChild;
                foreach (Elem w1 in v.Children())
                {
                    WlkFirstWalk(w1);
                    WlkApportion(w1, defaultAncestor);
                }
                WlkExecuteShifts(v);
                int midpoint = (leftmostChild.prelim + leftmostChild.Dimensions.Width / 2 +
                    rightmostChild.prelim + rightmostChild.Dimensions.Width / 2)
                    - v.Dimensions.Width;
                midpoint /= 2;

                Elem w = WlkGetLeftSibling(v);
                if (w != null)
                {
                    v.prelim = w.prelim + MarginHorizontal() + w.Dimensions.Width;
                    v.mod = v.prelim - midpoint;
                }
                else
                    v.prelim = midpoint;

            }
        }

        private Elem WlkNextLeft(Elem v)
        {
            if (v.NumChildren() > 0)
                return WlkLeftmostChild(v);
            else
                return v.thread;
        }

        private Elem WlkNextRight(Elem v)
        {
            if (v.NumChildren() > 0)
                return WlkRightmostChild(v);
            else
                return v.thread;
        }

        private void WlkApportion(Elem v, Elem defaultAncestor)
        {
            Elem w = WlkGetLeftSibling(v);
            Elem viplus, voplus, viminus, vominus;
            if (w != null)
            {
                int siplus, soplus, siminus, sominus;
                viplus = voplus = v;
                viminus = w;
                vominus = WlkGetLeftMostSibling(viplus);
                siplus = viplus.mod;
                soplus = voplus.mod;
                siminus = viminus.mod;
                sominus = vominus.mod;
                while (WlkNextRight(viminus) != null && WlkNextLeft(viplus) != null)
                {
                    viminus = WlkNextRight(viminus);
                    viplus = WlkNextLeft(viplus);
                    vominus = WlkNextLeft(vominus);
                    voplus = WlkNextRight(voplus);
                    voplus.ancestor = v;
                    int shift = (viminus.prelim + siminus) - (viplus.prelim + siplus) +
                        viminus.Dimensions.Width + MarginHorizontal();
                    if (shift > 0)
                    {
                        WlkMoveSubtree(WlkAncestor(viminus, v, defaultAncestor), v, shift);
                        siplus = siplus + shift;
                        soplus = soplus + shift;
                    }
                    siminus = siminus + viminus.mod;
                    siplus = siplus + viplus.mod;
                    sominus = sominus + vominus.mod;
                    soplus = soplus + voplus.mod;
                }

                if (WlkNextRight(viminus) != null && WlkNextRight(voplus) == null)
                {
                    voplus.thread = WlkNextRight(viminus);
                    voplus.mod = voplus.mod + siminus - soplus;
                }
                if (WlkNextLeft(viplus) != null && WlkNextLeft(vominus) == null)
                {
                    vominus.thread = WlkNextLeft(viplus);
                    vominus.mod = vominus.mod + siplus - sominus;
                    defaultAncestor = v;
                }
            }
        }

        private void WlkMoveSubtree(Elem wminus, Elem wplus, int shift)
        {
            int subtrees = wplus.GetNumber() - wminus.GetNumber() - 1;

            if (subtrees < 1)
                subtrees = 1;

            wplus.change = wplus.change - shift / subtrees;
            wplus.shift = wplus.shift + shift;
            wminus.change = wminus.change + shift / subtrees;
            wplus.prelim = wplus.prelim + shift;
            wplus.mod = wplus.mod + shift;
        }


        private Elem WlkAncestor(Elem viminus, Elem v, Elem defaultAncestor)
        {
            if (viminus.ancestor.Parent == v.Parent && viminus.ancestor != v)
                return viminus.ancestor;
            else return defaultAncestor;
        }

        private void UpdateTreePositions()
        {
            foreach (Elem e in Elements())
                CalcElementDimensions(e);
            m_levelDimensions = new Size[m_height];
            for (int i = 0; i < m_height; i++)
                m_levelDimensions[i] = GetLevelDimensions(i);

            OptTreeDraw1(m_elemRoot);
        }

        private void OptTreeDraw1(Elem root)
        {
            if (ShouldDoWalkerTree())
                WlkTreeLayout();
            else
                StdCalcBranchDimensions(m_elemRoot);

        }


        private Elem ScanWorker(Node n, List<List<Elem>> dic, int depth)
        {
            Elem elem = new Elem(n);
            CalcElementDimensions(elem);
            elem.level = depth - 1;
            List<Elem> list;
            while (dic.Count < depth)
            {
                list = new List<Elem>();
                dic.Add(list);
            }
            list = dic[depth - 1];
            list.Add(elem);
            m_height = Math.Max(m_height, depth);

            int maxaltitude = 0;
            foreach (Node child in n.Children())
            {
                Elem newelem = ScanWorker(child, dic, depth + 1);
                maxaltitude = Math.Max(maxaltitude, newelem.altitude + 1);
                elem.Add(newelem);
            }
            elem.altitude = maxaltitude;

            return elem;
        }

        private void StdCalcBranchDimensions(Elem elem)
        {
            CalcBranchDimensions(elem, 0);
        }
        private void CalcBranchDimensions(Elem elem, int depth)
        {
            Node n = elem.GetNode();
            if (!n.HasChildren())
                elem.BranchDimensions = elem.Dimensions;
            else
            {
                int wsum = 0; int hmax = 0; int wmax = 0;
                foreach (Elem child in elem.Children())
                {
                    CalcBranchDimensions(child, depth + 1);
                    wmax = Math.Max(wmax, child.BranchDimensions.Width);
                    hmax = Math.Max(hmax, child.BranchDimensions.Height);
                    wsum += MarginHorizontal() + child.BranchDimensions.Width;
                }
                if (RowsArranged())
                    hmax += m_levelDimensions[depth].Height + MarginVertical();
                else
                    hmax += elem.Dimensions.Height + MarginVertical();
                wsum -= MarginHorizontal();
                if (ShouldDoBalancedTree())
                {
                    foreach (Elem child in elem.Children())
                    {
                        if (wmax > child.BranchDimensions.Width)
                            CalcNodeInfoRecurse(child, child.BranchDimensions.Width, wmax);
                        child.BranchDimensions = new Size(wmax, child.BranchDimensions.Height);
                    }
                    wsum = (elem.GetNode().NumChildren()) *
                        wmax + (MarginHorizontal() * (elem.GetNode().NumChildren() - 1));
                }
                if (wsum < elem.Dimensions.Width)
                {
                    CalcNodeInfoRecurse(elem, wsum, elem.Dimensions.Width);
                    wsum = elem.Dimensions.Width;
                }
                elem.BranchDimensions = new Size(wsum, hmax);
            }

        }

        void CalcNodeInfoRecurse(Elem elem, int current, int target)
        {
            int nchildren = elem.GetNode().NumChildren();
            elem.BranchDimensions = new Size(target, elem.BranchDimensions.Height);
            if (nchildren == 0)
                return;
            int delta = target - current;
            int target_delta = delta / nchildren;
            foreach (Elem child in elem.Children())
            {
                int child_width = child.BranchDimensions.Width;
                CalcNodeInfoRecurse(child, child_width, child_width + target_delta);
            }
        }



        private void SelectByNode(List<Node> nodes)
        {
            m_selected.Clear();
            int found = 0;
            foreach (Elem elem in Elements())
            {
                if (nodes.Contains(elem.GetNode()))
                {
                    m_selected.Add(elem);
                    found++;
                    if (found >= nodes.Count)
                        break;
                }
            }
            FireSelectionChange();
        }

        private Elem SelectByNode(Node newnode)
        {
            m_selected.Clear();
            foreach (Elem elem in Elements())
            {
                if (elem.GetNode() == newnode)
                {
                    m_selected.Add(elem);
                    FireSelectionChange();
                    return elem;
                }
            }
            return null;
        }

        private void FireStructureChanged()
        {
            if (TreeStructureChanged != null)
                TreeStructureChanged(this, new EventArgs());
        }

        private void FireSelectionChange()
        {
            FireSelectionChange(false);
        }
        private void FireSelectionChange(bool shiftFocus)
        {
            if (TreeItemSelected != null)
                TreeItemSelected(this,
                    new TreeItemSelectedEventArgs(m_selected, this, shiftFocus));
        }

        public string GetBracketing()
        {
            return m_syntax.ToBracketing();
        }

        public TreeOptions GetCurrentOptions()
        {
            return m_syntax.m_options;
        }

        public void SetCurrentOptions(TreeOptions o)
        {
            m_syntax.m_options = o;
        }


        public SyntaxTree GetCurrentTree()
        {
            return m_syntax;
        }

        public void SetCurrentTree(SyntaxTree t)
        {
            SetCurrentTree(t, true);
        }

        internal void SetCurrentTree(SyntaxTree t, bool clearUndo)
        {
            if (clearUndo)
                undo.Clear();
            m_syntax = t;
            m_mousemode = MouseMode.None;
            ReleaseRequestForElem();
            ReleaseGDIResources();
            UpdateEverything(false);
        }

        public IEnumerable<Elem> Elements()
        {
            foreach (List<Elem> list in m_elemByLevel)
                foreach (Elem e in list)
                    yield return e;
        }

        public Size GetDimensions()
        {
            // this is wrong. do not use
            int maxwidth = 0;
            int sumheight = 0;
            for (int i = 0; i < m_height; i++)
            {
                Size thislevel = GetLevelDimensions(i);
                maxwidth = Math.Max(maxwidth, thislevel.Width);
                sumheight += thislevel.Height;
            }

            return new Size(maxwidth, sumheight);
        }

        public Size GetLevelDimensions(int n)
        {
            if (n >= m_elemByLevel.Count)
                throw new TreeException(string.Format("The tree has no level '{0}'", n));
            List<Elem> list = m_elemByLevel[n];
            int maxheight = 0;
            int sumwidth = 0;
            foreach (Elem e in list)
            {
                /*maxheight = Math.Max(maxheight,(e.GetNode().GetDisplayType()==NodeDisplayType.Normal)?
                    e.Dimensions.Height:e.Dimensions_label.Height);*/
                maxheight = Math.Max(maxheight, e.Dimensions.Height);
                sumwidth += e.Dimensions.Width + MarginHorizontal();
            }
            if (list.Count > 0)
                sumwidth -= MarginHorizontal();
            return new Size(sumwidth, maxheight);
        }

        private Pen StandardLinePen()
        {
            if (m_gdi_linepen == null)
            {
                PenStyle ps = m_syntax.m_options.linestyle; //make a copy, value semantics
                if (GetZoomFactor() != 1.0f)
                    ps.width *= GetZoomFactor();

                m_gdi_linepen = ps.GetPen();
            }
            return m_gdi_linepen;
        }

        private Pen BigXLinePen()
        {
            if (m_gdi_redpen == null)
                m_gdi_redpen = new Pen(Color.Red, 5);
            return m_gdi_redpen;
        }

        private SolidBrush HighlightBackBrush()
        {
            if (m_gdi_highlightbgpen == null)
                m_gdi_highlightbgpen = new SolidBrush(HighlightBackColor());
            return m_gdi_highlightbgpen;
        }

        private bool ShouldDoBalancedTree()
        {
            return false;
        }

        private bool ShouldDoWalkerTree()
        {
            return m_syntax.m_options.rendermode == RenderMode.Walker;
        }

        private bool RowsArranged()
        {
            return m_syntax.m_options.rendermode == RenderMode.OriginalAlign;
        }

        private void SetupAntiAliasing(GraphicsProxy g)
        {
            g.SmoothingMode = m_syntax.m_options.antialias ? SmoothingMode.AntiAlias : SmoothingMode.None;
        }

        private int PaddingHorizontal()
        {
            if (GetZoomFactor() != 1.0f)
                return (int)(m_syntax.m_options.paddinghorizontal * GetZoomFactor());
            else
                return m_syntax.m_options.paddinghorizontal;
        }

        private int PaddingVertical()
        {
            if (GetZoomFactor() != 1.0f)
                return (int)(m_syntax.m_options.paddingvertical * GetZoomFactor());
            else
                return m_syntax.m_options.paddingvertical;
        }

        private RenderMode GetRenderMode()
        {
            return m_syntax.m_options.rendermode;
        }

        public void SetZoomFactor(float n)
        {
            FreeAllCache();
            this.zoom = n;
        }

        public float GetZoomFactor()
        {
            return this.zoom;
        }

        private int TraceSpacingHorizontal()
        {
            if (GetZoomFactor() != 1.0f)
                return (int)(m_syntax.m_options.tracehorizontal * GetZoomFactor());
            else
                return m_syntax.m_options.tracehorizontal;
        }

        private int TraceSpacingVertical()
        {
            if (GetZoomFactor() != 1.0f)
                return (int)(m_syntax.m_options.tracevertical * GetZoomFactor());
            else
                return m_syntax.m_options.tracevertical;
        }


        private int MarginHorizontal()
        {
            if (GetZoomFactor() != 1.0f)
                return (int)(m_syntax.m_options.marginhorizontal * GetZoomFactor());
            else
                return m_syntax.m_options.marginhorizontal;
        }

        private int MarginVertical()
        {
            if (GetZoomFactor() != 1.0f)
                return (int)(m_syntax.m_options.marginvertical * GetZoomFactor());
            else
                return m_syntax.m_options.marginvertical;
        }

        private Color HighlightBackColor()
        {
            return m_syntax.m_options.highlightcolor;
        }

        private TreeAlignmentEnum TreeAlignment()
        {
            return m_syntax.m_options.treealignment;
        }

        private Color StandardBackColor()
        {
            return m_syntax.m_options.backgroundcolor;
        }

        public void FreeAllCache()
        {
            m_elemRoot.FreeCacheOnSubtree();
        }

        public void CalcElementDimensions(Elem elem)
        {
            if (elem.cache != null)
                return;
            Node node = elem.GetNode();

            int decorationAdjustment = (node.Decoration.mode == DecorationMode.Node) ?
                (int)node.Decoration.padding : 0;
            int triangleAdjustment = (node.GetDisplayType() == NodeDisplayType.Triangle) ?
                (PaddingVertical() * 2 + MarginVertical()) : 0;

            if (node.IsBlank())
            {
                /*this allows for drawing of the big red x; see PaintElement*/
                elem.Dimensions = new Size(20, 20);
                elem.Dimensions_label = elem.Dimensions_lex = Size.Empty;
            }
            else
                if (node.HasLexical() && node.HasLabel())
                {
                    m_richtext.Reset();
                    m_richtext.ZoomFactor = GetZoomFactor();
                    m_richtext.Rtf = node.GetLabelRtf();
                    m_richtext.FixupComponentLabel();
                    elem.Dimensions_label = EstimateSpace(m_richtext);

                    m_richtext.Reset();
                    m_richtext.ZoomFactor = GetZoomFactor();
                    m_richtext.Rtf = node.GetLexicalRtf();
                    m_richtext.FixupComponentLexical();
                    elem.Dimensions_lex = EstimateSpace(m_richtext);

                    elem.Dimensions = new Size(Math.Max(elem.Dimensions_lex.Width, elem.Dimensions_label.Width),
                        elem.Dimensions_lex.Height + elem.Dimensions_label.Height);
                }
                else
                    if (node.HasLabel())
                    {
                        m_richtext.Reset();
                        m_richtext.ZoomFactor = GetZoomFactor();
                        m_richtext.Rtf = node.GetLabelRtf();
                        m_richtext.FixupComponentLabel();
                        elem.Dimensions = elem.Dimensions_label = EstimateSpace(m_richtext);
                        elem.Dimensions_lex = Size.Empty;
                    }
                    else if (node.HasLexical())
                    {
                        m_richtext.Reset();
                        m_richtext.ZoomFactor = GetZoomFactor();
                        m_richtext.Rtf = node.GetLexicalRtf();
                        m_richtext.FixupComponentLexical();
                        elem.Dimensions = elem.Dimensions_lex = EstimateSpace(m_richtext);
                        elem.Dimensions_label = Size.Empty;
                    }
            elem.Dimensions = new Size(elem.Dimensions.Width + PaddingHorizontal() + decorationAdjustment, elem.Dimensions.Height + PaddingVertical() + decorationAdjustment + triangleAdjustment);
        }

        private Size EstimateSpace(RichTextBox2 c)
        {
            /*So. The original plan was just to use GetPreferredSize,
             * which, initially, seemed to correctly output the size
             * of the content of an RTF control. Little did I know.
             * In fact it only works correctly with the default font
             * size. So it was necessary to spent a few days figuring
             * shit out and eventually I wound up with the tepid
             * ugliness in RichTextBox2, which culminates in this
             * function: GetCleverSize.

            return c.GetPreferredSize(Size.Empty);
             * 
             */
            return c.GetCleverSize();
        }

        private class RenderNodePositionInfo
        {
            public Rectangle labelRect;
            public Rectangle lexRect;
            public RenderNodePositionInfo(Rectangle label, Rectangle lex)
            {
                this.labelRect = label;
                this.lexRect = lex;
            }
        }

        private RenderNodePositionInfo RenderNode(GraphicsProxy g, Elem elem, Rectangle where, bool isSelected, bool generatePosInfo)
        {
            Node node = elem.GetNode();

            int labelheight = node.HasLabel() ? elem.Dimensions_label.Height : 0;
            int lexheight = node.HasLexical() ? elem.Dimensions_lex.Height : 0;
            int paddingvertical = PaddingVertical();

            int sumheight = labelheight + lexheight +
                ((node.GetDisplayType() == NodeDisplayType.Triangle) ? (paddingvertical * 2 + MarginVertical()) : 0);

            if (sumheight > 0)
            {
                Rectangle labelRect = new Rectangle(where.Left + (where.Width / 2) - (elem.Dimensions_label.Width / 2),
                        where.Top + where.Height / 2 - sumheight / 2,
                        elem.Dimensions_label.Width, elem.Dimensions_label.Height);
                Rectangle lexRect = new Rectangle(where.Left + where.Width / 2 - elem.Dimensions_lex.Width / 2,
                        where.Bottom - where.Height / 2 + sumheight / 2 - lexheight,
                        elem.Dimensions_lex.Width, elem.Dimensions_lex.Height);



                if (node.HasLabel())
                {
                    m_richtext.Reset();
                    m_richtext.ZoomFactor = GetZoomFactor();
                    m_richtext.Rtf = node.GetLabelRtf();

                    //m_richtext.Text = elem.altitude.ToString();//!!
                    m_richtext.FixupComponentLabel(isSelected ? HighlightBackColor() : StandardBackColor());

                    g.RenderRTF(m_richtext, labelRect.Left, labelRect.Top, labelRect.Width, labelRect.Height);
                }
                if (node.HasLexical())
                {
                    m_richtext.Reset();
                    m_richtext.ZoomFactor = GetZoomFactor();
                    m_richtext.Rtf = node.GetLexicalRtf();
                    m_richtext.FixupComponentLexical(isSelected ? HighlightBackColor() : StandardBackColor());
                    g.RenderRTF(m_richtext, lexRect.Left, lexRect.Top, lexRect.Width, lexRect.Height);
                }
                if (generatePosInfo)
                    return new RenderNodePositionInfo(labelRect, lexRect);
                else
                    return null;
            }
            return null;
        }

        private static Elem FindCommonAncestor(Elem a, Elem b)
        {
            if (a.level < b.level)
            {
                Elem c = b;
                b = a;
                a = c;
            }
            while (a.level > b.level)
                if (a.Parent != null)
                    a = a.Parent;
                else
                    throw new TreeException("Apparently looking for common ancestor of unrelated nodes");
            while (a != b)
            {
                if (a.Parent != null)
                    a = a.Parent;
                else
                    throw new TreeException("Funny looking tree A");
                if (b.Parent != null)
                    b = b.Parent;
                else
                    throw new TreeException("Funny looking tree B");
            }
            return a;
        }

        private Rectangle OptGetSubTreeRectangle(Elem e)
        {
            if (ShouldDoWalkerTree())
                return new Rectangle(m_elemRoot.Location.X + e.shift,
                    e.Location.Y, e.BranchDimensions.Width, e.BranchDimensions.Height);
            else
                return new Rectangle(e.Location.X + e.Dimensions.Width / 2 - e.BranchDimensions.Width / 2,
                e.Location.Y, e.BranchDimensions.Width, e.BranchDimensions.Height);

        }

        private void PaintElement(GraphicsProxy g, Elem e)
        {

            Node node = e.GetNode();
            Point placement = e.Location;
            bool isSelected = m_selected.Contains(e);

            if (e.Parent != null)
                g.DrawLine(StandardLinePen(), placement.X + e.Dimensions.Width / 2, placement.Y,
                    e.Parent.Location.X + e.Parent.Dimensions.Width / 2, e.Parent.Location.Y +
                    e.Parent.Dimensions.Height);

            if (node.IsBlank())
            {
                if (isSelected)
                    g.FillRectangle(HighlightBackBrush(), placement.X, placement.Y, e.Dimensions.Width - 1,
                        e.Dimensions.Height - 1);
                /*the minus 1 in the above line prevent a weird screen artefact when drawing
                 the selected variant of the red X when antialiasing is on. It looks like
                 the antialiasing actually draws pixels outside the box specified? One solution
                 would be to push antialias state, turn it off, draw the box, and pop antialias state,
                 but this is faster and adequate*/
                DrawBigX(g, placement.X + (e.Dimensions.Width / 2), placement.Y + (e.Dimensions.Height / 2));
            }
            else
            {
                Graphics realGraphics = g.Graphics;
                if (realGraphics == null)
                    RenderNode(g, e, e.Rect, isSelected, false);
                else
                {
                    if (e.cache == null)
                    {
                        Bitmap bm = new Bitmap(e.Dimensions.Width, e.Dimensions.Height);
                        using (Graphics bmGraphics = Graphics.FromImage(bm))
                        {
                            GraphicsPassThrough gp = new GraphicsPassThrough(bmGraphics);
                            SetupAntiAliasing(gp);
                            RenderNodePositionInfo rnpi =
                                RenderNode(gp, e, new Rectangle(Point.Empty, e.Dimensions), false, true);
                            e.labelRect = rnpi.labelRect;
                            e.lexRect = rnpi.lexRect;
                        }
                        e.cache = bm;
                        bm = new Bitmap(e.Dimensions.Width, e.Dimensions.Height);
                        using (Graphics bmGraphics = Graphics.FromImage(bm))
                        {
                            GraphicsPassThrough gp = new GraphicsPassThrough(bmGraphics);
                            SetupAntiAliasing(gp);
                            RenderNode(gp, e, new Rectangle(Point.Empty, e.Dimensions), true, false);
                        }
                        e.cacheSelected = bm;
                    }
                    realGraphics.DrawImageUnscaled(isSelected ? e.cacheSelected : e.cache, e.Location);

                    /*
                     * I would like to draw the image with transparency, but this
                     * produces problems since the text image includes gradients
                     * resulting from text antialiasing
                    System.Drawing.Imaging.ImageAttributes attrs=
                        new System.Drawing.Imaging.ImageAttributes();
                    attrs.SetColorKey(Color.White, Color.White);
                    realGraphics.DrawImage(isSelected ? e.cacheSelected : e.cache,
                        e.Rect, 0, 0, e.Rect.Width, e.Rect.Height, GraphicsUnit.Pixel,
                        attrs);
                     */
                }

                if (node.GetDisplayType() == NodeDisplayType.Triangle)
                {
                    int paddingvertical = PaddingVertical();
                    Rectangle lexRect = e.lexRect;
                    Rectangle labelRect = e.labelRect;
                    lexRect.Offset(e.Location);
                    labelRect.Offset(e.Location);
                    g.DrawPolygon(StandardLinePen(), new Point[] {
                    new Point(lexRect.Left,lexRect.Top-paddingvertical),
                    new Point(lexRect.Right,lexRect.Top-paddingvertical),
                    new Point(labelRect.Left+labelRect.Width/2,labelRect.Bottom+paddingvertical)
                });
                }
            }

            Decoration decoration = node.Decoration;
            if (decoration.mode != DecorationMode.None)
            {
                int pv = PaddingVertical() / 2 + (int)node.Decoration.penstyle.width;
                int ph = PaddingHorizontal() / 2 + (int)node.Decoration.penstyle.width;
                int pd = node.Decoration.padding;
                Rectangle decorationRect;
                if (decoration.mode == DecorationMode.Node)
                    decorationRect = new Rectangle(e.Rect.Left + ph, e.Rect.Top + pv, e.Rect.Width - ph * 2, e.Rect.Height - pv * 2);
                else
                { // subtree mode
                    decorationRect = OptGetSubTreeRectangle(e);
                    decorationRect.Inflate(pd, pd);
                }
                switch (decoration.shape)
                {
                    case DecorationShape.Ellipse:
                        using (Pen pen = decoration.penstyle.GetPen(GetZoomFactor()))
                            g.DrawEllipse(pen, decorationRect.Left, decorationRect.Top, decorationRect.Width, decorationRect.Height);
                        break;
                    case DecorationShape.Cross:
                        using (Pen pen = decoration.penstyle.GetPen(GetZoomFactor()))
                        {
                            g.DrawLine(pen, decorationRect.Left, decorationRect.Top, decorationRect.Left + decorationRect.Width, decorationRect.Height + decorationRect.Top);
                            g.DrawLine(pen, decorationRect.Left, decorationRect.Top + decorationRect.Height, decorationRect.Left + decorationRect.Width, decorationRect.Top);
                        }
                        break;
                    case DecorationShape.Rectangle:
                        using (Pen pen = decoration.penstyle.GetPen(GetZoomFactor()))
                            g.DrawRectangle(pen, decorationRect.Left, decorationRect.Top, decorationRect.Width, decorationRect.Height);
                        break;
                    default:
                        throw new Exception("unknown shape");
                }
            }


            /*in determining how to vertically space traces,
             we should perhaps use a fancier algorithm, so that their
             height is offset only if they would actually cross. An reasonable
             approximation of such a fancy algorithm might keep track of
             traceCount on a per-level basis, rather than a per-common-ancestor
             basis
             
             * This block, which draws the traces, shouldn't be here,
             * as traces shouldn't be members of elements
             */
            int traceCount = 0;
            foreach (ElemTrace elemtrace in e.Traces())
            {
                traceCount++;
                using (Pen pen = elemtrace.trace.penstyle.GetPen(GetZoomFactor()))
                {

                    Point[] points = elemtrace.GetPoints(e, TraceSpacingHorizontal(),
                        TraceSpacingVertical(), traceCount);
                    switch (elemtrace.trace.tracestyle)
                    {
                        case TraceStyle.Line:
                            g.DrawLines(pen, points);
                            break;
                        case TraceStyle.Curve:
                            g.DrawBezier(pen, points[0], points[1], points[points.Length - 2], points[points.Length - 1]);
                            break;
                        default:
                            throw new TreeException("Unknown trace style");
                    }
                }
            }

        }

        private void DrawBigX(GraphicsProxy g, int x, int y)
        {
            const int r = 5;
            g.DrawLine(BigXLinePen(), x - r, y - r, x + r, y + r);
            g.DrawLine(BigXLinePen(), x - r, y + r, x + r, y - r);
        }

        public void WriteSVG(System.IO.Stream os, Encoding encoding)
        {
            StdCalcTree(m_elemRoot.BranchDimensions);
            Point offset = new Point(-m_maximumExtents.Left, -m_maximumExtents.Top);
            Rectangle size = new Rectangle(Point.Empty, m_maximumExtents.Size);

            using (GraphicsSVGWriter svg = new GraphicsSVGWriter(os, encoding))
            {
                svg.TranslateTransform(offset.X, offset.Y);
                DoPaint(svg);
            }
            UpdateEverything(true);
        }

        public Bitmap GetBitmap()
        {
            StdCalcTree(m_elemRoot.BranchDimensions);
            Point offset = new Point(-m_maximumExtents.Left, -m_maximumExtents.Top);
            Rectangle size = new Rectangle(Point.Empty, m_maximumExtents.Size);

            if (size.Width > 6 * 1024 || size.Height > 6 * 1024)
                throw new TreeException("The current view is too large to export a bitmap image.");
            Bitmap result = new Bitmap(size.Width,
                size.Height); //not sure if this needs 3rd param
            using (Graphics g = Graphics.FromImage(result))
            {
                g.TranslateTransform(offset.X, offset.Y);
                DoPaint(new GraphicsPassThrough(g));
            }

            UpdateEverything(true);
            return result;
        }


        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // base.OnPaintBackground(e);

            // This is a NOP: we paint the background in OnPaint
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            //Console.WriteLine(e.ClipRectangle);
            using (Graphics g = e.Graphics)
            {
                GraphicsPassThrough gpt = new GraphicsPassThrough(g);
                StdCalcTree(ClientSize);

                Point offset = new Point(Math.Max(0, -m_maximumExtents.Left), Math.Max(0, -m_maximumExtents.Top));

                if (offset.X > 0 || offset.Y > 0)
                {
                    Rectangle size = new Rectangle(Point.Empty, m_maximumExtents.Size);
                    StdCalcTree(ClientSize, offset);
                }
                AutoScrollMinSize = m_maximumExtents.Size;

                Point pt = AutoScrollPosition;
                gpt.TranslateTransform(pt.X, pt.Y);

                DoPaint(gpt);
                /*
                 * Alternatively, draw to back buffer:
                 * using (Bitmap bm = GetBitmap())
                 *   g.DrawImageUnscaled(bm, 0, 0);
                 */
            }
        }

        private void DoPaint(GraphicsProxy g)
        {
            SetupAntiAliasing(g);

            g.Clear(StandardBackColor());

            foreach (Elem elem in Elements())
                PaintElement(g, elem);

        }
        private void StdCalcTree(Size targetSize)
        {
            StdCalcTree(targetSize, Point.Empty);
        }

        private void StdCalcTree(Size targetSize, Point additionalOffset)
        {
            Size totalSize = m_elemRoot.BranchDimensions;
            int someX = targetSize.Width / 2;
            int someY;
            switch (TreeAlignment())
            {
                case TreeAlignmentEnum.Top:
                    someY = 0;
                    break;
                case TreeAlignmentEnum.Bottom:
                    someY = targetSize.Height - totalSize.Height;
                    break;
                default:
                case TreeAlignmentEnum.Middle:
                    someY = targetSize.Height / 2 - totalSize.Height / 2;
                    break;
            }

            if (someX < totalSize.Width / 2)
                someX = totalSize.Width / 2;
            if (someY < 0)
                someY = 0;
            Point origin = new Point(someX + additionalOffset.X, someY + additionalOffset.Y);

            OptTreeDraw2(totalSize, origin);
        }

        private void OptTreeDraw2(Size totalSize, Point origin)
        {
            m_maximumExtents = new Rectangle(new Point(origin.X - m_elemRoot.BranchDimensions.Width / 2,
                origin.Y), m_elemRoot.BranchDimensions);
            if (ShouldDoWalkerTree())
                WlkTreeDraw(totalSize, origin);
            else
                StdCalcPlacements(totalSize, origin);

            foreach (Elem e in Elements())
                ExtendMaximumExtents(e);
        }

        void ExtendMaximumExtents(Elem elem)
        {
            Decoration dec = elem.GetNode().Decoration;
            if (dec.mode == DecorationMode.Subtree)
            {
                Rectangle decorationRect = OptGetSubTreeRectangle(elem);
                decorationRect.Inflate(dec.padding + (int)dec.penstyle.width,
                    dec.padding + (int)dec.penstyle.width);
                m_maximumExtents = Rectangle.Union(m_maximumExtents, decorationRect);
            }

            foreach (ElemTrace elemtrace in elem.Traces())
            {
                Rectangle r = elemtrace.GetRectangle(elem, TraceSpacingHorizontal(),
                    TraceSpacingVertical(), elem.NumTraces());
                r.Inflate((int)elemtrace.trace.penstyle.width, (int)elemtrace.trace.penstyle.width);
                m_maximumExtents = Rectangle.Union(m_maximumExtents,
                    r);

            }
        }

        private void StdCalcPlacements(Size totalSize, Point origin)
        {
            int vindent = 0;
            for (int i = 0; i < m_height; i++)
            {
                int x = 0;
                foreach (Elem el in m_elemByLevel[i])
                {
                    int cw = el.BranchDimensions.Width;
                    int parent_indent = (el.Parent != null) ? el.Parent.indent : -1;
                    if (x < parent_indent)
                        x = parent_indent;
                    el.indent = x;

                    if (RowsArranged())
                        el.vindent = vindent;
                    else
                        el.vindent = (el.Parent != null ? el.Parent.vindent + el.Parent.Dimensions.Height + MarginVertical() : 0);

                    el.Location = StdCalcPlacement(el, origin, totalSize);

                    x += cw + MarginHorizontal();
                }
                vindent += m_levelDimensions[i].Height + MarginVertical();
            }
        }

        private Point StdCalcPlacement(Elem e, Point relativeTo, Size totalSize)
        {
            Point placement = new Point(
                relativeTo.X - totalSize.Width / 2 +
                e.BranchDimensions.Width / 2 + e.indent
                - e.Dimensions.Width / 2,
                e.vindent + relativeTo.Y);
            return placement;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SyntaxTreeViewer
            // 
            this.AutoScroll = true;
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SyntaxTreeViewer_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SyntaxTreeViewer_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SyntaxTreeViewer_MouseUp);
            this.ResumeLayout(false);

        }

        private Elem GetElemFromMouse(MouseEventArgs e)
        {
            /*This is grossly inefficient. The correct solution is a 2-dimensional R-Tree on
             * the coordinates of the rectangles forming the nodes
             * which can be implemented, more or less, as a sorted list of sorted lists
             * 
             * http://www.reddit.com/r/programming/comments/80dlc/i_have_a_set_of_rectangles_and_need_to_determine/
             * http://en.wikipedia.org/wiki/R-tree
            */
            Point p = new Point(e.Location.X - AutoScrollPosition.X,
                e.Location.Y - AutoScrollPosition.Y);

            foreach (Elem el in Elements())
                if (el.Rect.Contains(p))
                    return el;
            return null;
        }

        private void SetBlankNode(Node node)
        {
            //newnode.SetLabelRtfAndText("{\\rtf1\r\n \r\n}\r\n", " ");
            //node.SetLabelRtfAndText("{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\r\n\\viewkind4\\uc1\\pard\\f0\\fs16 \r\n}\r\n", " ");
            node.SetLabelRtfAndText("{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\r\n\\viewkind4\\uc1\\pard\\f0\\fs16\r\n}\r\n", "");
        }

        private void SyntaxTreeViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_mousemode == MouseMode.Down)
            {
                if (e.Button != MouseButtons.Left)
                {
                    m_mousemode = MouseMode.None;
                    Cursor.Current = Cursors.Default;
                    return;
                }
                const int r = 5;
                if (Math.Abs(e.Location.X - m_mousedownat.X) > r || Math.Abs(e.Location.Y - m_mousedownat.Y) > r)
                {
                    if (m_selected.Count == 1)
                    {
                        m_mousemode = MouseMode.Dragging;
                    }
                    if (m_selected.Count == 0)
                    {
                        m_mousemode = MouseMode.Selecting;

                        m_selectionrect = new Rectangle(PointToScreen(m_mousedownat), Size.Empty);

                    }
                }
            }
            if (m_mousemode == MouseMode.Selecting)
            {
                Cursor.Current = Cursors.Cross;
                ControlPaint.DrawReversibleFrame(m_selectionrect, Color.Black, FrameStyle.Dashed);
                if (e.Button != MouseButtons.Left)
                {
                    m_mousemode = MouseMode.None;
                    Cursor.Current = Cursors.Default;
                    return;
                }

                Point p = e.Location;
                if (p.X < ClientRectangle.Left)
                    p.X = ClientRectangle.Left;
                if (p.X > ClientRectangle.Right)
                    p.X = ClientRectangle.Right;
                if (p.Y < ClientRectangle.Top)
                    p.Y = ClientRectangle.Top;
                if (p.Y > ClientRectangle.Bottom)
                    p.Y = ClientRectangle.Bottom;

                m_selectionrect.Width = p.X - m_mousedownat.X;
                m_selectionrect.Height = p.Y - m_mousedownat.Y;
                DoSelectByRect(m_selectionrect);

                ControlPaint.DrawReversibleFrame(m_selectionrect, Color.Black, FrameStyle.Dashed);
            }
            if (m_mousemode == MouseMode.RequestForElem)
            {
                if (!ClientRectangle.Contains(e.Location))
                {
                    Cursor.Current = Cursors.No;
                }
                else
                {
                    Elem elem = GetElemFromMouse(e);
                    if (elem == null)
                        Cursor.Current = Cursors.No;
                    else
                    {

                        bool validTarget = m_requestforelem.Query(elem);
                        if (validTarget)
                            Cursor.Current = Cursors.Hand;
                        else
                            Cursor.Current = Cursors.No;
                    }
                }
            }
            if (m_mousemode == MouseMode.Dragging)
            {
                if (!ClientRectangle.Contains(e.Location))
                {
                    Cursor.Current = Cursors.No;
                }
                else
                {
                    Elem elem = GetElemFromMouse(e);
                    if (elem == null || m_selected.Count != 1)
                    {
                        Cursor.Current = Cursors.No;
                    }
                    else
                    {
                        bool validMode = DoDraggedTo(elem, m_selected[0], false);
                        if (validMode)
                            Cursor.Current = Cursors.Hand;
                        else
                            Cursor.Current = Cursors.No;
                    }
                }
            }
        }

        private const string CLIPBOARD_FORMAT = "syntax_tree_data";

        public bool DoCutBranch(bool forReal)
        {
            bool done = false;
            if (m_selected.Count == 1)
            {
                if (m_selected[0].Parent != null)
                {
                    if (forReal)
                    {
                        DataFormats.Format format = DataFormats.GetFormat(CLIPBOARD_FORMAT);
                        Node branch = m_selected[0].GetNode();
                        int i = m_selected[0].Parent.GetNode().FindChild(branch);
                        m_selected[0].Parent.GetNode().Detach(i);
                        branch.SetBranchTree(null);

                        DataObject ido = new DataObject(format.Name, branch);
                        Clipboard.Clear();
                        Clipboard.SetDataObject(ido, true);
                    }
                    done = true;
                }
            }
            return done;
        }

        public bool DoCopyBranch(bool forReal)
        {
            bool done = false;
            if (m_selected.Count == 1)
            {
                if (forReal)
                {
                    DataFormats.Format format = DataFormats.GetFormat(CLIPBOARD_FORMAT);
                    Node branch = m_selected[0].GetNode().CloneBranch(null);
                    DataObject ido = new DataObject(format.Name, branch);
                    Clipboard.Clear();
                    Clipboard.SetDataObject(ido, true);
                }
                done = true;
            }
            return done;
        }

        public bool DoPasteBranch(bool forReal)
        {
            bool done = false;
            if (m_selected.Count == 1)
            {
                Elem elem = m_selected[0];
                if (elem.GetNode().CanAddChildHere())
                {
                    if (Clipboard.ContainsData(CLIPBOARD_FORMAT))
                    {
                        IDataObject ido = Clipboard.GetDataObject();
                        if (ido != null)
                        {
                            DataFormats.Format format = DataFormats.GetFormat(CLIPBOARD_FORMAT);
                            if (ido.GetDataPresent(format.Name))
                            {
                                object o = ido.GetData(format.Name);
                                if (o != null)
                                {
                                    if (o is Node)
                                    {
                                        if (forReal)
                                        {
                                            Node newbranch = (Node)o;
                                            Node target = elem.GetNode();
                                            newbranch.SetBranchTree(target.GetTree());
                                            target.Attach(newbranch);
                                        }
                                        done = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return done;
        }

        private bool DoDraggedTo(Elem to, Elem from, bool forReal)
        {
            bool changed = false;
            if (to == from)
            {
                //can't do that
            }
            else if (to.Parent == from.Parent)
            {
                if (forReal)
                {
                    Node parent = to.Parent.GetNode();
                    int ito = parent.FindChild(to.GetNode());
                    int ifrom = parent.FindChild(from.GetNode());
                    parent.SwapChildren(ito, ifrom);
                }

                changed = true;
            }
            else if (to == from.Parent || from == to.Parent)
            {
                if (from == to.Parent)
                {
                    Elem temp = to;
                    to = from;
                    from = temp;
                }
                if (from.GetNode().CanAddChildHere())
                {
                    if (forReal)
                    {
                        int where = to.GetNode().FindChild(from.GetNode());
                        to.GetNode().SwapWithChild(where);
                    }
                    changed = true;
                }
            }
            else if (from.GetNode().IsAncestorOf(to.GetNode()))
            {
                if (from.GetNode().NumChildren() == 1)
                {
                    if (from.Parent == null)
                    {
                        if (forReal)
                        {
                            Node newroot = from.GetNode().FindChild(0);
                            from.GetNode().Detach(0);
                            to.GetNode().Attach(from.GetNode());
                            m_syntax.Root = newroot;
                        }
                        changed = true;
                    }
                    else
                    {
                        if (forReal)
                        {
                            int ff = from.Parent.GetNode().FindChild(from.GetNode());
                            from.Parent.GetNode().DetachAndReplaceWithChild(ff);
                            to.GetNode().Attach(from.GetNode());
                        }
                        changed = true;
                    }
                }
            }
            else
            {
                Node nto = to.GetNode();
                Node nfrom = from.GetNode();
                if (nto.CanAddChildHere() && from.Parent.GetNode() != null)
                {
                    if (forReal)
                    {
                        int ifrom = from.Parent.GetNode().FindChild(nfrom);
                        from.Parent.GetNode().Detach(ifrom);

                        nto.Attach(nfrom);
                    }
                    changed = true;
                }
            }
            if (changed && forReal)
                UpdateEverything(false);
            return changed;
        }

        private void DoSelectByRect(Rectangle rect)
        {
            /*This is also pretty horribly inefficient. Unlike GetElementFromMouse,
             though, there isn't a simple way to improve this. Fortunately it probably
             doesn't matter too much.*/
            List<Elem> newselected = new List<Elem>();
            foreach (Elem e in Elements())
            {
                Rectangle rect2 = AdjustForScroll(e.Rect);
                rect2.Location = PointToScreen(rect2.Location);
                if (rect.Width < 0)
                    rect = new Rectangle(rect.Left + rect.Width, rect.Top, -rect.Width, rect.Height);
                if (rect.Height < 0)
                    rect = new Rectangle(rect.Left, rect.Top + rect.Height, rect.Width, -rect.Height);
                if (rect2.IntersectsWith(rect))
                {
                    if (!m_selected.Contains(e))
                    {
                        Invalidate(AdjustForScroll(e.Rect));
                    }

                    newselected.Add(e);
                }
            }
            List<Elem> oldselected = m_selected;
            m_selected = newselected;
            foreach (Elem e in oldselected)
            {
                if (!m_selected.Contains(e))
                {
                    Invalidate(AdjustForScroll(e.Rect));
                }
            }
            Update();
        }

        Rectangle AdjustForScroll(Rectangle rect)
        {
            rect.Offset(AutoScrollPosition);
            return rect;
        }

        private void SyntaxTreeViewer_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            if (m_mousemode == MouseMode.Dragging)
            {
                if (ClientRectangle.Contains(e.Location))
                {
                    Elem elem = GetElemFromMouse(e);
                    if (elem != null && m_selected.Count == 1)
                    {
                        UndoAdd();
                        DoDraggedTo(elem, m_selected[0], true);
                    }
                }
                Cursor.Current = Cursors.Default;
            }
            if (m_mousemode == MouseMode.Selecting)
            {
                ControlPaint.DrawReversibleFrame(m_selectionrect, Color.Black, FrameStyle.Dashed);
                //Invalidate();
                Cursor.Current = Cursors.Default;
                FireSelectionChange();
            }
            m_mousemode = MouseMode.None;
        }

        private void SyntaxTreeViewer_MouseDown(object sender, MouseEventArgs e)
        {
            if (m_mousemode == MouseMode.RequestForElem)
            {
                Elem elem = GetElemFromMouse(e);
                if (elem != null && e.Button == MouseButtons.Left)
                {
                    m_requestforelem.Invoke(elem);
                }
                Cursor.Current = Cursors.Default;
            }
            if (m_mousemode == MouseMode.None)
            {
                if (e.Button == MouseButtons.Left)
                {
                    m_mousedownat = e.Location;

                    m_mousemode = MouseMode.Down;
                }

                Elem elem = GetElemFromMouse(e);

                if (e.Button == MouseButtons.Left || (e.Button == MouseButtons.Right && m_selected.Count <= 1))
                {
                    if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                    {
                        if (elem != null)
                        {
                            if (m_selected.Contains(elem))
                                m_selected.Remove(elem);
                            else
                                m_selected.Add(elem);
                            Invalidate(AdjustForScroll(elem.Rect));

                            FireSelectionChange();
                        }
                    }
                    else
                    {
                        if (m_selected.Count == 1 && m_selected[0] == elem)
                        {
                            // already there !  nop
                        }
                        else
                        {
                            bool didit = false;
                            List<Elem> oldselected = m_selected;
                            m_selected = new List<Elem>();
                            if (elem != null)
                                m_selected.Add(elem);
                            foreach (Elem wasselected in oldselected)
                            {
                                Invalidate(AdjustForScroll(wasselected.Rect));
                                didit = true;
                            }
                            if (elem != null)
                            {
                                Invalidate(AdjustForScroll(elem.Rect));
                                didit = true;
                            }
                            if (didit)
                                FireSelectionChange();
                        }
                    }
                }
                if (e.Button == MouseButtons.Right && elem != null)
                {
                    basicMenuStrip.Show(this, e.Location);
                }
            }
            ReleaseRequestForElem();

        }
        void basicMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (m_selected.Count < 1)
                e.Cancel = true;
            else
            {
                Elem elem = m_selected[0];
                basicMenuStripAddParent.Enabled = DoAddParent(false);
                basicMenuStripDeleteItem.Enabled = DoDeleteNode(false);
                basicMenuStripAddChild.Enabled = DoAddChild(false);

                basicMenuStripCut.Enabled = DoCutBranch(false);
                basicMenuStripCopy.Enabled = DoCopyBranch(false);
                basicMenuStripPaste.Enabled = DoPasteBranch(false);

                basicMenuStripAddTrace.Enabled = DoCreateTrace(false);
                basicMenuStripDecorate.Enabled = DoDecoration(false);
            }
        }
        public void basicMenuStripCut_Click(object sender, EventArgs e)
        {
            UndoAdd();
            DoCutBranch(true);
            UpdateEverything(false);
        }
        void basicMenuStripDecorate_Click(object sender, EventArgs e)
        {
            DoDecoration(true);
        }

        public void basicMenuStripCopy_Click(object sender, EventArgs e)
        {
            DoCopyBranch(true);
        }
        public bool DoDecoration(bool forReal)
        {
            //for some reason we don't use SelectionProxy on this one
            bool done = false;
            SyntaxTreeViewer syntaxTreeViewer = this;
            if (syntaxTreeViewer.GetSelection().Count > 0)
            {
                if (forReal)
                {
                    Decoration decoration = syntaxTreeViewer.GetSelection()[0].GetNode().Decoration;

                    using (LineDialog ld = new LineDialog())
                    {
                        ld.Decoration = decoration;
                        ld.ShapeMode = LineDialog.LineDialogMode.Decoration;

                        if (ld.ShowDialog(this) == DialogResult.OK)
                        {
                            syntaxTreeViewer.UndoAdd();
                            decoration = ld.Decoration;
                            foreach (Elem elem in syntaxTreeViewer.GetSelection())
                            {
                                elem.GetNode().Decoration = decoration;
                                elem.FreeCache(); /*this in only necessary because
                                                    changing the padding setting will change
                                                   the dimensions of the element*/
                            }
                            syntaxTreeViewer.UpdateEverything(true);
                        }
                    }
                }
                done = true;
            }
            return done;
        }

        public bool DoCreateTrace(Elem source, Elem dest, bool forReal)
        {
            bool done = false;
            if (source != dest)
            {
                if (!FindCommonAncestor(source, dest).HasTrace(source, dest))
                {
                    if (forReal)
                    {
                        UndoAdd();
                        source.GetNode().AddTraceFromHere(dest.GetNode());
                        UpdateEverything(false); //use a less comprehensive update here
                    }
                    done = true;
                }
            }
            return done;
        }

        public void RequestForElem(RequestForElem e)
        {
            if (m_requestforelem != null)
                ReleaseRequestForElem();

            m_mousemode = MouseMode.RequestForElem;
            m_requestforelem = e;

            if (m_hintbox == null)
                m_hintbox = new HintBox(this);
            m_hintbox.Label = m_requestforelem.Text;
            m_hintbox.Show(this);
        }
        public void ReleaseRequestForElem()
        {
            if (m_hintbox != null)
            {
                m_hintbox.Hide();
            }
            if (m_mousemode == MouseMode.RequestForElem)
                m_mousemode = MouseMode.None;
            m_requestforelem = null;
        }



        public class RequestForElemAddTrace : RequestForElem
        {
            private SyntaxTreeViewer stv;
            public RequestForElemAddTrace(SyntaxTreeViewer stv)
            {
                this.stv = stv;
            }
            public String Text { get { return "Click on a node to select the destination of the trace."; } }
            public void Invoke(Elem elem)
            {
                stv.DoCreateTrace(stv.m_selected[0], elem, true);
            }
            public bool Query(Elem elem)
            {
                return stv.DoCreateTrace(stv.m_selected[0], elem, false);
            }
        }
        public bool DoCreateTrace(bool forReal)
        {
            bool done = false;
            if (m_selected.Count == 1)
            {
                if (forReal)
                {
                    RequestForElem(new RequestForElemAddTrace(this));
                }
                done = true;
            }
            return done;
        }
        public void basicMenuStripAddTrace_Click(object sender, EventArgs e)
        {
            DoCreateTrace(true);
        }
        public void basicMenuStripPaste_Click(object sender, EventArgs e)
        {
            UndoAdd();
            DoPasteBranch(true);
            UpdateEverything(false);
        }

        public bool DoDeleteNode(bool forReal)
        {
            if (m_selected.Count != 1)
                return false;
            Elem elem = m_selected[0];
            bool canDelete = elem.GetNode().CanBeDeleted();


            if (forReal && canDelete)
            {
                UndoAdd();
                if ((elem.Parent == null && elem.GetNode().NumChildren() == 1) || elem.GetNode().NumChildren() <= 1)
                    elem.GetNode().Delete();

                UpdateEverything(false);
                return true;
            }
            return canDelete;
        }

        public bool DoAddParent(bool forReal)
        {
            bool done = false;
            if (m_selected.Count == 1)
            {
                if (forReal)
                {
                    UndoAdd();
                    Elem elem = m_selected[0];
                    Node oldnode = elem.GetNode();
                    Node newnode = oldnode.InsertParent();
                    SetBlankNode(newnode);

                    UpdateTreeData();
                    FireStructureChanged();

                    SelectByNode(oldnode);
                    Invalidate();
                }
                done = true;
            }
            return done;
        }

        public bool DoAddChild(bool forReal)
        {
            return DoAddChild(forReal,false);
        }

        public bool DoAddChild(bool forReal,bool selectNewNode)
        {
            bool done = false;
            if (m_selected.Count == 1)
            {
                Elem elem = m_selected[0];
                Node oldnode = elem.GetNode();
                if (oldnode.CanAddChildHere())
                {
                    if (forReal)
                    {
                        UndoAdd();
                        Node newnode = oldnode.AddChild();
                        SetBlankNode(newnode);

                        UpdateTreeData();
                        FireStructureChanged();
                        if (selectNewNode)
                            SelectByNode(newnode);
                        else
                            SelectByNode(oldnode);

                        Invalidate();
                    }
                    done = true;
                }
            }
            return done;
        }

        public void basicMenuStripDeleteItem_Click(object sender, EventArgs e)
        {
            DoDeleteNode(true);
        }

        public void basicMenuStripAddParent_Click(object sender, EventArgs e)
        {
            DoAddParent(true);
        }

        public void basicMenuStripAddChild_Click(object sender, EventArgs e)
        {
            DoAddChild(true);
        }

        public static TreeOptions GetDefaultOptions()
        {
            return new TreeOptions();
        }

        public void SelectAll()
        {
            m_selected.Clear();
            foreach (Elem elem in Elements())
            {
                m_selected.Add(elem);
            }
            FireSelectionChange();
        }


    }

    public class UndoManager
    {
        private List<byte[]> storage;
        private int pos;
        private const int MAXLEN = 10;
        private SyntaxTreeViewer stv;
        public UndoManager(SyntaxTreeViewer stv)
        {
            storage = new List<byte[]>();
            this.stv = stv;
            pos = -1;
        }
        public void Clear()
        {
            storage.Clear();
            pos = -1;
        }
        public void Add()
        {
            if (pos > 0)
            {
                storage.RemoveRange(0, pos);
                pos = 0;
            }
            byte[] bytes = Hacks.Serialize(stv.GetCurrentTree());
            storage.Insert(0, bytes);
            pos = -1;
            while (storage.Count > MAXLEN)
                storage.RemoveAt(storage.Count - 1);
        }
        private void DeserializeAt(int posx)
        {
            SyntaxTree st = Hacks.Deserialize(storage[posx]) as SyntaxTree;
            if (st != null)
                stv.SetCurrentTree(st, false);
            else
                throw new Exception("Corruption during undo deserialize");
        }
        public bool DoRedo(bool forReal)
        {
            bool done = false;
            if (pos > 0)
            {
                if (forReal)
                {
                    pos--;
                    DeserializeAt(pos);
                }
                done = true;
            }
            return done;
        }
        public bool DoUndo(bool forReal)
        {
            bool done = false;
            if (pos < storage.Count - 1)
            {
                if (forReal)
                {
                    if (pos == -1)
                    {
                        Add();
                        pos = 0;
                    }
                    pos++;
                    DeserializeAt(pos);
                }
                done = true;
            }
            return done;
        }
    }

    public class ElemTrace
    {
        public Trace trace;
        public Elem source;
        public Elem destination;
        public Elem ancestor;
        public int sourcexth;
        public int destinationxth;
        public bool longextent;
        public ElemTrace(Elem source, Elem destination, Trace trace)
        {
            this.source = source;
            this.destination = destination;
            this.trace = trace;
            sourcexth = destinationxth = 0;
            longextent = false; ancestor = null;
        }

        public Rectangle GetRectangle(Elem e, int traceHSpacing, int traceVSpacing, int traceCount)
        {
            Point[] points = GetPoints(e, traceHSpacing, traceVSpacing, traceCount);
            Rectangle ret = new Rectangle(points[0], Size.Empty);
            for (int i = 1; i < points.Length; i++)
                ret = Rectangle.Union(ret, new Rectangle(points[i], Size.Empty));
            return ret;
        }
        public Point[] GetPoints(Elem e, int traceHSpacing, int traceVSpacing, int traceCount)
        {
            ElemTrace elemtrace = this;
            int soffset = (-(elemtrace.source.tracecount - 1) / 2 + elemtrace.sourcexth) * traceHSpacing;
            int doffset = (-(elemtrace.destination.tracecount - 1) / 2 + elemtrace.destinationxth) * traceHSpacing;
            const int paddingv = 0;
            int depth;
            bool drop = true;
            bool reverse = false;
            
            if (longextent)
                depth = e.Location.Y + e.BranchDimensions.Height;
            else
                depth = Math.Max(elemtrace.destination.Rect.Bottom, elemtrace.source.Rect.Bottom);

            //wot a mess
            Point start;
            int whereto = elemtrace.destination.Rect.Left + elemtrace.destination.Rect.Width / 2 + doffset;
            if (elemtrace.source.Location.Y + elemtrace.source.Rect.Height / 2 > elemtrace.destination.Rect.Bottom
                && elemtrace.source.NumChildren() > 0 && elemtrace.sourcexth == 0
                && (whereto < elemtrace.source.Rect.Left || whereto > elemtrace.source.Rect.Right))
            {
                if (whereto < elemtrace.source.Rect.Left)
                    start = new Point(elemtrace.source.Rect.Left - paddingv,
                        elemtrace.source.Rect.Top + elemtrace.source.Rect.Height / 2);
                else
                    start = new Point(elemtrace.source.Rect.Right + paddingv,
                    elemtrace.source.Rect.Top + elemtrace.source.Rect.Height / 2);
                drop = false;
            }
            else
                start = new Point(elemtrace.source.Rect.Left + elemtrace.source.Rect.Width / 2 + soffset,
                                 elemtrace.source.Rect.Bottom + paddingv);


            Point end;
            whereto = elemtrace.source.Rect.Left + elemtrace.source.Rect.Width / 2 + soffset;
            if (elemtrace.destination.Location.Y + elemtrace.destination.Rect.Height / 2 > elemtrace.source.Rect.Bottom
                && elemtrace.destination.NumChildren() > 0 && elemtrace.destinationxth == 0
                && (whereto < elemtrace.destination.Rect.Left || whereto > elemtrace.destination.Rect.Right))
            {
                if (whereto < elemtrace.destination.Rect.Left)
                    end = new Point(elemtrace.destination.Rect.Left - paddingv,
                        elemtrace.destination.Rect.Top + elemtrace.destination.Rect.Height / 2);
                else
                    end = new Point(elemtrace.destination.Rect.Right + paddingv,
                    elemtrace.destination.Rect.Top + elemtrace.destination.Rect.Height / 2);
                drop = false;
                reverse = true;
            }
            else
                end = new Point(elemtrace.destination.Rect.Left + elemtrace.destination.Rect.Width / 2 + doffset,
                            elemtrace.destination.Rect.Bottom + paddingv);

            Point[] points;
            if (drop)
                points = new Point[] {
                            start,
                            new Point(start.X,
                            depth+traceCount*traceVSpacing),
                            new Point(end.X,
                            depth+traceCount*traceVSpacing),
                            end
                        };
            else
                points = new Point[]
            {
                start,
                reverse?new Point(start.X,end.Y):new Point(end.X,start.Y),
                end
            };
            return points;
        }
    }

    public class Elem : IDisposable
    {
        /*bitmap cache*/
        public Bitmap cache;
        public Bitmap cacheSelected;

        /*Walker-renderer related variables*/
        public int mod;
        public Elem thread;
        public Elem ancestor;
        public int prelim;
        public int change;
        public int shift;

        /*phpsyntaxtree-style-renderer related variables*/
        public int indent;
        public int vindent;

        public Rectangle lexRect;
        public Rectangle labelRect;

        public int level;
        public int altitude;
        private int number;

        private Point m_location;
        private Size m_dimensions;
        private Size m_branchdimensions;
        private Node m_node;
        private Elem m_parent;
        private List<Elem> m_children;
        private Size m_dimensions_label;
        private Size m_dimensions_lex;

        public int tracecount = 0;
        private List<ElemTrace> m_traces;

        public IEnumerable<Elem> Children()
        {
            foreach (Elem c in m_children)
            {
                yield return c;
            }
        }

        public void FreeCacheOnSubtree()
        {
            FreeCache();
            foreach (Elem elem in Children())
                elem.FreeCacheOnSubtree();
        }

        public int NumTraces()
        {
            return m_traces.Count;
        }

        public void FreeCache()
        {
            if (cache != null)
            {
                cache.Dispose();
                cache = null;
            }
            if (cacheSelected != null)
            {
                cacheSelected.Dispose();
                cacheSelected = null;
            }
        }

        public void Dispose()
        {
            FreeCache();
            foreach (Elem elem in Children())
                elem.Dispose();
            thread = ancestor = m_parent = null;
            m_children = null;
            m_traces = null;
        }


        public IEnumerable<Elem> LeftSiblings()
        {
            int me = GetNumber();
            foreach (Elem e in Parent.Children())
                if (e.GetNumber() < me)
                    yield return e;
        }

        public IEnumerable<Elem> RightSiblings()
        {
            int me = GetNumber();
            foreach (Elem e in Parent.Children())
                if (e.GetNumber() > me)
                    yield return e;
        }

        public int GetNumber()
        {
            if (this.number < 0)
                this.number = Parent.GetNode().FindChild(GetNode());
            return this.number;
        }

        public int NumChildren()
        {
            return m_node.NumChildren();
        }

        public Node GetNode()
        {
            return m_node;
        }

        public Elem Parent
        {
            get
            {
                return m_parent;
            }
        }

        public Rectangle Rect
        {
            get
            {
                return new Rectangle(Location, Dimensions);
            }
        }

        public Point Location
        {
            get
            {
                return m_location;
            }
            set
            {
                m_location = value;
            }
        }

        public Size Dimensions
        {
            get
            {
                return m_dimensions;
            }
            set
            {
                m_dimensions = value;
            }
        }

        public Size BranchDimensions
        {
            get
            {
                return m_branchdimensions;
            }
            set
            {
                m_branchdimensions = value;
            }
        }

        public Size Dimensions_lex
        {
            get
            {
                return m_dimensions_lex;
            }
            set
            {
                m_dimensions_lex = value;
            }
        }
        public Size Dimensions_label
        {
            get
            {
                return m_dimensions_label;
            }
            set
            {
                m_dimensions_label = value;
            }
        }

        public Elem(Node n)
        {
            m_node = n;
            m_parent = null;
            m_children = new List<Elem>();
            m_traces = new List<ElemTrace>();
            this.tracecount = 0;
            this.number = -1;
        }
        public void ClearTraceData()
        {
            this.tracecount = 0;
            m_traces.Clear();
        }
        public IEnumerable<ElemTrace> Traces()
        {
            foreach (ElemTrace e in m_traces)
                yield return e;
        }
        public bool HasTrace(Elem a, Elem b)
        {
            foreach (ElemTrace et in Traces())
                if ((et.source == a && et.destination == b) || (et.source == b && et.destination == a))
                    return true;
            return false;
        }
        public void Add(ElemTrace trace)
        {
            m_traces.Add(trace);
        }
        public void Add(Elem elem)
        {
            m_children.Add(elem);
            elem.m_parent = this;
        }

    }

    public class FormatChanges
    {
        public Color color;
        public Ternary bold;
        public Ternary italic;
        public Ternary underline;
        public Ternary subscript;
        public Ternary superscript;
        public Ternary strikeout;
        public String fontfamily;
        public float fontsize;
        public Font font;

        public FormatChanges()
        {
            font = null;
            color = Color.Empty;
            fontsize = 0f;
            fontfamily = String.Empty;
            bold = italic = strikeout = underline = subscript = superscript = Ternary.Indeterminate;

        }
    }

    public class SelectionProxy
    {
        private SyntaxTreeViewer m_syntax;
        private NodeEditor m_nodeeditor;
        private List<Elem> m_selected;

        private bool IsLexicalOperation()
        {
            return m_nodeeditor.lastTouchedWasLex;
        }
        private RichTextBox2 GetActiveEditor()
        {
            return m_nodeeditor.GetEditBox(IsLexicalOperation());
        }

        public SelectionProxy(SyntaxTreeViewer syntax, NodeEditor nodeeditor, List<Elem> selected)
        {
            m_syntax = syntax;
            m_nodeeditor = nodeeditor;
            m_selected = selected;
        }

        public FormatChanges GetFormatElement(Elem elem, bool lexical)
        {
            Node node = elem.GetNode();
            Dummy richtext = Dummy.GetInstance();
            richtext.Reset();
            richtext.Rtf = lexical ? node.GetLexicalRtf() : node.GetLabelRtf();
            richtext.SelectAll();
            return richtext.GetFormatChanges();
        }
        public void ReformatElement(Elem elem, bool lexical, FormatChanges fc)
        {
            Node node = elem.GetNode();
            Dummy richtext = Dummy.GetInstance();
            richtext.Reset();
            richtext.Rtf = lexical ? node.GetLexicalRtf() : node.GetLabelRtf();

            richtext.SelectAll();

            richtext.SetFormatChanges(fc);

            if (lexical)
                node.SetLexicalRtfAndText(richtext.Rtf, richtext.Text);
            else
                node.SetLabelRtfAndText(richtext.Rtf, richtext.Text);
            elem.FreeCache();
        }

        private void ReformatAll(FormatChanges fc)
        {
            foreach (Elem elem in m_selected)
                ReformatElement(elem, IsLexicalOperation(), fc);
        }
        private FormatChanges GetFormatOne()
        {
            RichTextBox2 rtb = GetActiveEditor();
            return rtb.GetFormatChanges();
        }
        private void ReformatOne(FormatChanges fc)
        {
            RichTextBox2 rtb = GetActiveEditor();
            rtb.SetFormatChanges(fc);

        }
        public FormatChanges SelectionSummary()
        {
            if (m_selected.Count == 1)
                return GetFormatOne();
            else
            {
                FormatChanges fc = GetFormatElement(m_selected[0], IsLexicalOperation());
                foreach (Elem elem in m_selected)
                {
                    FormatChanges nfc = GetFormatElement(elem, IsLexicalOperation());
                    if (nfc.bold != fc.bold)
                        fc.bold = Ternary.Indeterminate;
                    if (nfc.italic != fc.italic)
                        fc.italic = Ternary.Indeterminate;
                    if (nfc.underline != fc.underline)
                        fc.underline = Ternary.Indeterminate;
                    if (nfc.superscript != fc.superscript)
                        fc.superscript = Ternary.Indeterminate;
                    if (nfc.strikeout != fc.strikeout)
                        fc.strikeout = Ternary.Indeterminate;
                    if (nfc.subscript != fc.subscript)
                        fc.subscript = Ternary.Indeterminate;
                    if (nfc.color == Color.Empty || !nfc.color.Equals(fc.color))
                        fc.color = Color.Empty;
                    if (nfc.fontsize != fc.fontsize)
                        fc.fontsize = 0f;
                    if (!nfc.fontfamily.Equals(fc.fontfamily))
                        fc.fontfamily = String.Empty;
                    if (nfc.font != null && fc.font != null && !nfc.font.Equals(fc.font))
                        fc.font = null;
                }
                return fc;
            }
        }
        private void SelectionUpdate(FormatChanges fc)
        {
            if (m_selected.Count == 1)
                ReformatOne(fc);
            else
                ReformatAll(fc);
        }
        public Font Font
        {
            set
            {
                FormatChanges fc = new FormatChanges();
                fc.font = value;
                SelectionUpdate(fc);
            }
            get
            {
                return SelectionSummary().font;
            }
        }
        public Ternary Bold
        {
            set
            {
                FormatChanges fc = new FormatChanges();
                fc.bold = value;
                SelectionUpdate(fc);
            }
            get
            {
                return SelectionSummary().bold;
            }
        }
        public Ternary Strikeout
        {
            set
            {
                FormatChanges fc = new FormatChanges();
                fc.strikeout = value;
                SelectionUpdate(fc);
            }
            get
            {
                return SelectionSummary().strikeout;
            }
        }
        public Ternary Superscript
        {
            set
            {
                FormatChanges fc = new FormatChanges();
                fc.superscript = value;
                SelectionUpdate(fc);
            }
            get
            {
                return SelectionSummary().superscript;
            }
        }
        public Ternary Subscript
        {
            set
            {
                FormatChanges fc = new FormatChanges();
                fc.subscript = value;
                SelectionUpdate(fc);
            }
            get
            {
                return SelectionSummary().subscript;
            }
        }

        public Ternary Underline
        {
            set
            {
                FormatChanges fc = new FormatChanges();
                fc.underline = value;
                SelectionUpdate(fc);
            }
            get
            {
                return SelectionSummary().underline;
            }
        }

        public Ternary Italic
        {
            set
            {
                FormatChanges fc = new FormatChanges();
                fc.italic = value;
                SelectionUpdate(fc);
            }
            get
            {
                return SelectionSummary().italic;
            }
        }
        public String FontName
        {
            get
            {
                return SelectionSummary().fontfamily;
            }
            set
            {
                FormatChanges fc = new FormatChanges();
                fc.fontfamily = value;
                SelectionUpdate(fc);
            }
        }
        public float FontSize
        {
            get
            {
                return SelectionSummary().fontsize;
            }
            set
            {
                FormatChanges fc = new FormatChanges();
                fc.fontsize = value;
                SelectionUpdate(fc);
            }
        }
        public Color Color
        {
            get
            {
                return SelectionSummary().color;
            }
            set
            {
                FormatChanges fc = new FormatChanges();
                fc.color = value;
                SelectionUpdate(fc);
            }
        }
    }

    public class TreeItemSelectedEventArgs : EventArgs
    {
        private List<Elem> m_selected;
        private SyntaxTreeViewer m_stv;
        private bool refocus;
        public TreeItemSelectedEventArgs(List<Elem> selected,
            SyntaxTreeViewer stv, bool refocus)
            : base()
        {
            this.m_selected = selected;
            this.m_stv = stv;
            this.refocus = refocus;
        }
        public SyntaxTreeViewer SyntaxTreeViewer
        {
            get
            {
                return m_stv;
            }
        }
        public bool Refocus
        {
            get
            {
                return refocus;
            }
        }
        public List<Elem> Selected
        {
            get
            {
                return m_selected;
            }
        }
    }
    public interface RequestForElem
    {
        string Text { get; }
        void Invoke(Elem elem);
        bool Query(Elem elem);
    }

    public enum MouseMode
    {
        None,
        Down,
        Dragging,
        Selecting,
        RequestForElem,
    }
    public class Dummy : RichTextBox2
    {
        private static Dummy instance;
        public static Dummy GetInstance()
        {
            if (instance.IsDisposed)
                instance = null;
            if (instance == null)
                throw new TreeException("Dummy not initialied yet");
            return instance;
        }
        public static string TextAndColorToRtf(string text, FontAndColor fc)
        {
            Dummy dummy = GetInstance();
            dummy.Reset();
            dummy.Text = text;
            dummy.SelectAll();
            dummy.SelectionAlignment = HorizontalAlignment.Left; //hack
            dummy.SelectionFont = fc.Font;
            dummy.SelectionColor = fc.Color;

            return dummy.Rtf;
        }
        public static void ReleaseDummy()
        {
            if (instance != null)
            {
                instance.Dispose();
                instance = null;
            }
        }
        public static Dummy Init(SyntaxTreeViewer stv)
        {
            if (instance != null)
                return instance;
            instance = new Dummy();

            stv.Controls.Add(instance);
            instance.EnableCleverness();

            return instance;
        }
        private Dummy()
        {
            Visible = false;
            Size = Size.Empty;
            Margin = Padding.Empty;
            BorderStyle = BorderStyle.None;
            ScrollBars = RichTextBoxScrollBars.None;
            DetectUrls = false;
            WordWrap = false;


        }

        public void FixupComponentLabel()
        {
            FixupComponentLabel(Color.White);
        }

        public void FixupComponentLexical()
        {
            FixupComponentLexical(Color.White);
        }

        public void FixupComponentLabel(Color bgcolor)
        {
            SelectAll();
            SelectionAlignment = HorizontalAlignment.Center;
            BackColor = bgcolor;
            //m_richtext.ForeColor = LabelTextColor();
            //m_richtext.BackColor = StandardBackColor();
        }

        public void FixupComponentLexical(Color bgcolor)
        {
            SelectAll();
            SelectionAlignment = HorizontalAlignment.Center;
            BackColor = bgcolor;
            //m_richtext.ForeColor = LexicalTextColor();
            //m_richtext.BackColor = StandardBackColor();
        }

    }
}

