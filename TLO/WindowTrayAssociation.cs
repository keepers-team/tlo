using System.Windows.Forms;

namespace TLO
{
    public class WindowTrayAssociation
    {
        private readonly Form _form;
        private bool _iconAlwaysShown;

        public WindowTrayAssociation(Form form)
        {
            _form = form;

            Properties.Settings.Default.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ShowInTray")
                {
                    SyncSettings();
                }
            };

            TrayObject.OnClick += (sender, args) =>
            {
                if ((args.Button & MouseButtons.Left) == 0)
                {
                    return;
                }

                if (!_form.Visible)
                {
                    _form.ShowInTaskbar = true;
                    _form.Visible = true;
                }
                else if (_form.WindowState == FormWindowState.Normal)
                {
                    if (Properties.Settings.Default.HideToTray)
                    {
                        _form.WindowState = FormWindowState.Minimized;
                        _form.ShowInTaskbar = false;
                        TrayObject.TrayIcon.Visible = true;
                    }
                }
                else if (_form.WindowState == FormWindowState.Minimized)
                {
                    _form.WindowState = FormWindowState.Normal;
                    _form.ShowInTaskbar = true;
                }

                SyncSettings();
            };

            _form.Resize += (sender, args) =>
            {
                if (_form.WindowState == FormWindowState.Minimized && Properties.Settings.Default.HideToTray)
                {
                    _form.ShowInTaskbar = false;
                    TrayObject.TrayIcon.Visible = true;
                }
            };

            _form.FormClosing += (sender, args) =>
            {
                if (Properties.Settings.Default.CloseToTray && args.CloseReason == CloseReason.UserClosing)
                {
                    _form.Hide();
                    _form.ShowInTaskbar = false;
                    TrayObject.TrayIcon.Visible = true;
                    args.Cancel = true;
                }
                else
                {
                    TrayObject.TrayIcon.Visible = false;
                }
            };
        }

        public void SyncSettings()
        {
            _iconAlwaysShown = Properties.Settings.Default.ShowInTray;
            TrayObject.TrayIcon.Visible = _iconAlwaysShown ||
                                          _form.WindowState == FormWindowState.Minimized &&
                                          (
                                              Properties.Settings.Default.HideToTray ||
                                              Properties.Settings.Default.CloseToTray
                                          );
        }
    }
}