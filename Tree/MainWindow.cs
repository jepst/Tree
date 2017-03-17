using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing.Imaging;

namespace Tree
{
    public partial class MainWindow : Form
    {
        private const string APP_NAME = "Syntax Tree Editor";
        private const string FILE_SUFFIX = ".stree";
        private const string FILE_DESC = "Tree files";
        private string filename;
        public static string GetAppSignature()
        {
            return APP_NAME;
        }
        public MainWindow()
        {
            InitializeComponent();
            this.nodeEditor.SetSTV(syntaxTreeViewer);
#if !DEBUG
            this.feedbackToolStripMenuItem.Enabled = false;
#endif
            BlankDocument();

            UpdateGUI();

            InitFontToolBar();
            UpdateFontToolBar();

            
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            syntaxTreeViewer.RequestForElem(new RequestforElemWelcome());
        }

        private class RequestforElemWelcome : RequestForElem
        {
            public String Text
            {
                get
                {
#if DEBUG
                    return "Welcome to the beta version of " + APP_NAME + ", and "+
                           "thanks for helping us develop our program. To begin "+
                           "editing your syntax tree, click on the red X below.\r\n\r\n"+
                           "At any time, you can send commentary to the authors by "+
                           "selecting \"Provide feedback\" from the Help menu.";
#else
                    return "Welcome to "+APP_NAME+".\r\n\r\nTo begin editing your syntax "+
                        "tree, click on the red X below.";
#endif
                }
            }
            public void Invoke(Elem elem)
            {
            }
            public bool Query(Elem elem)
            {
                return true;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.D1))
            {
                applyBold();
                return true;
            }
            if (keyData == (Keys.Control | Keys.D2))
            {
                applyItalic();
                return true;
            }
            if (keyData == (Keys.Control | Keys.D3))
            {
                applyUnderline();
                return true;
            }
            if (keyData == (Keys.Control | Keys.D4))
            {
                applySubscript();
                return true;
            }
            if (keyData == (Keys.Control | Keys.D5))
            {
                applySuperscript();
                return true;
            }
            if (keyData == (Keys.Control | Keys.D6))
            {
                applyStrikeout();
                return true;
            }

            if (keyData == (Keys.Control | Keys.Space) ||
                keyData == Keys.Escape)
            {
                syntaxTreeViewer.Focus();
                return true;
            }
            if (keyData == undoStripMenuItem.ShortcutKeys)
            {
                syntaxTreeViewer.UndoUndo(true);
                return true;
            }
            if (keyData == (insertChildNodeToolStripMenuItem.ShortcutKeys | Keys.Shift))
            {
                syntaxTreeViewer.DoAddChild(true, true);
                return true;
            }
            if (keyData == insertChildNodeToolStripMenuItem.ShortcutKeys)
            {
                syntaxTreeViewer.DoAddChild(true, false);
                return true;
            }
            if (keyData == insertParentNodeToolStripMenuItem.ShortcutKeys)
            {
                syntaxTreeViewer.DoAddParent(true);
                return true;
            }
            if (keyData == deleteNodeToolStripMenuItem.ShortcutKeys ||
                keyData == Keys.Delete)
            {
                syntaxTreeViewer.DoDeleteNode(true);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private String GetStandardFilter()
        {
            return FILE_DESC + " (*" + FILE_SUFFIX + ")|*" + FILE_SUFFIX +
                "|All files (*.*)|*.*";
        }

        private string DisplayFileName(string fn)
        {
            Path.GetFileNameWithoutExtension(fn);
            if (Path.GetExtension(fn) == FILE_SUFFIX)
                return Path.GetFileNameWithoutExtension(fn);
            else
                return Path.GetFileName(fn);
        }

        private void UpdateTitleBar()
        {
            string title;
            if (filename == null)
                title = APP_NAME;
            else
                title = string.Format("{0} - {1}", DisplayFileName(filename), APP_NAME);
            this.Text = title;
        }


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Error(Exception e)
        {
            MessageBox.Show(this, e.ToString(), APP_NAME, MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private void ReDraw()
        {
            syntaxTreeViewer.Invalidate();
        }

        private void HorizontalSplitContainer_Panel1_Resize(object sender, EventArgs e)
        {
        }


        private bool Changed()
        {
            return syntaxTreeViewer.Dirty;
        }

        private void SetHasChanged()
        {
            syntaxTreeViewer.Dirty = true;
        }

        private void SetHasntChanged()
        {
            syntaxTreeViewer.Dirty = false;
        }

        private bool DoSaveAs()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = GetStandardFilter();
                if (sfd.ShowDialog(this) == DialogResult.OK)
                    try
                    {
                        using (Stream os = sfd.OpenFile())
                        {
                            if (os != null)
                            {
                                IFormatter formatter = new BinaryFormatter();
                                WriteCurrent(os);
                                this.filename = sfd.FileName;
                                UpdateTitleBar();
                                SetHasntChanged();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Error(ex);
                        return false;
                    }
                else
                    return false;
                return true;
            }
        }

        private bool DoExport()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PNG (*.png)|*.png|GIF (*.gif)|*.gif|Windows Bitmap (*.bmp)|*.bmp|SVG (Unicode) (*.svg)|*.svg|SVG (ASCII) (*.svg)|*.svg";
                sfd.Title = "Export";
                if (sfd.ShowDialog(this) == DialogResult.OK)
                    try
                    {
                        ImageFormat selectedFormat = ImageFormat.Png;
                        Encoding encoding = Encoding.ASCII;
                        switch (sfd.FilterIndex)
                        {
                            case 1: selectedFormat = ImageFormat.Png;
                                break;
                            case 2: selectedFormat = ImageFormat.Gif;
                                break;
                            case 3: selectedFormat = ImageFormat.Bmp;
                                break;
                            case 4: encoding = Encoding.Unicode;
                                break;
                            case 5: encoding = Encoding.ASCII;
                                break;
                            default:
                                throw new TreeException("Random or unexpected image format");
                        }

                        using (Stream os = sfd.OpenFile())
                        {
                            if (sfd.FilterIndex == 4 || sfd.FilterIndex == 5)
                            {
                                syntaxTreeViewer.WriteSVG(os, encoding);
                            }
                            else
                            {
                                using (Bitmap bmp = syntaxTreeViewer.GetBitmap())
                                    bmp.Save(os, selectedFormat);
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        Error(ex);
                        return false;
                    }
                    catch (Exception ex) /*probably for memory errors for huge bitmaps*/
                    {
                        Error(ex);
                        return false;
                    }
                else
                    return false;
                return true;
            }
        }

        public bool LoadFile(string fname)
        {
            try
            {
                using (FileStream ifs = new FileStream(fname, FileMode.Open))
                {
                    ReadCurrent(ifs);
                    this.filename = Path.GetFileName(fname);
                    UpdateTitleBar();
                    SetHasntChanged();
                }
            }
            catch (Exception e)
            {
                Error(e);
                return false;
            }
            return true;
        }

        private void BlankDocument()
        {
            filename = null;
            UpdateTitleBar();

            SyntaxTree theTree = SyntaxTree.GetDefaultTree();
            theTree.Dirty = false;
            syntaxTreeViewer.SetCurrentTree(theTree);
            UpdateGUI();
        }

        private void WriteCurrent(Stream os)
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(os, syntaxTreeViewer.GetCurrentTree());
        }

        private void ReadCurrent(Stream os)
        {
            IFormatter formatter = new BinaryFormatter();
            SyntaxTree tree = (SyntaxTree)formatter.Deserialize(os);
            syntaxTreeViewer.SetCurrentTree(tree);
            UpdateGUI();
        }

        private bool DoSave()
        {
            if (filename == null)
                return DoSaveAs();
            else
                try
                {
                    using (Stream os = File.OpenWrite(filename))
                    {
                        if (os != null)
                        {
                            WriteCurrent(os);
                            SetHasntChanged();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Error(ex);
                    return false;
                }
            return true;
        }

        private void saveStripMenuItem_Click(object sender, EventArgs e)
        {
            DoSave();
        }

        private bool OkToDiscard()
        {
            if (Changed())
            {
                DialogResult dlgres = MessageBox.Show(this,
                    string.Format("The file \"{0}\" has been changed. " +
                    "Do you want to save the changes?",
                    (filename == null) ? "untitled" : DisplayFileName(filename)), APP_NAME,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);
                switch (dlgres)
                {
                    case DialogResult.Yes:
                        return DoSave();
                    case DialogResult.Cancel:
                        return false;
                    case DialogResult.No:
                    default:
                        return true;
                }
            }
            else
                return true;

        }

        private void openStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!OkToDiscard())
                return;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = GetStandardFilter();
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        using (Stream ifs = ofd.OpenFile())
                        {
                            if (ifs != null)
                            {
                                ReadCurrent(ifs);
                                this.filename = ofd.FileName;
                                UpdateTitleBar();
                                SetHasntChanged();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Error(ex);
                    }

                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoSaveAs();
        }

        private void newStripMenuItem_Click(object sender, EventArgs e)
        {
            if (OkToDiscard())
                BlankDocument();
        }


        private void syntaxTreeViewer_TreeStructureChanged(object sender, EventArgs e)
        {
            BracketingTextBox.Text = syntaxTreeViewer.GetBracketing();
        }

        private void syntaxTreeViewer_TreeItemSelected(object sender, EventArgs e)
        {
            // UpdateFontToolBar();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoExport();
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!OkToDiscard())
                e.Cancel = true;
        }


        private void SetMenuItemToColor(ToolStripMenuItem mi, Color color)
        {
            if (mi.Image != null)
            {
                Image old = mi.Image;
                mi.Image = null;
                old.Dispose();
            }
            Bitmap image = new Bitmap(1, 1, PixelFormat.Format64bppPArgb);
            using (Graphics g = Graphics.FromImage(image))
            {
                g.Clear(color);
            }
            mi.ImageScaling = ToolStripItemImageScaling.SizeToFit;
            mi.Image = image;
        }

        private void UpdateGUI()
        {
            TreeOptions op = syntaxTreeViewer.GetCurrentOptions();
            antialiasToolStripMenuItem.Checked = op.antialias;

            renderAlignedToolStripMenuItem.Checked = op.rendermode == RenderMode.OriginalAlign;
            renderStandardToolStripMenuItem.Checked = op.rendermode == RenderMode.Original;
            renderWalkerToolStripMenuItem.Checked = op.rendermode == RenderMode.Walker;

            topToolStripMenuItem.Checked = op.treealignment == TreeAlignmentEnum.Top;
            middleToolStripMenuItem.Checked = op.treealignment == TreeAlignmentEnum.Middle;
            bottomToolStripMenuItem.Checked = op.treealignment == TreeAlignmentEnum.Bottom;

            SetMenuItemToColor(backgroundColorToolStripMenuItem, op.backgroundcolor);
            SetMenuItemToColor(highlightColorToolStripMenuItem, op.highlightcolor);
        }



        private void topToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxTreeViewer.UndoAdd();
            syntaxTreeViewer.GetCurrentOptions().treealignment = TreeAlignmentEnum.Top;
            SetHasChanged();
            syntaxTreeViewer.UpdateEverything(true);
            UpdateGUI();
        }

        private void middleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxTreeViewer.UndoAdd();
            syntaxTreeViewer.GetCurrentOptions().treealignment = TreeAlignmentEnum.Middle;
            SetHasChanged();
            syntaxTreeViewer.UpdateEverything(true);
            UpdateGUI();

        }

        private void bottomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxTreeViewer.UndoAdd();
            syntaxTreeViewer.GetCurrentOptions().treealignment = TreeAlignmentEnum.Bottom;
            SetHasChanged();
            syntaxTreeViewer.UpdateEverything(true);
            UpdateGUI();
        }


        public static Font GeneralPickFont(IWin32Window who, Font what)
        {

            using (FontDialog fd = new FontDialog())
            {
                fd.AllowSimulations = true;
                fd.ScriptsOnly = false;
                fd.ShowColor = false;
                fd.FontMustExist = true;
                fd.ShowEffects = true;
                fd.Font = what;
                if (fd.ShowDialog(who) == DialogResult.OK)
                {
                    return fd.Font;
                }
                else
                    return null;
            }
        }


        public static FontAndColor GeneralPickFontAndColor(IWin32Window who, FontAndColor what)
        {
            using (FontDialog fd = new FontDialog())
            {
                fd.AllowSimulations = true;
                fd.ScriptsOnly = false;
                fd.ShowColor = true;
                fd.FontMustExist = true;
                fd.ShowEffects = true;
                fd.Font = what.Font;
                fd.Color = what.Color;
                if (fd.ShowDialog(who) == DialogResult.OK)
                {
                    return new FontAndColor(fd.Font, fd.Color);
                }
                else
                    return FontAndColor.Empty;
            }
        }

        public static Color GeneralPickColor(IWin32Window who, Color what)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                cd.AllowFullOpen = false;
                cd.Color = what;
                cd.SolidColorOnly = true;
                if (cd.ShowDialog(who) == DialogResult.OK)
                {
                    return cd.Color;
                }
                else
                    return Color.Empty;
            }
        }

        private Color PickColor(Color input)
        {
            return GeneralPickColor(this, input);
        }
        private FontAndColor PickFontAndColor(FontAndColor input)
        {
            return GeneralPickFontAndColor(this, input);
        }

        private void backgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeOptions to = syntaxTreeViewer.GetCurrentOptions();
            Color color = PickColor(to.backgroundcolor);
            if (color != Color.Empty)
            {
                syntaxTreeViewer.UndoAdd();
                to.backgroundcolor = color;
                SetHasChanged();
                syntaxTreeViewer.UpdateEverything(true);
                UpdateGUI();
                syntaxTreeViewer.FreeAllCache();
            }
        }

        private void labelColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeOptions to = syntaxTreeViewer.GetCurrentOptions();
            FontAndColor color = PickFontAndColor(to.labelfont);
            if (color != FontAndColor.Empty)
            {
                syntaxTreeViewer.UndoAdd();
                to.labelfont = color;
                SetHasChanged();
                syntaxTreeViewer.UpdateEverything(true);
                UpdateGUI();
            }

        }

        private void lexicalItemColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeOptions to = syntaxTreeViewer.GetCurrentOptions();
            FontAndColor color = PickFontAndColor(to.lexicalfont);
            if (color != FontAndColor.Empty)
            {
                syntaxTreeViewer.UndoAdd();
                to.lexicalfont = color;
                SetHasChanged();
                syntaxTreeViewer.UpdateEverything(true);
                UpdateGUI();
            }

        }
        private void lineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (LineDialog ld = new LineDialog())
            {
                TreeOptions to = syntaxTreeViewer.GetCurrentOptions();
                ld.Style = to.linestyle;
                ld.ShapeMode = LineDialog.LineDialogMode.Line;

                if (ld.ShowDialog(this) == DialogResult.OK)
                {
                    syntaxTreeViewer.UndoAdd();
                    to.linestyle = ld.Style;
                    SetHasChanged();
                    syntaxTreeViewer.ReleaseGDIResources();
                    syntaxTreeViewer.UpdateEverything(true);
                }
            }
        }

        private void spacingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SpacingDialog sd = new SpacingDialog())
            {
                TreeOptions to = syntaxTreeViewer.GetCurrentOptions();
                sd.MarginHorizontal = to.marginhorizontal;
                sd.MarginVertical = to.marginvertical;
                sd.PaddingHorizontal = to.paddinghorizontal;
                sd.PaddingVertical = to.paddingvertical;
                sd.TraceHorizontal = to.tracehorizontal;
                sd.TraceVertical = to.tracevertical;

                if (sd.ShowDialog(this) == DialogResult.OK)
                {
                    syntaxTreeViewer.UndoAdd();
                    to.marginhorizontal = sd.MarginHorizontal;
                    to.marginvertical = sd.MarginVertical;
                    to.paddinghorizontal = sd.PaddingHorizontal;
                    to.paddingvertical = sd.PaddingVertical;
                    to.tracehorizontal = sd.TraceHorizontal;
                    to.tracevertical = sd.TraceVertical;
                    SetHasChanged();
                    syntaxTreeViewer.FreeAllCache();
                    syntaxTreeViewer.UpdateEverything(true);
                    UpdateGUI();
                }
            }
        }

        private void highlightColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeOptions to = syntaxTreeViewer.GetCurrentOptions();
            Color color = PickColor(to.highlightcolor);
            if (color != Color.Empty)
            {
                syntaxTreeViewer.UndoAdd();
                to.highlightcolor = color;
                SetHasChanged();
                syntaxTreeViewer.FreeAllCache();
                syntaxTreeViewer.ReleaseGDIResources();
                syntaxTreeViewer.UpdateEverything(true);
                UpdateGUI();
            }

        }
        private void antialiasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxTreeViewer.UndoAdd();
            TreeOptions op = syntaxTreeViewer.GetCurrentOptions();
            op.antialias = !op.antialias;
            SetHasChanged();
            syntaxTreeViewer.FreeAllCache();
            syntaxTreeViewer.UpdateEverything(true);
            UpdateGUI();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxTreeViewer.SelectAll();
            syntaxTreeViewer.UpdateEverything(true);
        }

        private void copyImagetoolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            try
            {
                using (Bitmap bmp = syntaxTreeViewer.GetBitmap())
                    Clipboard.SetImage(bmp);
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxTreeViewer.UndoAdd();
            syntaxTreeViewer.DoCutBranch(true);
            syntaxTreeViewer.UpdateEverything(false);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxTreeViewer.DoCopyBranch(true);
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxTreeViewer.UndoAdd();
            syntaxTreeViewer.DoPasteBranch(true);
            syntaxTreeViewer.UpdateEverything(false);
        }

        private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            undoStripMenuItem.Enabled = syntaxTreeViewer.UndoUndo(false);
            redoStripMenuItem.Enabled = syntaxTreeViewer.UndoRedo(false);

            cutToolStripMenuItem.Enabled = syntaxTreeViewer.DoCutBranch(false);
            copyToolStripMenuItem.Enabled = syntaxTreeViewer.DoCopyBranch(false);
            pasteToolStripMenuItem.Enabled = syntaxTreeViewer.DoPasteBranch(false);
        }


        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();


            /*
            using (Graphics g = CreateGraphics())
            {
                syntaxTreeViewer.DoPaint(new GraphicsPassThrough(g), ClientSize);
            }
            */
            //syntaxTreeViewer.UpdateEverything(false);


            /*
             * syntaxTreeViewer.UpdateEverything(false);
             * syntaxTreeViewer.Refresh(); //synchronous redraw
             * */


            //syntaxTreeViewer.UndoAdd();
            syntaxTreeViewer.UpdateEverything(false);


            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        private void renderStandardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxTreeViewer.UndoAdd();
            TreeOptions op = syntaxTreeViewer.GetCurrentOptions();
            op.rendermode = RenderMode.Original;
            SetHasChanged();
            syntaxTreeViewer.UpdateEverything(false);
            UpdateGUI();
        }

        private void renderAlignedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxTreeViewer.UndoAdd();
            TreeOptions op = syntaxTreeViewer.GetCurrentOptions();
            op.rendermode = RenderMode.OriginalAlign;
            SetHasChanged();
            syntaxTreeViewer.UpdateEverything(false);
            UpdateGUI();

        }

        private void renderWalkerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxTreeViewer.UndoAdd();
            TreeOptions op = syntaxTreeViewer.GetCurrentOptions();
            op.rendermode = RenderMode.Walker;
            SetHasChanged();
            syntaxTreeViewer.UpdateEverything(false);
            UpdateGUI();

        }

        private void zoomToolStripComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (zoomToolStripComboBox.SelectedIndex < 0)
                return;
            String text=(String)zoomToolStripComboBox.Items[zoomToolStripComboBox.SelectedIndex];
            zoomToolStripComboBox_UpdateManager(text);
        }

        private void zoomToolStripComboBox_UpdateManager(String newvalue)
        {
            String text=newvalue;
            uint percent = 0;

            if (text.EndsWith("%"))
                text = text.Substring(0, text.Length - 1);

            try
            {
                percent = Convert.ToUInt32(text);
            }
            catch (Exception)
            {
                percent = 0;
            }
            if (percent < 2 || percent > 1000)
                percent = 0;


            if (percent != 0)
            {
                syntaxTreeViewer.SetZoomFactor((float)percent / 100.0f);
                syntaxTreeViewer.ReleaseGDIResources();
                syntaxTreeViewer.UpdateEverything(true);
            }

        }

        private void zoomToolStripComboBox_TextChanged(object sender, EventArgs e)
        {
            zoomToolStripComboBox_UpdateManager(zoomToolStripComboBox.Text);
        }

        private void zoomToolStripComboBox_Validated(object sender, EventArgs e)
        {
            String text = zoomToolStripComboBox.Text;
            if (text.EndsWith("%"))
                text = text.Substring(0, text.Length - 1);
            uint percent;
            try
            {
                percent = Convert.ToUInt32(text);
            }
            catch (Exception)
            {
                percent = 100;
            }
            if (percent < 2 || percent > 1000)
                percent = 100;
            String newtext = Convert.ToString(percent) + "%";
            zoomToolStripComboBox.Text = newtext;
        }

        private static void CheckButton(ToolStripButton button, Ternary t)
        {
            switch (t)
            {
                case Ternary.Yes:
                    button.CheckState = CheckState.Checked;
                    break;
                case Ternary.No:
                    button.CheckState = CheckState.Unchecked;
                    break;
                default:
                case Ternary.Indeterminate:
                    button.CheckState = CheckState.Indeterminate;
                    break;
            }
        }


        private void InitFontToolBar()
        {
            foreach (String n in Hacks.GetFontNames())
                fontToolStripComboBox.Items.Add(n);
            fontColorToolStripComboBox.ComboBox.DataSource = Hacks.GetColorBrushes();
            fontColorToolStripComboBox.ComboBox.DrawItem += new DrawItemEventHandler(fontColorToolStripComboBox_DrawItem);
            fontColorToolStripComboBox.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;

            fontToolStripComboBox.ComboBox.SelectionChangeCommitted += new EventHandler(fontToolStripComboBox_SelectionChangeCommitted);
            fontSizeToolStripComboBox.ComboBox.SelectionChangeCommitted += new EventHandler(fontSizeToolStripComboBox_SelectionChangeCommitted);
            fontColorToolStripComboBox.ComboBox.SelectionChangeCommitted += new EventHandler(fontColorToolStripComboBox_SelectionChangeCommitted);
            zoomToolStripComboBox.ComboBox.SelectionChangeCommitted+=new EventHandler(zoomToolStripComboBox_SelectionChangeCommitted);
        }
        private void UpdateFontToolBar()
        {
            List<Elem> selected = syntaxTreeViewer.GetSelection();
            //mainToolStrip.SuspendLayout();

            if (selected == null || selected.Count == 0)
            {
                mainToolStrip.Enabled = false;

                fontToolStripComboBox.Text = String.Empty;
                fontSizeToolStripComboBox.Text = String.Empty;
                fontColorToolStripComboBox.SelectedItem = Brushes.Black;
                boldToolStripButton.Checked = false;
                italicToolStripButton.Checked = false;
                underlineToolStripButton.Checked = false;
                superscriptToolStripButton.Checked = false;
                subscriptToolStripButton.Checked = false;
                strikeoutToolStripButton.Checked = false;

                return;
            }

            SelectionProxy sp = new SelectionProxy(syntaxTreeViewer, nodeEditor, selected);
            FormatChanges fc = sp.SelectionSummary();

            fontToolStripComboBox.SelectedIndex = -1;
            fontToolStripComboBox.Text = fc.fontfamily;
            if (fc.fontsize == 0f)
                fontSizeToolStripComboBox.Text = String.Empty;
            else
                fontSizeToolStripComboBox.Text = Convert.ToString(fc.fontsize);


            if (fc.color == Color.Empty)
                fontColorToolStripComboBox.SelectedIndex = 0;
            else
                fontColorToolStripComboBox.SelectedIndex = IndexOfFontColor(fc.color);

            CheckButton(boldToolStripButton, fc.bold);
            CheckButton(italicToolStripButton, fc.italic);
            CheckButton(underlineToolStripButton, fc.underline);
            CheckButton(superscriptToolStripButton, fc.superscript);
            CheckButton(subscriptToolStripButton, fc.subscript);
            CheckButton(strikeoutToolStripButton, fc.strikeout);

            mainToolStrip.Enabled = true;
            //mainToolStrip.ResumeLayout(false);
        }

        private int IndexOfFontColor(Color c)
        {
            for (int i = 0; i < fontColorToolStripComboBox.Items.Count; i++)
                if (((SolidBrush)fontColorToolStripComboBox.Items[i]).Color == c)
                    return i;
            return 0;
        }

        private void boldToolStripButton_Click(object sender, EventArgs e)
        {
            applyBold();
        }

        private void applyBold()
        {
            syntaxTreeViewer.UndoAdd();
            SelectionProxy sp = new SelectionProxy(syntaxTreeViewer, nodeEditor,
                syntaxTreeViewer.GetSelection());

            if (sp.Bold == Ternary.No)
                sp.Bold = Ternary.Yes;
            else
                sp.Bold = Ternary.No;

            syntaxTreeViewer.UpdateEverything(true);
            UpdateFontToolBar();

        }

        private void italicToolStripButton_Click(object sender, EventArgs e)
        {
            applyItalic();
        }

        private void applyItalic()
        {
            syntaxTreeViewer.UndoAdd();
            SelectionProxy sp = new SelectionProxy(syntaxTreeViewer, nodeEditor,
                syntaxTreeViewer.GetSelection());

            if (sp.Italic == Ternary.No)
                sp.Italic = Ternary.Yes;
            else
                sp.Italic = Ternary.No;

            syntaxTreeViewer.UpdateEverything(true);
            UpdateFontToolBar();

        }

        private void underlineToolStripButton_Click(object sender, EventArgs e)
        {
            applyUnderline();
        }

        private void applyUnderline()
        {
            syntaxTreeViewer.UndoAdd();
            SelectionProxy sp = new SelectionProxy(syntaxTreeViewer, nodeEditor,
                syntaxTreeViewer.GetSelection());

            if (sp.Underline == Ternary.No)
                sp.Underline = Ternary.Yes;
            else
                sp.Underline = Ternary.No;

            syntaxTreeViewer.UpdateEverything(true);
            UpdateFontToolBar();

        }

        private void subscriptToolStripButton_Click(object sender, EventArgs e)
        {
            applySubscript();
        }

        private void applySubscript()
        {
            syntaxTreeViewer.UndoAdd();
            SelectionProxy sp = new SelectionProxy(syntaxTreeViewer, nodeEditor,
                syntaxTreeViewer.GetSelection());

            if (sp.Subscript == Ternary.No)
                sp.Subscript = Ternary.Yes;
            else
                sp.Subscript = Ternary.No;

            syntaxTreeViewer.UpdateEverything(true);
            UpdateFontToolBar();

        }

        private void superscriptToolStripButton_Click(object sender, EventArgs e)
        {
            applySuperscript();
        }

        private void applySuperscript()
        {
            syntaxTreeViewer.UndoAdd();
            SelectionProxy sp = new SelectionProxy(syntaxTreeViewer, nodeEditor,
                syntaxTreeViewer.GetSelection());

            if (sp.Superscript == Ternary.No)
                sp.Superscript = Ternary.Yes;
            else
                sp.Superscript = Ternary.No;

            syntaxTreeViewer.UpdateEverything(true);
            UpdateFontToolBar();

        }

        private void strikeoutToolStripButton_Click(object sender, EventArgs e)
        {
            applyStrikeout();
        }

        private void applyStrikeout()
        {
            syntaxTreeViewer.UndoAdd();
            SelectionProxy sp = new SelectionProxy(syntaxTreeViewer, nodeEditor,
                syntaxTreeViewer.GetSelection());

            if (sp.Strikeout == Ternary.No)
                sp.Strikeout = Ternary.Yes;
            else
                sp.Strikeout = Ternary.No;

            syntaxTreeViewer.UpdateEverything(true);
            UpdateFontToolBar();
        }

        private void fontToolStripComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (fontToolStripComboBox.SelectedIndex >= 0)
                fontToolStripComboBox.Text = (String)fontToolStripComboBox.Items[fontToolStripComboBox.SelectedIndex];
            fontToolStripComboBox_Validated(sender, e);
        }

        private void fontToolStripComboBox_Validated(object sender, EventArgs e)
        {
            syntaxTreeViewer.UndoAdd();
            SelectionProxy sp = new SelectionProxy(syntaxTreeViewer, nodeEditor, syntaxTreeViewer.GetSelection());

            sp.FontName = fontToolStripComboBox.Text;
            syntaxTreeViewer.UpdateEverything(true);
        }

        private void fontSizeToolStripComboBox_Validated(object sender, EventArgs e)
        {
            syntaxTreeViewer.UndoAdd();
            fontSizeToolStripComboBox.Text = Convert.ToString(StringToFontSize(fontSizeToolStripComboBox.Text));
            SelectionProxy sp = new SelectionProxy(syntaxTreeViewer, nodeEditor, syntaxTreeViewer.GetSelection());
            sp.FontSize = StringToFontSize(fontSizeToolStripComboBox.Text);
            syntaxTreeViewer.UpdateEverything(true);
        }

        private static float StringToFontSize(String text)
        {
            try
            {
                float f = Convert.ToSingle(text);
                if (f < 1 || f > 1638)
                    return 8;
                return f;
            }
            catch (Exception)
            {
                return 8;
            }
        }

        private void fontSizeToolStripComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (fontSizeToolStripComboBox.SelectedIndex >= 0)
                fontSizeToolStripComboBox.Text = (String)fontSizeToolStripComboBox.Items[fontSizeToolStripComboBox.SelectedIndex];
            fontSizeToolStripComboBox_Validated(sender, e);
        }

        private void fontColorToolStripComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            using (Graphics g = e.Graphics)
            {
                g.FillRectangle((SolidBrush)fontColorToolStripComboBox.Items[e.Index], e.Bounds);
            }
        }

        private void fontColorToolStripComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            syntaxTreeViewer.UndoAdd();
            SelectionProxy sp = new SelectionProxy(syntaxTreeViewer, nodeEditor, syntaxTreeViewer.GetSelection());
            sp.Color = ((SolidBrush)fontColorToolStripComboBox.SelectedItem).Color;
            syntaxTreeViewer.UpdateEverything(true);
        }

        private void nodeEditor_SelectionChanged(object sender, EventArgs e)
        {
            UpdateFontToolBar();
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!OkToDiscard())
                return;
            using (InputBox ib = new InputBox())
            {
                ib.Text = "Import from bracketing";
                ib.Prompt = "Paste the textual representation of the tree in the field below.";
                if (ib.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        SyntaxTree st = SyntaxTree.FromBracketing(ib.Input);
                        syntaxTreeViewer.SetCurrentTree(st);
                    }
                    catch (TreeException error)
                    {
                        Error(error);
                        return;
                    }
                    SetHasChanged();
                    this.filename = null;
                    UpdateTitleBar();
                    UpdateGUI();
                    syntaxTreeViewer.UndoAdd();
                }
            }
        }

        private bool DoFont(bool forReal)
        {
            bool done = false;
            if (syntaxTreeViewer.GetSelection().Count > 0)
            {
                if (forReal)
                {
                    SelectionProxy sp = new SelectionProxy(syntaxTreeViewer, nodeEditor,
                        syntaxTreeViewer.GetSelection());
                    Font theFont = sp.Font;

                    theFont = GeneralPickFont(this, theFont);
                    if (theFont != null)
                    {
                        syntaxTreeViewer.UndoAdd();
                        sp.Font = theFont;
                        UpdateFontToolBar();
                        syntaxTreeViewer.UpdateEverything(true);
                    }

                }
                done = true;
            }
            return done;
        }
        private bool DoDecoration(bool forReal)
        {
            return syntaxTreeViewer.DoDecoration(forReal);
        }
        private bool DoColor(bool forReal)
        {
            bool done = false;
            if (syntaxTreeViewer.GetSelection().Count > 0)
            {
                if (forReal)
                {
                    SelectionProxy sp = new SelectionProxy(syntaxTreeViewer, nodeEditor,
                        syntaxTreeViewer.GetSelection());
                    Color theColor = sp.Color;

                    theColor = GeneralPickColor(this, theColor);
                    if (theColor != Color.Empty)
                    {
                        syntaxTreeViewer.UndoAdd();
                        sp.Color = theColor;
                        UpdateFontToolBar();
                        syntaxTreeViewer.UpdateEverything(true);
                    }

                }
                done = true;
            }
            return done;
        }

        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoFont(true);
        }

        private void nodeToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            deleteNodeToolStripMenuItem.Enabled = syntaxTreeViewer.DoDeleteNode(false);
            insertChildNodeToolStripMenuItem.Enabled = syntaxTreeViewer.DoAddChild(false);
            insertParentNodeToolStripMenuItem.Enabled = syntaxTreeViewer.DoAddParent(false);

            fontToolStripMenuItem.Enabled = DoFont(false);
            colorToolStripMenuItem.Enabled = DoColor(false);
            decorationToolStripMenuItem.Enabled = DoDecoration(false);
        }

        private void colorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoColor(true);
        }

        private void decorationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoDecoration(true);
        }

        private void undoStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxTreeViewer.UndoUndo(true);
        }

        private void redoStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxTreeViewer.UndoRedo(true);
        }

        private void traceStyleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (LineDialog ld = new LineDialog())
            {
                TreeOptions to = syntaxTreeViewer.GetCurrentOptions();
                ld.Style = to.tracestyle;
                ld.TraceStyle = to.tracemode;
                ld.ShapeMode = LineDialog.LineDialogMode.Trace;

                if (ld.ShowDialog(this) == DialogResult.OK)
                {
                    syntaxTreeViewer.UndoAdd();
                    to.tracestyle = ld.Style;
                    to.tracemode = ld.TraceStyle;
                    SetHasChanged();
                    syntaxTreeViewer.UpdateTraces();
                    syntaxTreeViewer.UpdateEverything(true);
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox ab = new AboutBox();
            ab.ShowDialog(this);
        }

        private void insertChildNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxTreeViewer.DoAddChild(true);
        }

        private void insertParentNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxTreeViewer.DoAddParent(true);
        }

        private void deleteNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxTreeViewer.DoDeleteNode(true);
        }

        private void licenseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string licenseText = AboutBox.AssemblyCopyright + "\n\n" +
                "Permission is hereby granted, free of charge, to any person\n"+
                "obtaining a copy of this software and associated documentation\n"+
                "files (the \"Software\"), to deal in the Software without restriction,\n"+
                "including without limitation the rights to use, copy, modify, merge,\n"+
                "publish, distribute, sublicense, and/or sell copies of the Software,\n"+
                "and to permit persons to whom the Software is furnished to do so,\n"+
                "subject to the following conditions:\n"+
                "\n" +
                "The above copyright notice and this permission notice shall be\n"+
                "included in all copies or substantial portions of the Software.\n" +
                "\n" +
                "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT\n"+
                "WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,\n"+
                "INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF\n"+
                "MERCHANTABILITY, FITNESS FOR A PARTICULAR\n"+
                "PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE\n"+
                "COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES\n"+
                "OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,\n"+
                "TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN\n"+
                "CONNECTION WITH THE SOFTWARE OR THE USE OR\n"+
                "OTHER DEALINGS IN THE SOTWARE.";
            MessageBox.Show(this, licenseText, APP_NAME, MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void creditsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string creditText = "This program is the work of a group of people. We'd like " +
                              "to thank everyone who has contributed.\n\n" +
                              "Programmer:\nJeff Epstein\n" +
                              "\n" +
                              "Linguistic consultant:\nEdmund O'Neill\n" +
                              "\n" +
                              "Artist:\n"+
                              "Ian Schlaepfer\n"+
                              "\n" +
                              "Additional thanks to:\nZofia Stankiewicz";

            using (TextDialogBox tdb = new TextDialogBox())
            {
                tdb.Text = "Credits";

                tdb.Content = creditText.Replace("\n", "\r\n");

                tdb.ShowDialog(this);
            }
        }


        private void feedbackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FeedbackForm ff = new FeedbackForm();
            ff.ShowDialog(this);
        }

    }

}