using System;
using System.Drawing;
using System.Windows.Forms;

namespace AdGuardTrayApp
{
    public class TestForm : Form
    {
        private NotifyIcon trayIcon;

        public TestForm()
        {
            // Form ausblenden
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = false;

            // Tray Icon erstellen
            trayIcon = new NotifyIcon()
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "AdGuard Tray Test"
            };

            trayIcon.Click += (s, e) => MessageBox.Show("Test funktioniert!");

            Console.WriteLine("TestForm erstellt und Tray Icon gesetzt");
        }

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                trayIcon?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
