using System.Drawing;

namespace AirManager.Themes
{
    /// <summary>
    /// Tema colori globale per AirManager
    /// Stile Dark Theme professionale
    /// </summary>
    public static class AppTheme
    {
        // ✅ COLORI BACKGROUND
        public static readonly Color BgDark = Color.FromArgb(30, 30, 30);           // Background scuro principale
        public static readonly Color BgLight = Color.FromArgb(45, 45, 48);          // Background pannelli
        public static readonly Color BgPanel = Color.FromArgb(40, 40, 40);          // Background cards/panels
        public static readonly Color BgInput = Color.FromArgb(50, 50, 50);          // Background input fields
        public static readonly Color BgHover = Color.FromArgb(60, 60, 60);          // Background hover state

        // ✅ COLORI TESTO
        public static readonly Color TextPrimary = Color.White;                     // Testo principale
        public static readonly Color TextSecondary = Color.FromArgb(180, 180, 180); // Testo secondario
        public static readonly Color TextDisabled = Color.FromArgb(100, 100, 100);  // Testo disabilitato
        public static readonly Color TextAccent = Color.FromArgb(0, 150, 136);      // Testo accent (teal)

        // ✅ COLORI ACCENT
        public static readonly Color AccentPrimary = Color.FromArgb(0, 150, 136);   // Teal (colore principale)
        public static readonly Color AccentSecondary = Color.FromArgb(0, 120, 215); // Blue
        public static readonly Color AccentTertiary = Color.FromArgb(138, 43, 226); // Purple

        // ✅ COLORI STATUS
        public static readonly Color Success = Color.FromArgb(40, 167, 69);         // Verde successo
        public static readonly Color Warning = Color.FromArgb(255, 140, 0);         // Arancione warning
        public static readonly Color Danger = Color.FromArgb(220, 53, 69);          // Rosso errore
        public static readonly Color Info = Color.FromArgb(23, 162, 184);           // Azzurro info

        // ✅ COLORI LED/INDICATORS
        public static readonly Color LEDGreen = Color.FromArgb(0, 255, 0);          // LED verde attivo
        public static readonly Color LEDRed = Color.FromArgb(255, 0, 0);            // LED rosso inattivo
        public static readonly Color LEDOrange = Color.FromArgb(255, 140, 0);       // LED arancione warning
        public static readonly Color LEDBlue = Color.FromArgb(0, 150, 255);         // LED blu info
        public static readonly Color LEDGray = Color.FromArgb(100, 100, 100);       // LED grigio disabilitato

        // ✅ COLORI BORDI
        public static readonly Color BorderLight = Color.FromArgb(70, 70, 70);      // Bordo chiaro
        public static readonly Color BorderDark = Color.FromArgb(20, 20, 20);       // Bordo scuro
        public static readonly Color BorderAccent = Color.FromArgb(0, 150, 136);    // Bordo accent

        // ✅ COLORI GRID/TABELLE
        public static readonly Color GridHeader = Color.FromArgb(50, 50, 50);       // Header DataGridView
        public static readonly Color GridRowEven = Color.FromArgb(40, 40, 40);      // Riga pari
        public static readonly Color GridRowOdd = Color.FromArgb(35, 35, 35);       // Riga dispari
        public static readonly Color GridSelection = Color.FromArgb(0, 120, 215);   // Selezione
        public static readonly Color GridBorder = Color.FromArgb(60, 60, 60);       // Bordo celle

        // ✅ COLORI MENU
        public static readonly Color MenuBackground = Color.FromArgb(45, 45, 48);   // Background menu
        public static readonly Color MenuHover = Color.FromArgb(62, 62, 66);        // Hover item menu
        public static readonly Color MenuSelected = Color.FromArgb(0, 122, 204);    // Item selezionato
        public static readonly Color MenuBorder = Color.FromArgb(62, 62, 66);       // Bordo menu

        // ✅ COLORI STATUS BAR
        public static readonly Color StatusBarBg = Color.FromArgb(45, 45, 48);      // Background status bar
        public static readonly Color StatusBarText = Color.White;                   // Testo status bar
        public static readonly Color StatusBarAccent = Color.FromArgb(255, 140, 0); // Accent status bar

        // ✅ COLORI WAVEFORM (per MusicEditorForm)
        public static readonly Color WaveformBg = Color.FromArgb(10, 10, 10);       // Background waveform
        public static readonly Color WaveformTop = Color.FromArgb(0, 255, 100);     // Onda superiore
        public static readonly Color WaveformBottom = Color.FromArgb(0, 200, 80);   // Onda inferiore
        public static readonly Color WaveformCenter = Color.FromArgb(80, 80, 80);   // Linea centrale

        // ✅ COLORI MARKER (per MusicEditorForm)
        public static readonly Color MarkerIN = Color.FromArgb(255, 50, 50);        // Marker IN (rosso)
        public static readonly Color MarkerINTRO = Color.FromArgb(255, 0, 255);     // Marker INTRO (magenta)
        public static readonly Color MarkerMIX = Color.FromArgb(255, 255, 0);       // Marker MIX (giallo)
        public static readonly Color MarkerOUT = Color.FromArgb(255, 140, 0);       // Marker OUT (arancione)
        public static readonly Color MarkerPlayhead = Color.White;                  // Playhead (bianco)

        // ✅ COLORI BOTTONI
        public static readonly Color ButtonPrimary = Color.FromArgb(0, 150, 136);   // Bottone principale
        public static readonly Color ButtonSecondary = Color.FromArgb(0, 120, 215); // Bottone secondario
        public static readonly Color ButtonSuccess = Color.FromArgb(40, 167, 69);   // Bottone salva/conferma
        public static readonly Color ButtonDanger = Color.FromArgb(220, 53, 69);    // Bottone elimina/annulla
        public static readonly Color ButtonWarning = Color.FromArgb(255, 140, 0);   // Bottone attenzione
        public static readonly Color ButtonInfo = Color.FromArgb(23, 162, 184);     // Bottone info
        public static readonly Color ButtonDisabled = Color.FromArgb(80, 80, 80);   // Bottone disabilitato

        // ✅ COLORI CARDS
        public static readonly Color CardBackground = Color.FromArgb(40, 40, 40);   // Background card
        public static readonly Color CardBorder = Color.FromArgb(60, 60, 60);       // Bordo card
        public static readonly Color CardAccent = Color.FromArgb(0, 150, 136);      // Barra accent card
        public static readonly Color CardHover = Color.FromArgb(50, 50, 50);        // Card in hover

        // ✅ COLORI ARCHIVI (per tipo contenuto)
        public static readonly Color ArchiveMusic = Color.FromArgb(0, 180, 0);      // Verde per musica
        public static readonly Color ArchiveClips = Color.FromArgb(255, 140, 0);    // Arancione per clips
        public static readonly Color ArchiveReport = Color.FromArgb(138, 43, 226);  // Purple per report

        // ✅ METODI UTILITY

        /// <summary>
        /// Schiarisce un colore di una percentuale
        /// </summary>
        public static Color Lighten(Color color, float percentage)
        {
            float factor = 1 + (percentage / 100f);
            int r = (int)(color.R * factor);
            int g = (int)(color.G * factor);
            int b = (int)(color.B * factor);

            r = r > 255 ? 255 : r;
            g = g > 255 ? 255 : g;
            b = b > 255 ? 255 : b;

            return Color.FromArgb(color.A, r, g, b);
        }

        /// <summary>
        /// Scurisce un colore di una percentuale
        /// </summary>
        public static Color Darken(Color color, float percentage)
        {
            float factor = 1 - (percentage / 100f);
            int r = (int)(color.R * factor);
            int g = (int)(color.G * factor);
            int b = (int)(color.B * factor);

            r = r < 0 ? 0 : r;
            g = g < 0 ? 0 : g;
            b = b < 0 ? 0 : b;

            return Color.FromArgb(color.A, r, g, b);
        }

        /// <summary>
        /// Ottiene un colore con trasparenza
        /// </summary>
        public static Color WithAlpha(Color color, int alpha)
        {
            alpha = alpha < 0 ? 0 : (alpha > 255 ? 255 : alpha);
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        /// <summary>
        /// Ottiene colore per tipo di archivio
        /// </summary>
        public static Color GetArchiveColor(string archiveType)
        {
            switch (archiveType?.ToLower())
            {
                case "musica":
                case "music":
                    return ArchiveMusic;
                case "clips":
                case "jingle":
                    return ArchiveClips;
                case "report":
                    return ArchiveReport;
                default:
                    return AccentPrimary;
            }
        }

        /// <summary>
        /// Ottiene colore per status
        /// </summary>
        public static Color GetStatusColor(string status)
        {
            switch (status?.ToLower())
            {
                case "success":
                case "ok":
                case "active":
                case "online":
                    return Success;
                case "warning":
                case "pending":
                    return Warning;
                case "error":
                case "danger":
                case "offline":
                case "inactive":
                    return Danger;
                case "info":
                case "information":
                    return Info;
                default:
                    return TextSecondary;
            }
        }

        /// <summary>
        /// Ottiene colore LED per booleano
        /// </summary>
        public static Color GetLEDColor(bool isActive)
        {
            return isActive ? LEDGreen : LEDRed;
        }

        /// <summary>
        /// Applica tema a un Form
        /// </summary>
        public static void ApplyToForm(System.Windows.Forms.Form form)
        {
            form.BackColor = BgLight;
            form.ForeColor = TextPrimary;

            foreach (System.Windows.Forms.Control control in form.Controls)
            {
                ApplyToControl(control);
            }
        }

        /// <summary>
        /// Applica tema ricorsivamente a un controllo e ai suoi figli
        /// </summary>
        public static void ApplyToControl(System.Windows.Forms.Control control)
        {
            if (control is System.Windows.Forms.Panel)
            {
                control.BackColor = BgPanel;
            }
            else if (control is System.Windows.Forms.TextBox)
            {
                control.BackColor = BgInput;
                control.ForeColor = TextPrimary;
            }
            else if (control is System.Windows.Forms.ComboBox)
            {
                control.BackColor = BgInput;
                control.ForeColor = TextPrimary;
            }
            else if (control is System.Windows.Forms.Label)
            {
                control.ForeColor = TextPrimary;
            }
            else if (control is System.Windows.Forms.Button btn)
            {
                btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = ButtonPrimary;
                btn.ForeColor = Color.White;
                btn.Cursor = System.Windows.Forms.Cursors.Hand;
            }

            // ✅ RICORSIONE SUI FIGLI
            foreach (System.Windows.Forms.Control child in control.Controls)
            {
                ApplyToControl(child);
            }
        }
    }
}