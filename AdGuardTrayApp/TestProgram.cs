using System;
using System.Windows.Forms;

namespace AdGuardTrayApp
{
    internal static class TestProgram
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Console.WriteLine("Starte Test-Anwendung...");
            
            var testForm = new TestForm();
            Application.Run(testForm);
        }
    }
}
