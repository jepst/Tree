using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace Tree
{
    public interface GraphicsProxy
    {
        void DrawLine(Pen pen, int x, int y, int w, int h);
        void DrawLines(Pen pen, Point[] points);
        void FillRectangle(SolidBrush brush, int x, int y, int w, int h);
        void DrawRectangle(Pen pen, int x, int y, int w, int h);
        void DrawEllipse(Pen pen, int x, int y, int w, int h);
        void DrawBezier(Pen pen, Point a, Point b, Point c, Point d);
        void RenderRTF(RichTextBox2 rtf, int x, int y, int w, int h);
        void Clear(Color color);
        void DrawPolygon(Pen pen, Point[] points);
        SmoothingMode SmoothingMode { set;}
        void TranslateTransform(int x, int y);

        Graphics Graphics { get;}
    }

    /*
     * todo:
     * properly handle nontrivial text formatting and font names
     * arrow heads on lines
     * background color
     */

    /*
     * arrow heads like this:
     *  <defs>
        <marker id="endArrow" viewBox="0 0 10 10" refX="1" refY="5" markerUnits="strokeWidth" orient="auto" markerWidth="5" markerHeight="4">
        <polyline points="0,0 10,5 0,10 1,5" fill="darkblue" />
        </marker>
        </defs>
        <path marker-end="url(#endArrow)" d="M 50 70 L 50 80 L 10 80 L 10 70" style="stroke:rgb(0,0,0);stroke-width:1;fill:none"/>
     */
    public class GraphicsSVGWriter : GraphicsProxy, IDisposable
    {
        private StreamWriter o;
        private Stream destination;
        private MemoryStream buffer;
        private const int MAGICAL_TEXT_NOODLER = 5;
        private Point offset = Point.Empty;
        private List<string> defs;
        private Encoding encoding;
        public GraphicsSVGWriter(Stream os, Encoding encoding)
        {
            this.encoding = encoding;
            destination = os;
            buffer = new MemoryStream();
            o = new StreamWriter(buffer, encoding);
            defs = new List<string>();
        }
        public Graphics Graphics
        {
            get { return null; }
        }
        public void TranslateTransform(int x, int y)
        {
            offset = new Point(x, y);
        }

        public void Dispose()
        {
            o.WriteLine("</svg>");
            o.Flush();

            StreamWriter preludeWriter = new StreamWriter(destination,encoding);
            writePrelude(preludeWriter);
            preludeWriter.Flush();
            buffer.WriteTo(destination);
            preludeWriter.Close();
        }
        private void writePrelude(StreamWriter where)
        {
            string[] intro ={"<?xml version=\"1.0\" standalone=\"no\"?>",
                               "<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">",
                               "<!-- Created by "+MainWindow.GetAppSignature()+" -->",
                               "<svg width=\"100%\" height=\"100%\" version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\">"};
            foreach (string s in intro)
                where.WriteLine(s);
            if (defs.Count > 0)
            {
                where.WriteLine("<defs>");
                foreach (string l in defs)
                    where.WriteLine(l);
                where.WriteLine("</defs>");
            }
        }
        private String LinePatternToString(Pen pen)
        {
            int i = (int)pen.Width;
            const int dashsize = 3;
            switch (pen.DashStyle)
            {
                case DashStyle.Dot:
                    return string.Format("stroke-dasharray:{0},{1};",i,i);
                case DashStyle.Dash:
                    return string.Format("stroke-dasharray:{0},{1};", dashsize*i, i);
                case DashStyle.DashDot:
                    return string.Format("stroke-dasharray:{0},{1},{2},{3};", dashsize* i, i,i,i);
                case DashStyle.DashDotDot:
                    return string.Format("stroke-dasharray:{0},{1},{2},{3},{4},{5};", dashsize* i, i,i,i,i,i);
                    
                default:
                case DashStyle.Solid:
                    return "";
            }
        }

        private static int arrowId=0;
        private String EndCapToString(Pen pen)
        {
            if (pen.EndCap != LineCap.Custom)
                return "";
            ArrowStyle style = Hacks.GetLineCap(pen.CustomEndCap);
            switch (style)
            {
                case ArrowStyle.TinyArrow:
                case ArrowStyle.BigArrow:
                case ArrowStyle.LittleArrow:
                case ArrowStyle.LineArrow:
                case ArrowStyle.HollowArrow:
                case ArrowStyle.PointyArrow:
                    arrowId++;
                    defs.Add(String.Format("<marker id=\"arrow{1}\" viewBox=\"0 0 10 10\" refX=\"0\" refY=\"5\" stroke-width=\"1\" fill=\"rgb({0})\" orient=\"auto\">\n",ColorToRgb(pen.Color),arrowId)+
                                 "<path d=\"M 0 0 L 10 5 L 0 10 L 0 0\" />\n"+
                                 "</marker>");
                    break;
                case ArrowStyle.None:
                default:
                    return "";
            }
            return String.Format(" marker-end=\"url(#arrow{0})\"", arrowId);
        }
        private String PenToString(Pen pen)
        {
            return String.Format("{2}stroke:rgb({0});stroke-width:{1}", ColorToRgb(pen.Color),
                pen.Width,LinePatternToString(pen));
        }
        private String ColorToRgb(Color c)
        {
            int col = c.ToArgb();
            return string.Format("{0},{1},{2}", (col & 0xFF0000) >> 16, (col & 0xFF00) >> 8, col & 0xFF);
        }

        public void DrawLine(Pen pen, int x, int y, int w, int h)
        {
            String l = string.Format("<line x1=\"{0}\" y1=\"{1}\" x2=\"{2}\" y2=\"{3}\" style=\"{4}\"{5} />",
                x + offset.X, y + offset.Y, w + offset.X, h + offset.Y, PenToString(pen),EndCapToString(pen));
            o.WriteLine(l);
        }
        public void FillRectangle(Color color, int x, int y, int w, int h)
        {
            String l = string.Format("<rect x=\"{0}\" y=\"{1}\" width=\"{2}\" height=\"{3}\" style=\"fill:rgb({4})\" />",
                x + offset.X, y + offset.Y, w, h, ColorToRgb(color));
            o.WriteLine(l);
        }
        public void DrawEllipse(Pen pen, int x, int y, int w, int h)
        {
            String l = string.Format("<ellipse cx=\"{0}\" cy=\"{1}\" rx=\"{2}\" ry=\"{3}\" style=\"{4};fill:none\" />",
                x + w / 2 + offset.X, y + h / 2 + offset.Y, w / 2, h / 2, PenToString(pen));
            o.WriteLine(l);
        }

        public void DrawRectangle(Pen pen, int x, int y, int w, int h)
        {
            String l = string.Format("<rect x=\"{0}\" y=\"{1}\" width=\"{2}\" height=\"{3}\" style=\"{4};fill:none\" />",
                x + offset.X, y + offset.Y, w, h, PenToString(pen));
            o.WriteLine(l);
        }
        public void FillRectangle(SolidBrush brush, int x, int y, int w, int h)
        {
            this.FillRectangle(brush.Color, x, y, w, h);
        }
        public void DrawLines(Pen pen, Point[] points)
        {
            /*alternatively:
            for (int i = 1; i < points.Length; i++)
                this.DrawLine(pen, points[i - 1].X, points[i - 1].Y, points[i].X, points[i].Y);*/
            StringBuilder b = new StringBuilder();
            for (int i = 1; i < points.Length; i++)
                b.Append(string.Format(" L {0} {1}",points[i].X+offset.X,points[i].Y+offset.Y));
            String l = string.Format("<path d=\"M {0} {1}{2}\" style=\"{3};fill:none\"{4} />",
                points[0].X+offset.X, points[0].Y+offset.Y, b.ToString(), PenToString(pen),EndCapToString(pen));
            o.WriteLine(l);
        }

        public void DrawBezier(Pen pen, Point a, Point b, Point c, Point d)
        {
            String l = string.Format("<path d=\"M {0} {1} C {2} {3} {4} {5} {6} {7}\" style=\"{8};fill:none\"{9}/>",
                a.X+offset.X, a.Y+offset.Y, b.X+offset.X, b.Y+offset.Y, c.X+offset.X, 
                c.Y+offset.Y, d.X+offset.X, d.Y+offset.Y, PenToString(pen),EndCapToString(pen));
            o.WriteLine(l);
        }

        public void DrawPolygon(Pen pen, Point[] points)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (Point p in points)
            {
                if (!first)
                {
                    sb.Append(" ");
                }
                else
                    first = false;
                sb.Append(string.Format("{0},{1}", p.X + offset.X, p.Y + offset.Y));
            }
            String l = string.Format("<polygon points=\"{0}\" style=\"{1};fill:none\"/>", sb.ToString(),
                PenToString(pen));
            o.WriteLine(l);

        }
        public void RenderRTF(RichTextBox2 rtf, int x, int y, int w, int h)
        {
            /* 
                obviously doesn't handle
             * RTFs with nontrivial formatting: more than one style. The correct solution
             * is to parse out the RTF into... into what? 
             * And finally, we just output the local "true" name of each font, when we should
             * really be translating it into the names officially supported.
            */
            Font font = rtf.GetCurrentFont();
            int ch = font.Height;
            string[] lines = rtf.Lines;

            int lmax = 0;
            foreach (string line in lines)
            {
                if (line.Length > lmax)
                    lmax = line.Length;
            }
            int linenum = 1;
            o.WriteLine(string.Format("<text text-anchor=\"middle\" x=\"{0}\" y=\"{1}\">",
                x + MAGICAL_TEXT_NOODLER + offset.X + w / 2,
                y - MAGICAL_TEXT_NOODLER + offset.Y));
            foreach (string line in lines)
            {
                String l = string.Format("<tspan x=\"{0}\" dy=\"1em\" style=\"{1}\">{2}</tspan>",
                           x + MAGICAL_TEXT_NOODLER + offset.X + w / 2, FontToString(font, rtf.GetCurrentColor()),

                          System.Security.SecurityElement.Escape(line));
                o.WriteLine(l);
                linenum++;
            }
            o.WriteLine("</text>");

        }
        private String FontToString(Font f, Color c)
        {
            /*should take care of additional font styles, correct translation of font name*/
            String l = String.Format("fill:rgb({0});font-size:{1}pt;font-family:'{2}'",

            ColorToRgb(c), f.SizeInPoints, f.FontFamily.Name);
            return l;
        }
        public void Clear(Color color)
        {
            //This should either set some bgcolor tag (or SVG's equivalent thereof
            //to the given color or draw a filled rectangle equal to the maximum 
            //extents of the image
        }
        public SmoothingMode SmoothingMode
        {
            set
            {
                //nop
            }
        }

    }

    public class GraphicsPassThrough : GraphicsProxy
    {
        private Graphics g;
        private Size m_transform;
        public void Clear(Color color)
        {
            g.Clear(color);
        }
        public SmoothingMode SmoothingMode
        {
            set
            {
                g.SmoothingMode = value;
            }
        }
        public GraphicsPassThrough(Graphics g)
        {
            this.g = g;
            m_transform = Size.Empty;
        }
        public void DrawBezier(Pen pen, Point a, Point b, Point c, Point d)
        {
            g.DrawBezier(pen, a, b, c, d);
        }
        public void DrawLines(Pen pen, Point[] points)
        {
            g.DrawLines(pen, points);
        }
        public void DrawLine(Pen pen, int x, int y, int w, int h)
        {
            g.DrawLine(pen, x, y, w, h);
        }
        public void FillRectangle(SolidBrush brush, int x, int y, int w, int h)
        {
            g.FillRectangle(brush, x, y, w, h);
        }
        public void DrawRectangle(Pen pen, int x, int y, int w, int h)
        {
            g.DrawRectangle(pen, x, y, w, h);
        }
        public void DrawEllipse(Pen pen, int x, int y, int w, int h)
        {
            g.DrawEllipse(pen, x, y, w, h);
        }
        public void RenderRTF(RichTextBox2 rtf, int x, int y, int w, int h)
        {
            // The RTFToGraphics routine, based on EM_FORMATRANGE,
            // ignores the CLR-only transformation, so we have to factor it in manually here
            rtf.RTFToGraphics(g, x + m_transform.Width, y + m_transform.Height, w, h);
        }
        public void TranslateTransform(int x, int y)
        {
            m_transform = new Size(x, y);
            g.TranslateTransform(x, y);
        }
        public void DrawPolygon(Pen pen, Point[] points)
        {
            g.DrawPolygon(pen, points);
        }
        public Graphics Graphics
        {
            get { return g; }
        }

    }
}
