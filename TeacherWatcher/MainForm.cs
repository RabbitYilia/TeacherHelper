using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace TeacherHelper
{
    public partial class MainForm : Form
    {
        [DllImport("ProcCmdLine32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetProcCmdLine")]
        public extern static bool GetProcCmdLine32(uint nProcId, StringBuilder sb, uint dwSizeBuf);

        [DllImport("ProcCmdLine64.dll", CharSet = CharSet.Unicode, EntryPoint = "GetProcCmdLine")]
        public extern static bool GetProcCmdLine64(uint nProcId, StringBuilder sb, uint dwSizeBuf);
        Dictionary<string, string> dic = new Dictionary<string, string>();
        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(WatchProcess));
            thread.Start();
        }
        private void WatchProcess()
        {
            string path = @"C:\Files\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            while (true)
            {
                Process[] ps = Process.GetProcesses();
                foreach (Process p in ps)
                {
                    var cmdline = GetCommandLineOfProcess(p);
                    foreach (string attr in cmdline.Split('"'))
                    {
                        var thisattr = attr.ToLower();
                        if (thisattr.Contains(".pdf") || thisattr.Contains(".doc") || thisattr.Contains(".docx") || thisattr.Contains(".ppt") || thisattr.Contains(".pptx"))
                        {
                            Console.WriteLine(attr);
                            if (!dic.ContainsKey(attr))
                            {
                                System.IO.File.Copy(attr, path + Path.GetFileName(attr), true);
                                dic.Add(attr, "1");
                            }

                        }
                    }
                }
                System.Threading.Thread.Sleep(10000);
            }
        }
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose();
            this.Close();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            this.Hide();
        }
        public static string GetCommandLineOfProcess(Process proc)
        {
            // max size of a command line is USHORT/sizeof(WCHAR), so we are going
            // just allocate max USHORT for sanity's sake.
            var sb = new StringBuilder(0xFFFF);
            switch (IntPtr.Size)
            {
                case 4: GetProcCmdLine32((uint)proc.Id, sb, (uint)sb.Capacity); break;
                case 8: GetProcCmdLine64((uint)proc.Id, sb, (uint)sb.Capacity); break;
            }
            return sb.ToString();
        }
    }
}
