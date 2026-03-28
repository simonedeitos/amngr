using System;
using System.Windows.Forms;
using AirManager.Services.Database;
using AirManager.Services.Licensing;
using AirManager.Forms;

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

            // ✅ VERIFICA LICENZA
            try
            {
                Console.WriteLine("[AirManager] Verifica licenza...");

                if (!LicenseManager.IsLicenseValid())
                {
                    Console.WriteLine("[AirManager] ⚠️ Licenza non valida, apertura form di attivazione...");
                    using (var licenseForm = new LicenseForm())
                    {
                        DialogResult result = licenseForm.ShowDialog();
                        if (result != DialogResult.OK)
                        {
                            Console.WriteLine("[AirManager] ❌ Attivazione licenza annullata. Chiusura applicazione.");
                            return;
                        }
                    }
                    Console.WriteLine("[AirManager] ✅ Licenza attivata con successo");
                }
                else
                {
                    Console.WriteLine("[AirManager] ✅ Licenza valida");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AirManager] ⚠️ Errore verifica licenza: {ex.Message}");
                MessageBox.Show(
                    $"⚠️ Errore durante la verifica della licenza:\n\n{ex.Message}\n\nL'applicazione verrà chiusa.",
                    "Errore Licenza",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
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