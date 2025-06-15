using System;
using System.Windows.Forms;

namespace AdGuardTrayApp
{
    // Application Context für Tray-only Anwendung
    public class TrayApplicationContext : ApplicationContext
    {
        private MainForm? mainForm;

        public TrayApplicationContext()
        {
            try
            {
                mainForm = new MainForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Start: {ex.Message}", "AdGuard Tray App", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                mainForm?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Verhindere mehrfache Instanzen
            using (var mutex = new System.Threading.Mutex(true, "AdGuardTrayApp_SingleInstance", out bool createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show("AdGuard Tray App läuft bereits!", "Bereits gestartet", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                
                // Erstelle das Hauptformular (unsichtbar)
                var context = new TrayApplicationContext();
                
                // Starte die Anwendung mit dem Application Context
                Application.Run(context);
            }
        }
    }
}
