﻿using System;
using System.Diagnostics;
using System.Net;
using System.Windows.Forms;
using MihaZupan;
using TLO.Forms;

namespace TLO
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
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
        }
    }
}