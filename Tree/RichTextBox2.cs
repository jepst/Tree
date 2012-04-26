using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace Tree
{
    /* Bizarre GetCleverSize hack inspired by this
     * http://www.dotnetmonster.com/Uwe/Forum.aspx/winform-controls/5064/using-EM-GETRECT-vs-using-EM-GETLINECOUNT-with-a-RichTextBox
     * and
     * http://msdn.microsoft.com/en-us/library/system.windows.forms.nativewindow.assignhandle.aspx
     * although this worries me:
     * http://www.codeproject.com/KB/edit/richeditsize.aspx?display=PrintAll
     * I just hope this works with other version of Richedit.dll`
     * 
     * RTB -> Bitmap converter inspried by Victor
     * http://bytes.com/groups/net-c/523657-c-printing-content-richtextbox-image-wysiwyg-mode
     * and http://support.microsoft.com/kb/812425
     * 
     * RTB contextmenu courtesy of 
     * http://social.msdn.microsoft.com/forums/en-US/vbgeneral/thread/327847d0-ac59-4596-b2f7-a88c1dc8fc61/
     * more or less
     * 
     * MS RTF reference 1.7 used for writing hacky zoomer
     */


    public class RichTextBox2 : RichTextBox
    {

        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        private class NativeTextBox : NativeWindow
        {
            private Control win;
            public NativeTextBox(Control win)
            {
                this.win = win;
                AssignHandle(win.Handle);
                win.HandleCreated += new EventHandler(this.OnHandleCreated);
                win.HandleDestroyed += new EventHandler(this.OnHandleDestroyed);
            }

            private void OnHandleCreated(object sender, EventArgs e)
            {
                AssignHandle(((Control)sender).Handle);
            }
            private void OnHandleDestroyed(object sender, EventArgs e)
            {
                ReleaseHandle();
            }

            [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if ((m.Msg == (WM_REFLECT | WM_NOTIFY)) || m.Msg == WM_NOTIFY)
                {
                    STRUCT_NMHDR hdr = (STRUCT_NMHDR)(Marshal.PtrToStructure(m.LParam, typeof(STRUCT_NMHDR)));
                    if (hdr.code == EN_OBJECTPOSITIONS)
                    {
                        m.Result = (IntPtr)(-1);
                    }
                    if (hdr.code == EN_REQUESTRESIZE)
                    {
                        STRUCT_REQRESIZE rrs = (STRUCT_REQRESIZE)(Marshal.PtrToStructure(m.LParam, typeof(STRUCT_REQRESIZE)));

                        Control from = Control.FromHandle(rrs.nmhdr.hwndFrom);
                        if (from == null)
                            return;

                        if (!(from is RichTextBox2))
                            return;
                        RichTextBox2 rtb2 = (RichTextBox2)from;
                        rtb2.native_rect = new Size(rrs.rc.right - rrs.rc.left, rrs.rc.bottom - rrs.rc.top);
                    }
                }
            }
        }

        private NativeTextBox native;
        private Size native_rect;
        private float m_zoomfactor;
        private float dpix;

        public RichTextBox2()
            : base()
        {
            dpix = 0;
            native = null;
            m_zoomfactor = 1.0f;
            //            SendMessage(this.Handle, EM_SETEDITSTYLE, SES_SCROLLONKILLFOCUS, (IntPtr)SES_SCROLLONKILLFOCUS);
        }

        public void EnableCleverness()
        {
            if (native != null)
                throw new Exception("Cleverness already enabled");
            if (Parent == null)
                throw new Exception("Cleverness requires parent");
            if (Parent.Handle != GetParent(Handle))
                throw new Exception("Unusual parent shenanigans");
            native = new NativeTextBox(Parent);
            SetStyle(ControlStyles.EnableNotifyMessage, true);

            IntPtr mask = SendMessage(native.Handle, EM_GETEVENTMASK, (IntPtr)0, (IntPtr)0); //THE KEY LINE THAT FIXES THE 64-BIT VERSION
            mask = (IntPtr)((Int32)mask | (ENM_REQUESTRESIZE | ENM_OBJECTPOSITIONS));

            SendMessage(this.Handle, EM_SETEVENTMASK, (IntPtr)0, mask);
        }

        public Size GetCleverSize()
        {
            if (native == null)
                throw new Exception("Not clever enough!");
            native_rect = Size.Empty;
            SendMessage(this.Handle, EM_REQUESTRESIZE, (IntPtr)0, (IntPtr)0);

            STRUCT_NMHDR foo=new STRUCT_NMHDR();
            foo.code = EN_OBJECTPOSITIONS;
            IntPtr ptr=Marshal.AllocHGlobal(Marshal.SizeOf(foo));
            Marshal.StructureToPtr(foo, ptr, false);
            SendMessage(native.Handle,WM_NOTIFY,(IntPtr)0,ptr);

            if (native_rect == Size.Empty)
                throw new Exception("Cleverness failed");
            return native_rect;
        }

        protected override void Dispose(bool disposing)
        {
            if (native != null)
            {
                native.ReleaseHandle();
                native = null;
            }
            base.Dispose(disposing);
        }

        public new string Rtf
        {
            get
            {
                return base.Rtf;
            }
            set
            {
                if (m_zoomfactor != 1.0f)
                {
                    String result = ZoomFonts(m_zoomfactor, value);
                    base.Rtf = result;
                    //Console.WriteLine("from:\n{0}\nTo:\n{1}", value, result);
                }
                else
                    base.Rtf = value;
            }
        }

        public new float ZoomFactor
        {
            get
            {
                return m_zoomfactor;
            }
            set
            {
                m_zoomfactor = value;
            }
        }

        private static String ZoomFonts(float factor, String rtf)
        {
            StringBuilder sb = new StringBuilder(rtf.Length + 20);
            StringBuilder control = null;
            int mode = 0;
            for (int i = 0; i < rtf.Length; i++)
            {
                bool isalpha = Char.IsLetter(rtf[i]); //(rtf[i] >= 'a' && rtf[i] <= 'z') || (rtf[i] >= 'A' && rtf[i] <= 'Z');
                bool isnum = Char.IsNumber(rtf[i]); // (rtf[i] >= '0' && rtf[i] <= '9');
                switch (mode)
                {
                    case 3:
                        if (isnum)
                            control.Append(rtf[i]);
                        else
                        {
                            uint x = Convert.ToUInt32(control.ToString());
                            x = (uint)((float)x * (factor));
                            sb.Append(Convert.ToString(x));
                            if (rtf[i] == '\\')
                                mode = 1;
                            else
                                mode = 0;
                            sb.Append(rtf[i]);
                        }
                        break;
                    case 2:
                        if (isalpha)
                            control.Append(rtf[i]);
                        else
                        {
                            if (isnum && control.ToString() == "fs")
                            {
                                mode = 3;
                                control = new StringBuilder();
                                control.Append(rtf[i]);
                                break;
                            }
                            mode = 0;
                        }
                        if (rtf[i] == '\\')
                            mode = 1;
                        sb.Append(rtf[i]);
                        break;
                    case 0:
                        if (rtf[i] == '\\')
                            mode = 1;
                        sb.Append(rtf[i]);
                        break;
                    case 1:
                        if (isalpha)
                        {
                            control = new StringBuilder();
                            control.Append(rtf[i]);
                            mode = 2;
                        }
                        else
                            mode = 0;
                        sb.Append(rtf[i]);
                        break;
                }

            }
            return sb.ToString();
        }


        public void Reset()
        {
            base.ResetText();
            m_zoomfactor = 1.0f;
        }

        private Ternary GetFontEffect(uint mask, uint effect)
        {
            STRUCT_CHARFORMAT2 charFormat2 = new STRUCT_CHARFORMAT2();

            GetCharFormat(ref charFormat2);
            if ((charFormat2.dwMask & mask) == 0)
                return Ternary.Indeterminate;
            return ((charFormat2.dwEffects & effect) != 0) ? Ternary.Yes : Ternary.No;
        }

        private void SetFontEffect(uint mask, uint effect, Ternary val)
        {
            if (val == Ternary.Indeterminate)
                return;
            STRUCT_CHARFORMAT2 charFormat2 = new STRUCT_CHARFORMAT2();
            charFormat2.dwMask = mask;
            charFormat2.dwEffects = val == Ternary.Yes ? effect : 0;
            SetCharFormat(ref charFormat2);
        }

        public FormatChanges GetFormatChanges()
        {
            FormatChanges fc = new FormatChanges();
            fc.color = SelectionColor;
            fc.font = SelectionFont;
            STRUCT_CHARFORMAT2 cf2 = new STRUCT_CHARFORMAT2();
            GetCharFormat(ref cf2);
            if ((cf2.dwMask & CFM_BOLD) != 0)
                fc.bold = ((cf2.dwEffects & CFE_BOLD) != 0) ? Ternary.Yes : Ternary.No;
            if ((cf2.dwMask & CFM_ITALIC) != 0)
                fc.italic = ((cf2.dwEffects & CFE_ITALIC) != 0) ? Ternary.Yes : Ternary.No;
            if ((cf2.dwMask & CFM_UNDERLINE) != 0)
                fc.underline = ((cf2.dwEffects & CFE_UNDERLINE) != 0) ? Ternary.Yes : Ternary.No;
            if ((cf2.dwMask & CFM_SUPERSCRIPT) != 0)
                fc.superscript = ((cf2.dwEffects & CFE_SUPERSCRIPT) != 0) ? Ternary.Yes : Ternary.No;
            if ((cf2.dwMask & CFM_SUBSCRIPT) != 0)
                fc.subscript = ((cf2.dwEffects & CFE_SUBSCRIPT) != 0) ? Ternary.Yes : Ternary.No;
            if ((cf2.dwMask & CFM_STRIKEOUT) != 0)
                fc.strikeout = ((cf2.dwEffects & CFE_STRIKEOUT) != 0) ? Ternary.Yes : Ternary.No;
            if ((cf2.dwMask & CFM_FACE) != 0)
                fc.fontfamily = cf2.szFaceName;
            if ((cf2.dwMask & CFM_SIZE) != 0)
                fc.fontsize = cf2.yHeight / 20f; /*again, this may not be right. why 20?*/
            return fc;
        }

        public void SetFormatChanges(FormatChanges fc)
        {
            if (fc.color != Color.Empty)
                SelectionColor = fc.color;
            if (fc.font != null)
                SelectionFont = fc.font;

            STRUCT_CHARFORMAT2 cf2 = new STRUCT_CHARFORMAT2();
            if (fc.fontfamily != String.Empty)
            {
                cf2.dwMask |= CFM_FACE;
                cf2.szFaceName = fc.fontfamily;
            }
            if (fc.fontsize != 0f)
            {
                cf2.dwMask |= CFM_SIZE;
                cf2.yHeight = (int)(fc.fontsize * 20f); /*no idea if this is correct conversion*/
            }
            if (fc.bold != Ternary.Indeterminate)
            {
                cf2.dwMask |= CFM_BOLD;
                if (fc.bold == Ternary.Yes)
                    cf2.dwEffects |= CFE_BOLD;
            }
            if (fc.italic != Ternary.Indeterminate)
            {
                cf2.dwMask |= CFM_ITALIC;
                if (fc.italic == Ternary.Yes)
                    cf2.dwEffects |= CFE_ITALIC;
            }
            if (fc.underline != Ternary.Indeterminate)
            {
                cf2.dwMask |= CFM_UNDERLINE;
                if (fc.underline == Ternary.Yes)
                    cf2.dwEffects |= CFE_UNDERLINE;
            }
            if (fc.strikeout != Ternary.Indeterminate)
            {
                cf2.dwMask |= CFM_STRIKEOUT;
                if (fc.strikeout == Ternary.Yes)
                    cf2.dwEffects |= CFE_STRIKEOUT;
            }
            if (fc.superscript != Ternary.Indeterminate)
            {
                cf2.dwMask |= CFM_SUPERSCRIPT;
                if (fc.superscript == Ternary.Yes)
                    cf2.dwEffects |= CFE_SUPERSCRIPT;
            }
            if (fc.subscript != Ternary.Indeterminate)
            {
                cf2.dwMask |= CFM_SUBSCRIPT;
                if (fc.subscript == Ternary.Yes)
                    cf2.dwEffects |= CFE_SUBSCRIPT;
            }
            if (cf2.dwMask != 0)
                SetCharFormat(ref cf2);
        }

        public Ternary Superscript
        {
            get
            {
                return GetFontEffect(CFM_SUPERSCRIPT, CFE_SUPERSCRIPT);
            }
            set
            {
                SetFontEffect(CFM_SUPERSCRIPT, CFE_SUPERSCRIPT, value);
            }
        }

        public Ternary Subscript
        {
            get
            {
                return GetFontEffect(CFM_SUBSCRIPT, CFE_SUBSCRIPT);
            }
            set
            {
                SetFontEffect(CFM_SUBSCRIPT, CFE_SUBSCRIPT, value);
            }
        }

        public Ternary Bold
        {
            get
            {
                return GetFontEffect(CFM_BOLD, CFE_BOLD);
            }
            set
            {
                SetFontEffect(CFM_BOLD, CFE_BOLD, value);
            }
        }
        public Ternary Italic
        {
            get
            {
                return GetFontEffect(CFM_ITALIC, CFE_ITALIC);
            }
            set
            {
                SetFontEffect(CFM_ITALIC, CFE_ITALIC, value);
            }
        }

        public Ternary Underline
        {
            get
            {
                return GetFontEffect(CFM_UNDERLINE, CFE_UNDERLINE);
            }
            set
            {
                SetFontEffect(CFM_UNDERLINE, CFE_UNDERLINE, value);
            }
        }


        private void SetCharFormat(ref STRUCT_CHARFORMAT2 charFormat2)
        {
            charFormat2.cbSize = Marshal.SizeOf(charFormat2);
            /*
            charFormat2.dwMask |= CFM_UNDERLINE | CFM_UNDERLINETYPE;
            charFormat2.dwEffects |= CFE_UNDERLINE;
            charFormat2.bUnderlineType = CFU_UNDERLINETHICK;
             */

            SendMessageCharFormat2(this.Handle, EM_SETCHARFORMAT, (IntPtr)SCF_SELECTION, ref charFormat2);
        }
        private void GetCharFormat(ref STRUCT_CHARFORMAT2 charFormat2)
        {
            charFormat2.cbSize = Marshal.SizeOf(charFormat2);
            SendMessageCharFormat2(this.Handle, EM_GETCHARFORMAT, (IntPtr)SCF_SELECTION, ref charFormat2);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessageCharFormat2(IntPtr hWnd, UInt32 Msg, IntPtr wParam, ref STRUCT_CHARFORMAT2 cf2);

        [DllImport("user32.dll")]
        private static extern IntPtr GetParent(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWnd, IntPtr hNewParent);



        //richedit options
        private const Int32 SES_SCROLLONKILLFOCUS = 8192;

        //wm_notify subcodes
        private const Int32 EN_REQUESTRESIZE = 0x701;
        private const Int32 EN_OBJECTPOSITIONS = 0x07a;

        //richedit notification request flags
        private const Int32 ENM_REQUESTRESIZE = 0x00040000;
        private const Int32 ENM_OBJECTPOSITIONS = 0x02000000;

        //messages to windows
        private const Int32 WM_USER = 0x400;
        private const Int32 WM_REFLECT = WM_USER + 0x1C00;
        private const Int32 WM_PASTE = 0x0302;
        private const Int32 WM_NOTIFY = 0x4e;
        private const Int32 WM_CONTEXTMENU = 0x7b;

        //messages to richedit controls
        private const Int32 EM_FORMATRANGE = WM_USER + 57;
        private const Int32 EM_SETCHARFORMAT = WM_USER + 68;
        private const Int32 EM_GETCHARFORMAT = WM_USER + 58;
        private const Int32 EM_GETEVENTMASK = WM_USER + 59;
        private const Int32 EM_PASTESPECIAL = WM_USER + 64;
        private const Int32 EM_REQUESTRESIZE = WM_USER + 65;
        private const Int32 EM_SETEVENTMASK = WM_USER + 69;
        private const Int32 EM_SETEDITSTYLE = WM_USER + 204;
        private const Int32 EM_SETTEXTMODE = WM_USER + 89;

        //richedit selection type
        public const int SCF_SELECTION = 0x0001;
        public const int SCF_WORD = 0x0002;
        public const int SCF_DEFAULT = 0x0000;
        public const int SCF_ALL = 0x0004;

        //richedit character format masks (validity indicator)
        public const uint CFM_FACE = 0x20000000;
        public const uint CFM_SIZE = 0x80000000;
        public const uint CFM_COLOR = 0x40000000;
        public const uint CFM_BOLD = 0x00000001;
        public const uint CFM_ITALIC = 0x00000002;
        public const uint CFM_UNDERLINE = 0x00000004;
        public const uint CFM_STRIKEOUT = 0x00000008;
        public const uint CFM_UNDERLINETYPE = 0x00800000;
        public const uint CFM_SUBSCRIPT = CFE_SUBSCRIPT | CFE_SUPERSCRIPT;
        public const uint CFM_SUPERSCRIPT = CFM_SUBSCRIPT;
        public const uint CFM_effects = CFM_BOLD | CFM_ITALIC | CFM_UNDERLINE | CFM_STRIKEOUT | CFM_SUBSCRIPT | CFM_SUPERSCRIPT;

        //richedit effects
        public const int CFE_BOLD = 0x0001;
        public const int CFE_ITALIC = 0x0002;
        public const int CFE_UNDERLINE = 0x0004;
        public const int CFE_STRIKEOUT = 0x0008;
        public const int CFE_SUBSCRIPT = 0x00010000;
        public const int CFE_SUPERSCRIPT = 0x00020000;

        //richedit underline styles
        public const byte CFU_UNDERLINENONE = 0;
        public const byte CFU_UNDERLINE = 1;
        public const byte CFU_UNDERLINEDOTTED = 4;
        public const byte CFU_UNDERLINEDASH = 5;
        public const byte CFU_UNDERLINEDASHDOT = 6;
        public const byte CFU_UNDERLINEDASHDOTDOT = 7;
        public const byte CFU_UNDERLINEWAVE = 8;
        public const byte CFU_UNDERLINETHICK = 9;

        private int FormatRangeBmp(bool measureOnly, Bitmap b, int charFrom, int charTo,
            int x, int y)
        {
            // Specify which characters to print
            int result;
            using (Graphics g = Graphics.FromImage(b))
                result = FormatRange(measureOnly, g, charFrom, charTo, x, y, b.Width, b.Height);
            return result;
        }

        private int FormatRange(bool measureOnly, Graphics g, int charFrom, int charTo, int x, int y, int w, int h)
        {
            STRUCT_CHARRANGE cr;
            cr.cpMin = charFrom;
            cr.cpMax = charTo;

            if (dpix == 0)
                dpix = g.DpiX;
            IntPtr hdc = g.GetHdc();

            // Specify the area inside page margins
            STRUCT_RECT rc;
            rc.top = HundredthInchToTwips(y);
            rc.bottom = HundredthInchToTwips(h);
            rc.left = HundredthInchToTwips(x);
            rc.right = HundredthInchToTwips(w);

            // Specify the page area
            STRUCT_RECT rcPage;
            rcPage.top = HundredthInchToTwips(y);
            rcPage.bottom = HundredthInchToTwips(h);
            rcPage.left = HundredthInchToTwips(x);
            rcPage.right = HundredthInchToTwips(w);

            // Get device context of output device

            // Fill in the FORMATRANGE struct
            STRUCT_FORMATRANGE fr;
            fr.chrg = cr;
            fr.hdc = hdc;
            fr.hdcTarget = hdc;
            fr.rc = rc;
            fr.rcPage = rcPage;

            // Non-Zero wParam means render, Zero means measure
            IntPtr wParam = (IntPtr)(measureOnly ? 0 : 1);

            // Allocate memory for the FORMATRANGE struct and
            // copy the contents of our struct to this memory
            IntPtr lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(fr));
            Marshal.StructureToPtr(fr, lParam, false);

            // Send the actual Win32 message
            int res = (int)SendMessage(this.Handle, EM_FORMATRANGE, wParam, lParam);

            // Free allocated memory
            Marshal.FreeCoTaskMem(lParam);

            // and release the device context
            g.ReleaseHdc(hdc);

            return res;
        }





        private Int32 HundredthInchToTwips(int n)
        {
            return (Int32)(n * (1440 / dpix));
        }


        // C#
        /// <summary>
        /// Free cached data from rich edit control after printing
        /// </summary>
        public void FormatRangeDone()
        {
            IntPtr lParam = new IntPtr(0);
            SendMessage(this.Handle, EM_FORMATRANGE, (IntPtr)0, lParam);
        }


        public void RTFtoBitmap(Bitmap b)
        {
            RTFtoBitmap(b, 0, 0);
        }

        public void RTFToGraphics(Graphics g, int x, int y, int w, int h)
        {
            this.FormatRange(false, g, 0, Text.Length, x, y, w + x, h + y);
            this.FormatRangeDone();
        }

        public void RTFtoBitmap(Bitmap b, int x, int y)
        {

            this.FormatRangeBmp(false, b,
                0, Text.Length, x, y);

            this.FormatRangeDone();

        }
        /*
        public Bitmap DrawToBitmap()
        {
            IntPtr srcDC = GetDC(this.Handle);
            Bitmap bm = new Bitmap(this.Width,
                this.Height);
            Graphics g = Graphics.FromImage(bm);
            IntPtr bmDC = g.GetHdc();
            BitBlt(bmDC, 0, 0, bm.Width, bm.Height, srcDC, 0, 0, 0x00CC0020);
            ReleaseDC(this.Handle, srcDC);
            g.ReleaseHdc(bmDC);
            g.Dispose();

            return bm;
        }*/

        public Font GetCurrentFont()
        {
            Font res = SelectionFont;
            if (res != null)
                return res;
            else
            {
                int start = SelectionStart;
                int len = SelectionLength;
                Select(0, 0);
                res = SelectionFont;
                if (res == null)
                    throw new TreeException("Highly unusual font behavior");
                Select(start, len);
                return res;
            }
        }
        public Color GetCurrentColor()
        {
            Color res = SelectionColor;
            if (res != Color.Empty)
                return res;
            else
            {
                int start = SelectionStart;
                int len = SelectionLength;
                Select(0, 0);
                res = SelectionColor;
                if (res == Color.Empty)
                    throw new TreeException("Highly unusual font behavior");
                Select(start, len);
                return res;
            }
        }

    }
    public class DynamicToolStripMenuItem : ToolStripMenuItem
    {
    }
    public class RichTextContextMenuStrip : ContextMenuStrip
    {
        private RichTextBox rtb;
        void Undo(object sender, EventArgs e)
        {
            rtb.Undo();
        }
        void Cut(object sender, EventArgs e)
        {
            rtb.Cut();
        }
        void Copy(object sender, EventArgs e)
        {
            rtb.Copy();
        }
        void Paste(object sender, EventArgs e)
        {
            rtb.Paste();
        }
        void PasteAsText(object sender, EventArgs e)
        {
            rtb.Paste(DataFormats.GetFormat(DataFormats.Text));
        }
        void SelectAll(object sender, EventArgs e)
        {
            rtb.SelectAll();
        }

        private void SetUp()
        {
            this.Items[2].Enabled = rtb.CanUndo;
            this.Items[4].Enabled = rtb.SelectionLength > 0;
            this.Items[5].Enabled = rtb.SelectionLength > 0;
            this.Items[6].Enabled = Clipboard.ContainsText();
            this.Items[7].Enabled = Clipboard.ContainsText();
        }



        public RichTextContextMenuStrip(RichTextBox rtb)
            : base()
        {
            this.rtb = rtb;
            Opening += new System.ComponentModel.CancelEventHandler(RichTextContextMenuStrip_Opening);
            ToolStripMenuItem special = new ToolStripMenuItem("&Insert special", null,
                new ToolStripMenuItem("\x22ee"), //vertical ellipsis
                new ToolStripMenuItem("\x2205"), //empty set
                new ToolStripMenuItem("\x2203"), //there exists
                new ToolStripMenuItem("\x2200")  //for all
                );
            special.DropDownItemClicked += new ToolStripItemClickedEventHandler(special_DropDownItemClicked);
            ToolStripMenuItem undo = new ToolStripMenuItem("&Undo", null, Undo);
            ToolStripMenuItem cut = new ToolStripMenuItem("Cu&t", null, Cut);
            ToolStripMenuItem copy = new ToolStripMenuItem("&Copy", null, Copy);
            ToolStripMenuItem paste = new ToolStripMenuItem("&Paste", null, Paste);
            ToolStripMenuItem pasteastext = new ToolStripMenuItem("Paste as &Text", null, PasteAsText);
            ToolStripMenuItem selectall = new ToolStripMenuItem("Select &All", null, SelectAll);
            this.Items.AddRange(new ToolStripItem[] {
                special,
                new ToolStripSeparator(),
                undo,
                new ToolStripSeparator(),
                cut,
                copy,
                paste,
                pasteastext,
                new ToolStripSeparator(),
                    selectall,
                });
        }

        void special_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            rtb.SelectedText = e.ClickedItem.Text;
        }

        void RichTextContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SetUp();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STRUCT_NMHDR
    {
        public IntPtr hwndFrom;
        public IntPtr idFrom;
        public uint code;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STRUCT_RECT
    {
        public Int32 left;
        public Int32 top;
        public Int32 right;
        public Int32 bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STRUCT_CHARRANGE
    {
        public Int32 cpMin;
        public Int32 cpMax;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STRUCT_REQRESIZE
    {
        public STRUCT_NMHDR nmhdr;
        public STRUCT_RECT rc;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
    public struct STRUCT_CHARFORMAT2
    {
        public int cbSize;
        public uint dwMask;
        public uint dwEffects;
        public int yHeight;
        public int yOffset;
        public int crTextColor;
        public byte bCharSet;
        public byte bPitchAndFamily;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szFaceName;
        public short wWeight;
        public short sSpacing;
        public int crBackColor;
        public int lcid;
        public int dwReserved;
        public short sStyle;
        public short wKerning;
        public byte bUnderlineType;
        public byte bAnimation;
        public byte bRevAuthor;
        public byte bReserved1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct STRUCT_FORMATRANGE
    {
        public IntPtr hdc;
        public IntPtr hdcTarget;
        public STRUCT_RECT rc;
        public STRUCT_RECT rcPage;
        public STRUCT_CHARRANGE chrg;
    }
    public enum Ternary
    {
        Yes,
        No,
        Indeterminate
    }
    public sealed class Hacks
    {
        private Hacks() { }

        private static Dictionary<ArrowStyle,CustomLineCap> linecaps;

        public static ArrowStyle GetLineCap(CustomLineCap custom)
        {
            //This should return the actual kind of arrow;
            //however, apparently this information isn't
            //available via the CustomLineCap; this means
            //that GraphicsProxy needs to be re-thought out
            //and that endcaps need to be passed separately,
            //not as part of the Pen
            return ArrowStyle.TinyArrow;
        }

        public static CustomLineCap GetLineCap(ArrowStyle style)
        {
            if (linecaps==null)
            {
                linecaps = new Dictionary<ArrowStyle, CustomLineCap>();
            }
            CustomLineCap clc;
            if (!linecaps.TryGetValue(style,out clc))
                linecaps.Add(style,clc=GenerateLineCap(style));
            return clc;
        }

        private static CustomLineCap GenerateLineCap(ArrowStyle style)
        {
            CustomLineCap clc = null;
            using (GraphicsPath gp = new GraphicsPath())
                switch (style)
                {
                    case ArrowStyle.PointyArrow:
                        gp.AddPolygon(new PointF[]{new PointF(0f,-0f),new PointF(.1f,-.2f),
                        new PointF(0f,-.15f),
                        new PointF(-.10f,-.2f)});
                        clc = new CustomLineCap(gp,null);
                        clc.WidthScale = 30;
                        clc.BaseInset = .15f;
                        break;
                    case ArrowStyle.HollowArrow:
                        gp.AddPolygon(new PointF[] { new PointF(0,-1f),new PointF(2f,-4f),
                    new PointF(-2f,-4f)});
                        clc = new CustomLineCap(null, gp);
                        clc.BaseInset = 5.0f;
                        break;
                    case ArrowStyle.BigArrow:
                        gp.AddPolygon(new PointF[]{new PointF(0,0f),new PointF(.25f,-.4f),
                        new PointF(-.25f,-.4f)});
                        clc = new CustomLineCap(gp, null);
                        clc.BaseInset = .4f;
                        clc.WidthScale = 15;
                        break;
                    case ArrowStyle.LineArrow:
                        gp.AddLine(new PointF(0,0),new PointF(2.5f,-4f));
                        gp.AddLine(new PointF(0,0),new PointF(-2.5f,-4f));
                        clc = new CustomLineCap(null,gp);
                        clc.BaseInset = 3f;
                        break;
                    case ArrowStyle.TinyArrow:
                        gp.AddPolygon(new PointF[]{new PointF(0,0f),new PointF(.25f,-.4f),
                        new PointF(-.25f,-.4f)});
                        clc = new CustomLineCap(gp, null);
                        clc.BaseInset = .4f;
                        clc.WidthScale = 5;
                        break;
                    case ArrowStyle.LittleArrow:
                        gp.AddPolygon(new PointF[]{new PointF(0,0f),new PointF(.25f,-.4f),
                        new PointF(-.25f,-.4f)});
                        clc = new CustomLineCap(gp, null);
                        clc.BaseInset = .4f;
                        clc.WidthScale = 10;
                        break;
                    default:
                        throw new Exception("Unexpected arrow type");
                }
            return clc;
        }

        public static byte[] Serialize(object o)
        {
            using (MemoryStream memstream = new MemoryStream())
            {
                using (GZipStream zs = new GZipStream(memstream, CompressionMode.Compress))
                {
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(zs, o);
                }
                return memstream.ToArray();
            }
        }

        public static object Deserialize(byte[] data)
        {
            using (MemoryStream memstream = new MemoryStream(data, false))
            {
                using (GZipStream zs = new GZipStream(memstream, CompressionMode.Decompress))
                {
                    IFormatter formatter = new BinaryFormatter();
                    object o = formatter.Deserialize(zs);
                    return o;
                }
            }
        }

        public static String[] GetFontNames()
        {
            FontFamily[] families = FontFamily.Families;
            String[] names = new String[families.Length];
            for (int i = 0; i < families.Length; i++)
                names[i] = families[i].Name;
            return names;

        }
        public static SolidBrush[] GetColorBrushes()
        {
            SolidBrush[] colors;
            PropertyInfo[] colorprops = typeof(Brushes).GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty);
            colors = new SolidBrush[colorprops.Length - 1];
            for (int i = 1; i < colorprops.Length; i++)
            {
                object val = colorprops[i].GetValue(null, null);
                if (val is SolidBrush)
                    colors[i - 1] = (SolidBrush)val;
                else
                    colors[i - 1] = (SolidBrush)Brushes.Black;
            }
            return colors;
        }
    }

}