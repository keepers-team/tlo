using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Windows.Forms;
using MihaZupan;
using TLO.Forms;

namespace TLO
{
    internal static class Program
    {   
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int FreeConsole();
        [STAThread]
        private static void Main()
        {
            AttachConsole(ATTACH_PARENT_PROCESS);
            if (Settings.Current.DontRunCopy)
            {
                var currentProcess = Process.GetCurrentProcess();
                foreach (var process in Process.GetProcessesByName(currentProcess.ProcessName))
                {
                    if (process.Id == currentProcess.Id) continue;
                    process.CloseMainWindow();
                    process.WaitForExit(2000);
                    process.Close();
                }
            }

            if (Settings.Current.UseProxy == true)
            {
                if (Settings.Current.SystemProxy == true)
                {
                    WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy();
                }
                else
                {
                    var proxy = Settings.Current.SelectedProxy;
                    if (proxy.Contains("http://"))
                    {
                        WebRequest.DefaultWebProxy = new WebProxy(proxy);
                    }
                    else
                    {
                        var uri = new Uri(proxy);
                        WebRequest.DefaultWebProxy = new HttpToSocks5Proxy(uri.Host, uri.Port);
                    }
                }
            }

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var mainForm = new MainForm();
                new WindowTrayAssociation(mainForm).SyncSettings();
                Application.ApplicationExit += (sender, args) => TrayObject.TrayIcon.Dispose();
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
            FreeConsole();
        }
        
        

        public static List<TreeNode> GetAllNodes(this TreeNode _self)
        {
            List<TreeNode> result = new List<TreeNode>();
            result.Add(_self);
            foreach (TreeNode child in _self.Nodes)
            {
                result.AddRange(child.GetAllNodes());
            }
            return result;
        }

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
    }
}