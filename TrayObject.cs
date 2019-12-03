using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TLO.Forms;

namespace TLO
{
    public static class TrayObject
    {
        private static NotifyIcon _trayIcon;

        public static NotifyIcon TrayIcon
        {
            get
            {
                NotifyIcon CreateIconTrayIcon()
                {
                    var icon = new NotifyIcon
                    {
                        Icon = (Icon) new ComponentResourceManager(typeof(MainForm)).GetObject("$this.Icon"),
                        ContextMenu = new ContextMenu(new MenuItem[] { }),
                        Visible = false
                    };

                    icon.MouseClick += (sender, args) => OnClick?.Invoke(icon, args);

                    return icon;
                }

                return _trayIcon ??= CreateIconTrayIcon();
            }
        }

        public delegate void TrayEventHandler(object sender, MouseEventArgs args);

        public static event TrayEventHandler OnClick;
    }
}
