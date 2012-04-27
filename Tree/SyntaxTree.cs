using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Tree
{
    public class TreeException : ApplicationException
    {

        public TreeException(string exception)
            : base(exception)
        {
        }
    }

    [Serializable]
    public enum ArrowStyle
    {
        None,
        TinyArrow,
        LittleArrow,
        BigArrow,
        LineArrow,
        HollowArrow,
        PointyArrow
    }

    [Serializable]
    public enum TraceStyle
    {
        Line,
        Curve
    }

    [Serializable]
    public enum DecorationMode
    {
        None,
        Node,
        Subtree
    }

    [Serializable]
    public enum DecorationShape
    {
        Rectangle,
        Ellipse,
        Cross
    }

    [Serializable]
    public struct Decoration
    {
        private static Decoration m_Empty = new Decoration(DecorationMode.None, DecorationShape.Rectangle, new PenStyle(Color.Black, -1f, DashStyle.Solid, ArrowStyle.None), 0);
        public static Decoration Empty
        {
            get { return m_Empty; }
        }
        public DecorationMode mode;
        public DecorationShape shape;
        public PenStyle penstyle;
        public int padding;
        public Decoration(DecorationMode mode, DecorationShape shape)
        {
            this.mode = mode;
            this.shape = shape;
            this.penstyle = new PenStyle(Color.Black, 1.0f, DashStyle.Solid, ArrowStyle.None);
            this.padding = 4;
        }
        public Decoration(DecorationMode mode, DecorationShape shape, PenStyle penstyle, int padding)
        {
            this.mode = mode;
            this.shape = shape;
            this.penstyle = penstyle;
            this.padding = padding;
        }
        public override bool Equals(object obj)
        {
            if (obj is Decoration)
                return this == (Decoration)obj;
            return false;
        }
        public static bool operator !=(Decoration left, Decoration right)
        {
            return !(left == right);
        }
        public static bool operator ==(Decoration left, Decoration right)
        {
            return left.shape == right.shape && left.penstyle == right.penstyle &&
                left.padding == right.padding && left.mode == right.mode;
        }
        public override int GetHashCode()
        {
            return shape.GetHashCode() ^ penstyle.GetHashCode() ^ mode.GetHashCode();
        }
    }

    [Serializable]
    public struct PenStyle
    {
        public Color color;
        public float width;
        public DashStyle dashstyle;
        public ArrowStyle arrowstyle;
        //public LineJoin linejoin;
        //public LineCap endcap;
        //public LineCap startcap;
        //public DashCap dashcap;
        public PenStyle(Color color, float width, DashStyle dashstyle, ArrowStyle arrowstyle)
        {
            this.color = color;
            this.width = width;
            this.dashstyle = dashstyle;
            this.arrowstyle = arrowstyle;
        }
        public Pen GetPen()
        {
            return GetPen(1.0f);
        }
        public Pen GetPen(float zoom)
        {
            Pen pen = new Pen(color, width * zoom);
            pen.DashStyle = dashstyle;
            if (arrowstyle == ArrowStyle.None)
            {
                //!! what should the default line ending be? Round?
                pen.EndCap = LineCap.Flat;
            }
            else
            {
                pen.CustomEndCap = ArrowStyleToLineCap(arrowstyle);
            }
            return pen;
        }
        private static CustomLineCap ArrowStyleToLineCap(ArrowStyle arrowstyle)
        {
            return Hacks.GetLineCap(arrowstyle);
        }
        public static bool operator !=(PenStyle left, PenStyle right)
        {
            return !(left == right);
        }
        public static bool operator ==(PenStyle left, PenStyle right)
        {
            return left.color == right.color && left.width == right.width && left.dashstyle == right.dashstyle;
        }
        public override bool Equals(object obj)
        {
            if (obj is PenStyle)
                return this == (PenStyle)obj;
            return false;
        }
        public override int GetHashCode()
        {
            return color.GetHashCode() ^ width.GetHashCode() ^ dashstyle.GetHashCode();
        }

    }

    [Serializable]
    public class Trace
    {
        public PenStyle penstyle;
        public TraceStyle tracestyle;
        public Node source;
        public Node destination;
        public Trace(Node source, Node destination, PenStyle penstyle, TraceStyle tracestyle)
        {
            this.penstyle = penstyle;
            this.source = source;
            this.destination = destination;
            this.tracestyle = tracestyle;
        }

        public override string ToString()
        {
            return GetName();
        }

        public String GetName()
        {
            return source.GetDisplayableNodeName() + " -> " + destination.GetDisplayableNodeName();
        }
    }

    [Serializable]
    public enum ConnectorOrientation
    {
        Agnostic,
        Left,
        Center,
        Right,
    }

    [Serializable]
    public enum NodeDisplayType
    {
        Normal,
        Triangle
    }

    [Serializable]
    public class Connector
    {
        private ConnectorOrientation m_orientation;
        private Node m_child;
        public Connector(Node child)
        {
            this.m_child = child;
            this.m_orientation = ConnectorOrientation.Agnostic;
        }
        public Connector CloneBranch(Node node)
        {
            Connector c = new Connector(m_child.CloneBranch(node));
            c.m_orientation = m_orientation;
            return c;
        }
        public Connector(Node child, ConnectorOrientation orientation)
        {
            this.m_child = child;
            this.m_orientation = orientation;
        }
        public void SetChild(Node n)
        {
            m_child = n;
        }
        public Node Child
        {
            get
            {
                return m_child;
            }
        }
    }

    [Serializable]
    public struct RichText
    {
        private string m_text;
        private string m_rtf;

        public RichText(string text, string rtf)
        {
            m_text = text;
            m_rtf = rtf;
        }
        public RichText(string text)
        {
            m_text = text;
            m_rtf = "";
        }
        public string Rtf
        {
            get
            {
                return m_rtf;
            }
            set
            {
                m_rtf = value;
            }
        }
        public string Text
        {
            get
            {
                return m_text;
            }
            set
            {
                m_text = value;
            }
        }
    }

    [Serializable]
    public class Node
    {
        private List<Connector> m_children;
        private List<Trace> m_traces;
        private Node m_parent;
        private SyntaxTree m_tree;
        private RichText m_label;
        private RichText m_lexical;
        private Decoration m_decoration;
        private NodeDisplayType m_displaytype;

        public const int MAX_CHILDREN = 3;

        public Decoration Decoration
        {
            get { return m_decoration; }
            set { m_tree.Dirty = true; m_decoration = value; }
        }

        public Node GetParent()
        {
            return m_parent;
        }
        public Node CloneBranch(Node parent)
        {
            Node node = new Node();
            node.m_parent = parent;
            node.m_tree = null;
            node.m_label = m_label; // value semantics
            node.m_lexical = m_lexical;
            node.m_displaytype = m_displaytype;
            node.m_children = new List<Connector>();
            node.m_traces = new List<Trace>(); /*do not copy traces*/
            node.m_decoration = m_decoration;
            foreach (Connector child in m_children)
                node.m_children.Add(child.CloneBranch(node));
            return node;
        }
        private void Initialize(SyntaxTree tree)
        {
            m_children = new List<Connector>();
            m_traces = new List<Trace>();
            m_decoration = new Decoration(DecorationMode.None, DecorationShape.Rectangle);
            m_displaytype = NodeDisplayType.Normal;
            /*m_label = new RichText("",Dummy.TextAndColorToRtf("",m_tree.m_options.labelfont));
            m_lexical = new RichText("", Dummy.TextAndColorToRtf("", m_tree.m_options.lexicalfont));*/
            m_label = new RichText("", "{\\rtf }");
            m_lexical = new RichText("", "{\\rtf }");

        }
        private Node()
        {
        }
        public Node(SyntaxTree tree)
        {
            this.m_tree = tree;
            this.m_parent = null;

            Initialize(tree);
        }
        public Node(Node parent)
        {
            this.m_parent = parent;
            this.m_tree = parent.m_tree;
            Initialize(this.m_tree);
        }
        public SyntaxTree GetTree()
        {
            return m_tree;
        }
        public int NumTraces()
        {
            return m_traces.Count;
        }
        public IEnumerable<Trace> Traces()
        {
            foreach (Trace trace in m_traces)
                yield return trace;
        }
        public void AddTraceFromHere(Node to)
        {
            if (to == this)
                throw new TreeException("Cannot trace to oneself");
            foreach (Trace tracex in Traces())
            {
                if (tracex.destination == to || tracex.source == to)
                    throw new TreeException("Node may be connected by at most one trace");
            }
            Trace trace = new Trace(this, to, m_tree.m_options.tracestyle,
                m_tree.m_options.tracemode);
            m_tree.Dirty = true;
            m_traces.Add(trace);
            to.m_traces.Add(trace);
        }
        public void RemoveAllTracesRecurse()
        {
            RemoveAllTraces();
            foreach (Node n in Children())
                n.RemoveAllTraces();
        }
        public void RemoveAllTraces()
        {
            Trace[] traces = new Trace[m_traces.Count];
            for (int i = 0; i < m_traces.Count; i++)
                traces[i] = m_traces[i];

            foreach (Trace trace in traces)
                RemoveTrace(trace);
        }
        public void RemoveTrace(Trace t)
        {
            if (!t.source.m_traces.Contains(t))
                throw new TreeException("Disproportionate tracing");
            if (!t.destination.m_traces.Contains(t))
                throw new TreeException("Disproportionate tracing");
            m_tree.Dirty = true;
            t.source.m_traces.Remove(t);
            t.destination.m_traces.Remove(t);
        }
        public void SetBranchTree(SyntaxTree tree)
        {
            m_tree = tree;
            foreach (Connector connector in m_children)
                connector.Child.SetBranchTree(tree);
        }
        public Node FindChild(int n)
        {
            if (n < m_children.Count)
            {
                return m_children[n].Child;
            }
            else throw new TreeException("Not a real child, like Pinocchio");
        }
        public int FindChild(Node n)
        {
            for (int i = 0; i < m_children.Count; i++)
            {
                if (m_children[i].Child == n)
                {
                    return i;
                }
            }
            throw new TreeException("Internal tree grokking error");
        }
        public void Detach(int i)
        {
            if (i < m_children.Count)
            {
                m_children[i].Child.RemoveAllTracesRecurse();
                m_children[i].Child.m_parent = null;
                m_children.RemoveAt(i);
                m_tree.Dirty = true;
            }
            else throw new TreeException(string.Format("Invalid tree child index: {0}", i));
        }
        public void DetachAndReplaceWithChild(int i)
        {
            if (i < m_children.Count)
            {
                Node middle = m_children[i].Child;
                if (middle.NumChildren() != 1)
                    throw new TreeException(string.Format("Too many children on detach and replace"));
                Node child = middle.m_children[0].Child;
                middle.Detach(0);
                middle.m_parent = null;
                m_children[i].SetChild(child);
                child.m_parent = this;
                m_tree.Dirty = true;
            }
            else throw new TreeException(string.Format("Invalid tree child index: {0}", i));
        }
        public void Attach(Node n)
        {
            if (NumChildren() < MAX_CHILDREN)
            {
                n.m_parent = this;
                m_children.Add(new Connector(n));
                m_tree.Dirty = true;
            }
            else
                throw new TreeException("Too many children!");
        }
        public void SwapChildren(int a, int b)
        {
            if (a < m_children.Count && b < m_children.Count)
            {
                Node temp;

                temp = m_children[a].Child;
                m_children[a].SetChild(m_children[b].Child);
                m_children[b].SetChild(temp);

                m_tree.Dirty = true;
            }
            else
                throw new TreeException(string.Format("Invalid tree child index: {0}, {1}", a, b));
        }
        public Node InsertParent()
        {
            Node n = new Node(m_tree);
            if (m_parent == null)
            {
                this.m_parent = n;
                this.m_tree.Root = n;
                n.m_children.Add(new Connector(this));
            }
            else
            {
                int i = this.m_parent.FindChild(this);
                n.m_parent = this.m_parent;
                m_parent.m_children[i] = new Connector(n);
                this.m_parent = n;
                n.m_children.Add(new Connector(this));
            }
            m_tree.Dirty = true;
            return n;
        }
        public void Delete()
        {
            if (m_children.Count == 0 && m_parent == null)
                throw new TreeException("Can't delete last node!");
            RemoveAllTraces();
            if (m_parent == null)
            {
                if (m_children.Count > 1)
                    throw new TreeException("There are too many children of this node for it to be deleted");
                m_children[0].Child.m_parent = null;
                this.m_tree.Root = m_children[0].Child;
                m_tree.Dirty = true;
            }
            else
            {
                if (m_children.Count == 1)
                {
                    int i = m_parent.FindChild(this);
                    this.m_children[0].Child.m_parent = this.m_parent;
                    m_parent.m_children[i] = this.m_children[0];
                    m_tree.Dirty = true;
                    return;
                }
                else
                {
                    if (m_children.Count == 0)
                    {
                        int i = m_parent.FindChild(this);
                        m_parent.m_children.RemoveAt(i);
                        m_tree.Dirty = true;
                        return;
                    }
                    else
                    {
                        throw new TreeException("Can't delete node that has more children");
                    }
                }
            }
        }
        public int NumChildren()
        {
            return m_children.Count;
        }
        public bool IsAncestorOf(Node n)
        {
            while (n != null)
            {
                if (n.m_parent == this)
                    return true;
                n = n.m_parent;
            }
            return false;
        }
        public bool CanAddChildHere()
        {
            return CanAddMoreChildren() && !HasLexical();
        }
        public bool CanBeDeleted()
        {
            return NumChildren() <= 1 &&
                    (m_parent != null || NumChildren() > 0);
        }
        public bool CanAddMoreChildren()
        {
            return m_children.Count < MAX_CHILDREN;
        }
        public bool HasChildren()
        {
            return m_children.Count != 0;
        }
        public void SwapWithChild(int child)
        {
            if (child >= m_children.Count)
                throw new TreeException("Not a real child");
            Connector connector = m_children[child];
            if (this.m_parent != null)
                this.m_parent.m_children[this.m_parent.FindChild(this)].SetChild(connector.Child);
            else
                m_tree.Root = connector.Child;
            connector.Child.m_parent = this.m_parent;
            this.m_parent = connector.Child;

            List<Connector> temp = this.m_children;
            this.m_children = connector.Child.m_children;
            connector.Child.m_children = temp;

            foreach (Node c in this.Children())
                c.m_parent = this;
            foreach (Node c in connector.Child.Children())
                if (c != connector.Child)
                    c.m_parent = connector.Child;

            connector.Child.m_children[connector.Child.FindChild(connector.Child)].SetChild(this);
        }
        public IEnumerable<Connector> Connectors()
        {
            foreach (Connector c in m_children)
            {
                yield return c;
            }
        }
        public IEnumerable<Node> Children()
        {
            foreach (Connector c in m_children)
            {
                yield return c.Child;
            }
        }
        /*
        public Dictionary<int,List<Node>> SubElementsByHeight()
        {
            Dictionary<int,List<Node>> result = new Dictionary<int,List<Node>>();
            this.SubElementsByHeightWorker(result, 1);
            return result;
        }

        private void SubElementsByHeightWorker(Dictionary<int,List<Node>> result,int myheight)
        {
            if (result.ContainsKey
            List<Node> list = result.TryGetValue(myheight, null);
            if (list == null)
            {
                list = new List<Node>();
                result[myheight] = list;
            }
            list.Add(this);
            foreach (Node n in Children())
                n.SubElementsByHeightWorker(result, myheight + 1);
        }
        
        public int Height()
        {
            int max = 0;
            foreach (Connector n in m_children)
            {
                int thisheight = n.Child.Height();
                if (thisheight > max)
                    max = thisheight;
            }
            m_height = max + 1;
            return m_height;
        }
         */
        public bool IsBlank()
        {
            return !(m_label.Text != "" || m_lexical.Text != "");
        }
        public bool HasLabel()
        {
            return m_label.Text != "";
        }
        public bool HasLexical()
        {
            return (m_lexical.Text != "") || (m_displaytype == NodeDisplayType.Triangle);
        }
        public string GetLabelText()
        {
            return m_label.Text;
        }
        public string GetLabelRtf()
        {
            return m_label.Rtf;
        }
        public string GetLexicalRtf()
        {
            return m_lexical.Rtf;
        }
        public string GetLexicalText()
        {
            return m_lexical.Text;
        }
        public void SetLabelRtfAndText(string rtf, string text)
        {
            m_label.Text = text;
            m_label.Rtf = rtf;
            m_tree.Dirty = true;
        }
        public void SetLexicalRtfAndText(string rtf, string text)
        {
            m_lexical.Text = text;
            m_lexical.Rtf = rtf;
            m_tree.Dirty = true;
        }
        public string GetDisplayableNodeName()
        {
            if (this.HasLabel())
            {
                string s = GetLabelText();
                int idx = s.IndexOf('\n');
                if (idx > 0)
                    return s.Substring(0, idx);
                else
                    return s;
            }
            else
                return "?";
        }
        public string GetDisplayableNodeNameForException()
        {
            if (this.HasLabel())
                return string.Format("node '{0}'", GetLabelText());
            else
                return "this node";
        }
        public Node AddChild()
        {
            if (HasLexical())
                throw new TreeException(string.Format("Can't add a child node " +
                           "to {0}, because it has a lexical item '{1}'; remove" +
                           " the lexical item first.", GetDisplayableNodeNameForException(), GetLexicalText()));
            if (m_children.Count >= MAX_CHILDREN)
                throw new TreeException(string.Format("Can't add a child node to " +
                    "{0} because it already has the maximum number of children, {1}.",
                    GetDisplayableNodeNameForException(), MAX_CHILDREN));
            Node newnode = new Node(this);
            m_children.Add(new Connector(newnode));
            m_tree.Dirty = true;
            return newnode;
        }
        public void SetDisplayType(NodeDisplayType style)
        {
            m_displaytype = style;
        }
        public NodeDisplayType GetDisplayType()
        {
            return m_displaytype;
        }

    }

    [Serializable]
    public class SyntaxTree
    {
        private Node m_root;
        private bool m_changed;
        public TreeOptions m_options;
        public SyntaxTree()
        {
            m_options = new TreeOptions();
            m_root = new Node(this);
            m_changed = true;
        }
        public bool Dirty
        {
            get
            {
                return m_changed;
            }
            set
            {
                m_changed = value;
            }
        }
        public static SyntaxTree FromBracketing(String text)
        {
            SyntaxTree st = new SyntaxTree();
            Node current = null;
            StringBuilder sb = null;
            int mode = 0;
            char openBracket;
            char closeBracket;
            if (text.Length > 0 && text[0] == '(')
            {
                openBracket = '(';
                closeBracket = ')';
            }
            else
            {
                openBracket = '[';
                closeBracket = ']';
            }
            for (int i = 0; i < text.Length; i++)
            {
                switch (mode)
                {
                    case 0:
                        if (text[i] == openBracket)
                        {
                            mode = 1;
                            if (current == null)
                            {
                                current = new Node(st);
                                st.Root = current;
                            }
                            else
                            {
                                Node newnode = current.AddChild();
                                current = newnode;
                            }
                            sb = new StringBuilder();
                        }
                        else
                            throw new TreeException("Bracketing syntax error: expected open bracket");
                        break;
                    case 1:
                        if (text[i] == ' ' || text[i] == openBracket || text[i] == closeBracket)
                        {
                            if (sb != null)
                            {
                                SetNodeLabelSimple(current, sb.ToString());
                                sb = null;
                            }
                            if (text[i] == ' ')
                            {
                                mode = 2;
                                sb = new StringBuilder();
                            }
                            if (text[i] == closeBracket)
                            {
                                current = current.GetParent();
                                if (current == null)
                                    mode = 4;
                            }
                            if (text[i] == openBracket)
                            {
                                mode = 0;
                                i--;
                            }
                        }
                        else
                            if (sb != null)
                                sb.Append(text[i]);
                        break;
                    case 2:
                        if (text[i] == closeBracket || text[i] == openBracket)
                        {
                            if (sb != null)
                            {
                                String n = sb.ToString().Trim();
                                if (n != String.Empty)
                                    SetNodeLexicalSimple(current, n);
                                sb = null;
                            }
                            if (text[i] == closeBracket)
                            {
                                current = current.GetParent();
                                if (current == null)
                                    mode = 4;
                            }
                            if (text[i] == openBracket)
                            {
                                mode = 0;
                                i--;
                            }
                        }
                        else
                            if (sb != null)
                                sb.Append(text[i]);
                        break;
                    case 4:
                        throw new TreeException("Bracketing syntax error: Unexpected garbage at end");
                }
            }
            if (current != null)
                throw new TreeException("Bracketing sytnax error: premature termination of string");
            return st;
        }
        private static void SetNodeLabelSimple(Node node, String text)
        {
            node.SetLabelRtfAndText(Dummy.TextAndColorToRtf(text, node.GetTree().m_options.labelfont), text);
        }
        private static void SetNodeLexicalSimple(Node node, String text)
        {
            node.SetLexicalRtfAndText(Dummy.TextAndColorToRtf(text, node.GetTree().m_options.lexicalfont), text);
        }
        public static SyntaxTree GetDefaultTree()
        {
            SyntaxTree ret = new SyntaxTree();//!!
            //ret.Root.SetLabelRtfAndText(Dummy.TextAndColorToRtf("S",ret.m_options.labelfont),"S");
            return ret;
        }
        public string ToBracketing()
        {
            StringBuilder sb = new StringBuilder();
            ToBracketing(sb, m_root);
            return sb.ToString();
        }
        private void ToBracketing(StringBuilder sb, Node n)
        {
            sb.Append("[");
            sb.Append(n.GetDisplayableNodeName());
            sb.Append(" ");
            if (n.HasLexical())
            {
                sb.Append(n.GetLexicalText());
                sb.Append(" ");
            }
            foreach (Node child in n.Children())
            {
                ToBracketing(sb, child);
            }
            sb.Append("]");
        }
        public Node Root
        {
            set
            {
                m_root = value;
            }
            get
            {
                return m_root;
            }
        }
    }
    [Serializable]
    public class TreeOptions
    {
        public RenderMode rendermode;
        public TreeAlignmentEnum treealignment;
        public int marginhorizontal;
        public int marginvertical;
        public int paddinghorizontal;
        public int paddingvertical;
        public int tracehorizontal;
        public int tracevertical;
        public Color backgroundcolor;
        public FontAndColor lexicalfont;
        public FontAndColor labelfont;
        public Color highlightcolor;
        public bool antialias;
        public PenStyle linestyle;
        public PenStyle tracestyle;
        public TraceStyle tracemode;
        private Font AttemptFont(string name, float size)
        {
            try
            {
                Font f = new Font(name, size);
                if (f.FontFamily.Name != name)
                {
                    f.Dispose();
                    return null;
                }
                return f;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static string[] PreferredFontNames =
            {
                "Doulos SIL",
                "Palatino Linotype",
                "Times New Roman",
                "Microsoft Sans Serif"
            };
        private Font DefaultFont()
        {
            foreach (string fontname in PreferredFontNames)
            {
                Font f = AttemptFont(fontname, 12);
                if (f != null)
                    return f;
            }
            return SystemFonts.DefaultFont;
        }
        public TreeOptions()
        {
            rendermode = RenderMode.Walker;
            treealignment = TreeAlignmentEnum.Middle;
            marginhorizontal = 20;
            marginvertical = 20;
            paddinghorizontal = 0;
            paddingvertical = 5;
            tracehorizontal = 3;
            tracevertical = 10;
            backgroundcolor = Color.White;
            labelfont = new FontAndColor(DefaultFont(), Color.Black);
            lexicalfont = new FontAndColor(DefaultFont(), Color.Red);
            highlightcolor = Color.Blue;
            antialias = true;
            linestyle = new PenStyle(Color.Black, 1.0f, DashStyle.Solid, ArrowStyle.None);
            tracestyle = new PenStyle(Color.Black, 1.0f, DashStyle.Dot, ArrowStyle.LittleArrow);
            tracemode = TraceStyle.Line;
        }
    }

    [Serializable]
    public enum RenderMode
    {
        Original,
        OriginalAlign,
        Walker
    }

    [Serializable]
    public enum TreeAlignmentEnum
    {
        Top,
        Middle,
        Bottom,
    }
    [Serializable]
    public struct FontAndColor
    {
        public Font Font;
        public Color Color;
        public FontAndColor(Font font, Color color)
        {
            Font = font;
            Color = color;
        }
        private static FontAndColor m_Empty = new FontAndColor(null, Color.Empty);
        public static FontAndColor Empty
        {
            get { return m_Empty; }
        }
        public override int GetHashCode()
        {
            return Font.GetHashCode() ^ Color.GetHashCode();
        }
        public static bool operator !=(FontAndColor left, FontAndColor right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj is FontAndColor)
                return this == (FontAndColor)obj;
            else return false;
        }
        public static bool operator ==(FontAndColor left, FontAndColor right)
        {
            return left.Color == right.Color && ((left.Font != null && right.Font != null && left.Font.Equals(right.Font)) || (left.Font == null && right.Font == null));
        }
    }

}
