using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Tree
{
    static class Program
    {
        [STAThread]
        static void Main(string[]args)
        {
            //Application.EnableVisualStyles();
            ToolStripManager.VisualStylesEnabled = false;
            ToolStripManager.RenderMode = ToolStripManagerRenderMode.System;
            Application.SetCompatibleTextRenderingDefault(false);

            MainWindow main = new MainWindow();
            if (args.Length > 0)
                main.LoadFile(args[0]);
            Application.Run(main);
        }
    }
}