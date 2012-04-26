using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace Tree
{
    public partial class LineDialog : Form
    {
        private static SolidBrush[] colors;
        private static DashStyle[] dashstyles;
        private static int[] widths;
        private static DecorationShape[] shapes;
        private static ArrowStyle[] arrows;

        static LineDialog()
        {
            colors = Hacks.GetColorBrushes();

            Array aarrows = Enum.GetValues(typeof(ArrowStyle));
            arrows = aarrows as ArrowStyle[];
            if (arrows == null)
                throw new Exception("Unusual arrow styles");

            Array ashapes = Enum.GetValues(typeof(DecorationShape));
            shapes = ashapes as DecorationShape[];
            if (shapes == null)
                throw new Exception("Got something unexpected from DecorationShape");

            dashstyles = new DashStyle[] {DashStyle.Solid,DashStyle.Dot,DashStyle.Dash,
                DashStyle.DashDot,DashStyle.DashDotDot};

            widths = new int[10];
            for (int i = 0; i < widths.Length; i++)
                widths[i] = i + 1;
        }

        private void UpdateGUI()
        {
            bool enable = false;
            if (!noneRadioButton.Checked
                || decorationGroupBox.Visible == false)
                enable = true;
            widthComboBox.Enabled = shapeComboBox.Enabled = styleComboBox.Enabled = colorComboBox.Enabled
                = paddinglNumeric.Enabled = enable;
        }

        public LineDialog()
        {
            InitializeComponent();

            colorComboBox.DataSource = colors;
            styleComboBox.DataSource = dashstyles;
            widthComboBox.DataSource = widths;
            shapeComboBox.DataSource = shapes;
            arrowComboBox.DataSource = arrows;
        }

        public enum LineDialogMode
        {
            Line,
            Decoration,
            Trace
        }

        public TraceStyle TraceStyle
        {
            get
            {
                if (traceCurveRadioButton.Checked)
                    return TraceStyle.Curve;
                if (traceLineRadioButton.Checked)
                    return TraceStyle.Line;
                throw new TreeException("Unknown trace style");
            }
            set
            {
                switch (value)
                {
                    case TraceStyle.Line:
                        traceLineRadioButton.Checked = true;
                        break;
                    case TraceStyle.Curve:
                        traceCurveRadioButton.Checked = true;
                        break;
                    default:
                        throw new TreeException("Unknown trace style");
                }
            }
        }

        public LineDialogMode ShapeMode
        {
            set
            {
                switch (value)
                {
                    case LineDialogMode.Decoration:
                        this.Text = "Decoration";
                        decorationGroupBox.Visible = true;
                        traceGroupBox.Visible = false;
                        break;
                    case LineDialogMode.Line:
                        this.Text = "Line";
                        decorationGroupBox.Visible = false;
                        traceGroupBox.Visible = false;
                        break;
                    case LineDialogMode.Trace:
                        this.Text = "Trace";
                        decorationGroupBox.Visible = false;
                        traceGroupBox.Visible = true;
                        break;
                }
            }
        }

        public DecorationMode DecorationMode
        {
            get
            {
                if (noneRadioButton.Checked)
                    return DecorationMode.None;
                else
                    if (nodeRadioButton.Checked)
                        return DecorationMode.Node;
                    else
                        if (subtreeRadioButton.Checked)
                            return DecorationMode.Subtree;
                        else
                            throw new Exception("Unchecked checkbox");
            }
            set
            {
                switch (value)
                {
                    case DecorationMode.None:
                        noneRadioButton.Checked = true;
                        break;
                    case DecorationMode.Node:
                        nodeRadioButton.Checked = true;
                        break;
                    case DecorationMode.Subtree:
                        subtreeRadioButton.Checked = true;
                        break;
                    default:
                        throw new Exception("Double unchecked checkbox");
                }
            }
        }

        public Decoration Decoration
        {
            set
            {
                this.DecorationMode = value.mode;
                this.Style = value.penstyle;
                this.DecorationPadding = value.padding;
                for (int i = 0; i < shapes.Length; i++)
                    if (shapes[i] == value.shape)
                    {
                        shapeComboBox.SelectedIndex = i;
                        return;
                    }
                shapeComboBox.SelectedIndex = 0;
            }
            get
            {
                return new Decoration(this.DecorationMode, (DecorationShape)shapeComboBox.SelectedItem, this.Style,
                    this.DecorationPadding);
            }
        }

        public int DecorationPadding
        {
            get
            {
                return (int)paddinglNumeric.Value;
            }
            set
            {
                paddinglNumeric.Value = value;
            }
        }

        public PenStyle Style
        {
            get
            {
                return new PenStyle(this.LineColor,
                    this.LineWidth, this.DashStyle, this.ArrowStyle);
            }
            set
            {
                this.LineColor = value.color;
                this.DashStyle = value.dashstyle;
                this.LineWidth = value.width;
                this.ArrowStyle = value.arrowstyle;
            }
        }

        public Color LineColor
        {
            get
            {
                return colors[colorComboBox.SelectedIndex].Color;
            }
            set
            {
                for (int i = 0; i < colors.Length; i++)
                    if (colors[i].Color == (value))
                    {
                        colorComboBox.SelectedIndex = i;
                        return;
                    }
                colorComboBox.SelectedIndex = 0;
            }
        }

        public ArrowStyle ArrowStyle
        {
            get
            {
                return arrows[arrowComboBox.SelectedIndex];
            }
            set
            {
                for (int i = 0; i < arrows.Length; i++)
                    if (arrows[i] == value)
                    {
                        arrowComboBox.SelectedIndex = i;
                        return;
                    }
                throw new Exception("Unkown arrow style");
            }
        }

        public DashStyle DashStyle
        {
            get
            {
                return dashstyles[styleComboBox.SelectedIndex];
            }
            set
            {
                for (int i = 0; i < dashstyles.Length; i++)
                    if (dashstyles[i] == value)
                    {
                        styleComboBox.SelectedIndex = i;
                        return;
                    }
                throw new Exception("Unkown style");
            }
        }

        public float LineWidth
        {
            get
            {
                try
                {
                    float a = Convert.ToSingle(widthComboBox.Text);
                    if (ValidWidth(a))
                        return a;
                    else
                        return 1f;
                }
                catch (Exception)
                {
                    return 1f;
                }
            }
            set
            {
                if (ValidWidth(value))
                {
                    widthComboBox.SelectedIndex = -1;
                    widthComboBox.Text = Convert.ToString(value);
                }
                else
                    widthComboBox.SelectedIndex = 0;
            }
        }

        private static bool ValidWidth(float f)
        {
            return f >= 1f && f <= 30f;
        }

        private Color DrawItemStateToColor(DrawItemState state)
        {
            if ((state & DrawItemState.Disabled) != 0)
                return SystemColors.GrayText;
            if ((state & DrawItemState.Selected) != 0)
                return SystemColors.HighlightText;
            return SystemColors.WindowText;
        }

        private void colorComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            SolidBrush brush = colors[e.Index];
            using (Graphics g = e.Graphics)
            {
                Rectangle colorBound = new Rectangle(e.Bounds.Left, e.Bounds.Top, e.Bounds.Height, e.Bounds.Height);
                g.FillRectangle(brush, colorBound);
                using (Brush tbrush = new SolidBrush(DrawItemStateToColor(e.State)))
                {
                    g.DrawString(brush.Color.Name, SystemFonts.MenuFont, tbrush,
                        new Rectangle(e.Bounds.Left + e.Bounds.Height, e.Bounds.Top, e.Bounds.Width - e.Bounds.Height, e.Bounds.Height));
                }
            }
        }

        private static Pen GetArrowStyleSample(Color color, ArrowStyle ars)
        {
            Pen pen = new Pen(color, 2);
            if (ars == ArrowStyle.None)
                pen.EndCap = LineCap.Flat;
            else
            {
                pen.CustomEndCap = Hacks.GetLineCap(ars);
            }
            return pen;
        }

        private static Pen GetDashStyleSample(Color color, DashStyle ds)
        {
            Pen pen = new Pen(color, 2);
            pen.DashStyle = ds;
            return pen;
        }

        private void styleComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            const int margin = 5;
            DashStyle ds = dashstyles[e.Index];
            using (Graphics g = e.Graphics)
            {
                using (Pen pen = GetDashStyleSample(DrawItemStateToColor(e.State), ds))
                {
                    g.DrawLine(pen, new Point(e.Bounds.Left + margin, e.Bounds.Top + (e.Bounds.Height / 2)),
                        new Point(e.Bounds.Right - margin, e.Bounds.Top + (e.Bounds.Height / 2)));
                }
            }
        }

        private void arrowComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            const int margin = 5;
            ArrowStyle ds = arrows[e.Index];
            using (Graphics g = e.Graphics)
            {
                using (Pen pen = GetArrowStyleSample(DrawItemStateToColor(e.State), ds))
                {
                    g.DrawLine(pen, new Point(e.Bounds.Left + margin, e.Bounds.Top + (e.Bounds.Height / 2)),
                        new Point(e.Bounds.Right - margin, e.Bounds.Top + (e.Bounds.Height / 2)));
                }
            }
        }

        private void widthComboBox_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                float f = Convert.ToSingle(widthComboBox.Text);
                if (!ValidWidth(f))
                    e.Cancel = true;
            }
            catch (Exception)
            {
                e.Cancel = true;
            }
        }

        private void DrawShape(Graphics g, DecorationMode mode, DecorationShape ds, Pen pen, Rectangle rect)
        {
            const int margin = 5;
            Rectangle where = rect;
            where.Inflate(-margin, 0);
            if (mode == DecorationMode.None)
                return;
            switch (ds)
            {
                case DecorationShape.Rectangle:
                    g.DrawRectangle(pen, where);
                    break;
                case DecorationShape.Ellipse:
                    g.DrawEllipse(pen, where);
                    break;
                case DecorationShape.Cross:
                    g.DrawLine(pen, new Point(where.X, where.Y), new Point(where.X + where.Width, where.Y + where.Height));
                    g.DrawLine(pen, new Point(where.X + where.Width, where.Y), new Point(where.X, where.Y + where.Height));
                    break;
                default:
                    throw new Exception("Unknown shape");
            }
        }

        private void samplePanel_Paint(object sender, PaintEventArgs e)
        {
            const int margin = 5;

            if (decorationGroupBox.Visible)
            {
                Rectangle myrect = samplePanel.ClientRectangle;
                myrect.Inflate(-45, -10);
                Decoration d = this.Decoration;
                using (Graphics g = e.Graphics)
                using (Pen pen = d.penstyle.GetPen())
                    DrawShape(g, d.mode, d.shape, pen, myrect);
            }
            else
            {
                PenStyle ps = this.Style;
                using (Graphics g = e.Graphics)
                using (Pen pen = ps.GetPen())
                {
                    Rectangle r = samplePanel.ClientRectangle;
                    if (traceGroupBox.Visible && traceCurveRadioButton.Checked)
                    {
                        g.DrawBezier(pen, new Point(r.Left + margin, r.Top + margin),
                            new Point(r.Left + r.Width / 4, r.Bottom),
                            new Point(r.Left + 3 * r.Width / 4, r.Top),
                            new Point(r.Right - margin, r.Bottom - margin));
                    }
                    else
                    {
                        g.DrawLine(pen, new Point(r.Left + margin, r.Top + r.Height / 2),
                            new Point(r.Right - margin, r.Top + r.Height / 2));
                    }
                }
            }
        }



        private void styleComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            samplePanel.Invalidate();
        }

        private void colorComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            samplePanel.Invalidate();
        }

        private void widthComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            samplePanel.Invalidate();
        }

        private void widthComboBox_TextUpdate(object sender, EventArgs e)
        {
            samplePanel.Invalidate();
        }

        private void shapeComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            const int margin = 5;
            Rectangle where = e.Bounds;
            where.Inflate(-margin, -margin);
            DecorationShape shape = shapes[e.Index];
            using (Graphics g = e.Graphics)
            using (Pen pen = new Pen(DrawItemStateToColor(e.State)))
            {
                DrawShape(g, DecorationMode.Node, shape, pen, where);
            }
        }

        private void shapeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGUI();
            samplePanel.Invalidate();
        }

        private void LineDialog_Shown(object sender, EventArgs e)
        {
            UpdateGUI();
        }

        private void noneRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (noneRadioButton.Checked)
                UpdateGUI();
            samplePanel.Invalidate();
        }

        private void nodeRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (nodeRadioButton.Checked)
                UpdateGUI();
            samplePanel.Invalidate();
        }

        private void subtreeRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (subtreeRadioButton.Checked)
                UpdateGUI();
            samplePanel.Invalidate();
        }

        private void traceCurveRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            samplePanel.Invalidate();
        }

        private void traceLineRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            samplePanel.Invalidate();
        }

        private void arrowComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            samplePanel.Invalidate();
        }



    }
}