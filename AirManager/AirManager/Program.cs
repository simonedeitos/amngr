using System;
using System.Windows.Forms;
using AirManager.Services.Database;

namespace AirManager
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // ✅ CONFIGURA APPLICATION
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ✅ INIZIALIZZA DATABASE MANAGER
            try
            {
                Console.WriteLine("[AirManager] Inizializzazione Database Manager...");
                DbcManager.Initialize();
                Console.WriteLine("[AirManager] ✅ Database Manager inizializzato");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Errore inizializzazione Database Manager:\n\n{ex.Message}\n\nL'applicazione verrà chiusa.",
                    "Errore Critico",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // ✅ CARICA MAINFORM
            try
            {
                Console.WriteLine("[AirManager] Avvio MainForm...");
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Errore durante l'esecuzione:\n\n{ex.Message}",
                    "Errore",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}