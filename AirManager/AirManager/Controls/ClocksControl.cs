using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using AirManager.Services.Database;
using AirManager.Services;
using AirManager.Forms;
using AirManager.Themes;
using AirManager.Models;
using Newtonsoft.Json;

namespace AirManager.Controls
{
    public partial class ClocksControl : UserControl
    {
        public event EventHandler<string> StatusChanged;

        public ClocksControl()
        {
            InitializeComponent();

            this.Resize += ClocksControl_Resize;

            ApplyLanguage();
            RefreshClocks();

            LanguageManager.LanguageChanged += OnLanguageChanged;
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            ApplyLanguage();
            RefreshClocks();
        }

        private void ApplyLanguage()
        {
            if (this.Controls.Find("lblTitle", true).FirstOrDefault() is Label lbl)
                lbl.Text = LanguageManager.GetString("Clocks.Title", "🕐 GESTIONE CLOCK");

            if (btnNew != null)
                btnNew.Text = "➕ " + LanguageManager.GetString("Clocks.NewClock", "Nuovo Clock");

            if (btnRefresh != null)
                btnRefresh.Text = "🔄 " + LanguageManager.GetString("Clocks.Refresh", "Aggiorna");

            if (lblDefault != null && !string.IsNullOrEmpty(lblDefault.Text))
            {
                string text = lblDefault.Text;
                if (text.Contains("⭐"))
                {
                    int colonIndex = text.IndexOf(':');
                    if (colonIndex > 0 && colonIndex < text.Length - 1)
                    {
                        string clockName = text.Substring(colonIndex + 1).Trim();
                        lblDefault.Text = "⭐ " + string.Format(
                            LanguageManager.GetString("Clocks.DefaultClock", "Predefinito:  {0}"), clockName);
                    }
                }
                else if (text.Contains("⚠️"))
                {
                    lblDefault.Text = LanguageManager.GetString("Clocks.NoDefaultClock", "⚠️ Nessun clock predefinito");
                }
            }

            if (lblStatus != null && !string.IsNullOrEmpty(lblStatus.Text))
            {
                string[] parts = lblStatus.Text.Split(' ');
                if (parts.Length > 1 && int.TryParse(parts[1], out int count))
                {
                    lblStatus.Text = string.Format(
                        LanguageManager.GetString("Clocks.ClocksAvailable", "📊 {0} clock disponibili"), count);
                }
            }
        }

        private double NormalizeDuration(double duration)
        {
            return duration / 1000.0;
        }

        private void ClocksControl_Resize(object sender, EventArgs e)
        {
            if (flowClocks != null && flowClocks.Controls.Count > 0)
            {
                int newWidth = flowClocks.ClientSize.Width - 30;

                foreach (Control ctrl in flowClocks.Controls)
                {
                    if (ctrl is Panel card)
                    {
                        card.Width = newWidth;

                        int btnX = newWidth - 155;
                        foreach (Control btnCtrl in card.Controls)
                        {
                            if (btnCtrl is Button btn)
                            {
                                if (btn.Text == "✏️")
                                    btn.Location = new Point(btnX, 18);
                                else if (btn.Text == "⭐" || btn.Text == "☆")
                                    btn.Location = new Point(btnX + 50, 18);
                                else if (btn.Text == "🗑️")
                                    btn.Location = new Point(btnX + 100, 18);
                            }
                        }
                    }
                }
            }
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            ClockEditorForm editorForm = new ClockEditorForm(null);
            if (editorForm.ShowDialog() == DialogResult.OK)
            {
                RefreshClocks();
                StatusChanged?.Invoke(this, LanguageManager.GetString("Clocks.ClockCreated", "✅ Clock creato con successo"));
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            RefreshClocks();
        }

        public void RefreshClocks()
        {
            if (flowClocks == null)
                return;

            flowClocks.SuspendLayout();
            flowClocks.Controls.Clear();

            var clocks = DbcManager.LoadFromCsv<ClockEntry>("Clocks.dbc");

            ClockEntry defaultClock = null;

            foreach (var clock in clocks.OrderBy(c => c.ClockName))
            {
                if (clock.IsDefault == 1)
                    defaultClock = clock;

                Panel card = CreateClockCard(clock);
                flowClocks.Controls.Add(card);
            }

            flowClocks.ResumeLayout();

            if (defaultClock != null)
            {
                lblDefault.Text = "⭐ " + string.Format(
                    LanguageManager.GetString("Clocks.DefaultClock", "Predefinito:  {0}"), defaultClock.ClockName);
                lblDefault.ForeColor = Color.FromArgb(255, 152, 0);
            }
            else
            {
                lblDefault.Text = LanguageManager.GetString("Clocks.NoDefaultClock", "⚠️ Nessun clock predefinito");
                lblDefault.ForeColor = Color.Red;
            }

            lblStatus.Text = string.Format(
                LanguageManager.GetString("Clocks.ClocksAvailable", "📊 {0} clock disponibili"), clocks.Count);
            StatusChanged?.Invoke(this, $"Clocks: {clocks.Count} " +
                LanguageManager.GetString("Clocks.Elements", "elementi"));
        }

        private Panel CreateClockCard(ClockEntry clock)
        {
            bool isDefault = clock.IsDefault == 1;

            int itemCount = 0;
            List<ClockItem> items = null;
            double totalDuration = 0;

            try
            {
                items = JsonConvert.DeserializeObject<List<ClockItem>>(clock.Items);
                itemCount = items?.Count ?? 0;

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        totalDuration += GetAverageItemDuration(item);
                    }
                }
            }
            catch { }

            int cardWidth = flowClocks.ClientSize.Width - 30;

            Panel card = new Panel
            {
                Width = cardWidth,
                Height = 75,
                BackColor = isDefault ? Color.FromArgb(255, 248, 225) : Color.White,
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(12)
            };

            card.Paint += (s, e) =>
            {
                Color borderColor = isDefault ? Color.FromArgb(255, 193, 7) : AppTheme.Primary;
                using (Pen pen = new Pen(borderColor, 2))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            Label lblName = new Label
            {
                Text = clock.ClockName,
                Location = new Point(12, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblName);

            if (isDefault)
            {
                Label lblDefaultBadge = new Label
                {
                    Text = "⭐ " + LanguageManager.GetString("Clocks.Default", "PREDEFINITO"),
                    Location = new Point(lblName.Right + 10, 10),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 7, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(255, 152, 0),
                    Padding = new Padding(5, 2, 5, 2)
                };
                card.Controls.Add(lblDefaultBadge);
            }

            TimeSpan duration = TimeSpan.FromSeconds(totalDuration);
            string formattedDuration = string.Format("{0:D2}:{1:D2}:{2:D2}",
                (int)duration.TotalHours, duration.Minutes, duration.Seconds);

            string preview = items != null && items.Count > 0
                ? GetItemsPreview(items)
                : LanguageManager.GetString("Clocks.NoElements", "Nessun elemento");

            Label lblInfo = new Label
            {
                Text = $"⏱️ {LanguageManager.GetString("Clocks.ExpectedDuration", "Durata prevista")}: {formattedDuration}  |  📊 {LanguageManager.GetString("Clocks.ElementsPresent", "Elementi presenti")}: {itemCount}  |  {preview}",
                Location = new Point(12, 35),
                Size = new Size(card.Width - 200, 30),
                Font = new Font("Segoe UI", 8),
                ForeColor = AppTheme.TextSecondary,
                AutoEllipsis = true,
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblInfo);

            int btnX = cardWidth - 155;
            int btnY = 18;

            Button btnEdit = new Button
            {
                Text = "✏️",
                Location = new Point(btnX, btnY),
                Size = new Size(38, 38),
                BackColor = AppTheme.Warning,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13),
                Cursor = Cursors.Hand,
                Tag = clock
            };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += BtnEditCard_Click;

            Button btnSetDefault = new Button
            {
                Text = isDefault ? "⭐" : "☆",
                Location = new Point(btnX + 50, btnY),
                Size = new Size(38, 38),
                BackColor = isDefault ? Color.FromArgb(255, 152, 0) : Color.FromArgb(150, 150, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13),
                Cursor = Cursors.Hand,
                Tag = clock,
                Enabled = !isDefault
            };
            btnSetDefault.FlatAppearance.BorderSize = 0;
            btnSetDefault.Click += BtnSetDefaultCard_Click;

            Button btnDelete = new Button
            {
                Text = "🗑️",
                Location = new Point(btnX + 100, btnY),
                Size = new Size(38, 38),
                BackColor = AppTheme.Danger,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13),
                Cursor = Cursors.Hand,
                Tag = clock,
                Enabled = !isDefault
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += BtnDeleteCard_Click;

            card.Controls.Add(btnEdit);
            card.Controls.Add(btnSetDefault);
            card.Controls.Add(btnDelete);

            btnEdit.BringToFront();
            btnSetDefault.BringToFront();
            btnDelete.BringToFront();

            ToolTip tooltip = new ToolTip();
            tooltip.SetToolTip(btnEdit, LanguageManager.GetString("Clocks.EditClock", "Modifica clock"));
            tooltip.SetToolTip(btnSetDefault, isDefault
                ? LanguageManager.GetString("Clocks.AlreadyDefault", "Già predefinito")
                : LanguageManager.GetString("Clocks.SetAsDefault", "Imposta come predefinito"));
            tooltip.SetToolTip(btnDelete, isDefault
                ? LanguageManager.GetString("Clocks.CannotDeleteDefault", "Non puoi eliminare il clock predefinito")
                : LanguageManager.GetString("Clocks.DeleteClock", "Elimina clock"));

            return card;
        }

        private double GetAverageItemDuration(ClockItem item)
        {
            try
            {
                string source = item.Type.StartsWith("Music_") ? "Music" : "Clips";
                string type = item.Type.Replace("Music_", "").Replace("Clips_", "");

                if (source == "Music")
                {
                    var musicEntries = DbcManager.LoadFromCsv<MusicEntry>("Music.dbc");
                    var filtered = ApplyItemFilterToMusic(musicEntries, item, type);

                    if (filtered.Count > 0)
                    {
                        double totalSeconds = filtered.Sum(e => NormalizeDuration(e.Duration));
                        return totalSeconds / filtered.Count;
                    }
                }
                else
                {
                    var clipEntries = DbcManager.LoadFromCsv<ClipEntry>("Clips.dbc");
                    var filtered = ApplyItemFilterToClips(clipEntries, item, type);

                    if (filtered.Count > 0)
                    {
                        double totalSeconds = filtered.Sum(e => NormalizeDuration(e.Duration));
                        return totalSeconds / filtered.Count;
                    }
                }

                return 180;
            }
            catch
            {
                return 180;
            }
        }

        private List<MusicEntry> ApplyItemFilterToMusic(List<MusicEntry> entries, ClockItem item, string type)
        {
            var filtered = entries.AsEnumerable();

            if (type == "Category")
            {
                filtered = filtered.Where(e =>
                    !string.IsNullOrEmpty(e.Categories) &&
                    e.Categories.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Any(c => c.Trim().Equals(item.Value, StringComparison.OrdinalIgnoreCase)));
            }
            else if (type == "Genre")
            {
                filtered = filtered.Where(e =>
                    !string.IsNullOrEmpty(e.Genre) &&
                    e.Genre.Trim().Equals(item.Value, StringComparison.OrdinalIgnoreCase));
            }
            else if (type == "Category+Genre")
            {
                string[] parts = item.Value.Split(new[] { " + " }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    string category = parts[0];
                    string genre = parts[1];
                    filtered = filtered.Where(e =>
                        !string.IsNullOrEmpty(e.Categories) &&
                        e.Categories.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Any(c => c.Trim().Equals(category, StringComparison.OrdinalIgnoreCase)) &&
                        !string.IsNullOrEmpty(e.Genre) &&
                        e.Genre.Trim().Equals(genre, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (item.YearFilterEnabled)
                filtered = filtered.Where(e => e.Year >= item.YearFrom && e.Year <= item.YearTo);

            return filtered.ToList();
        }

        private List<ClipEntry> ApplyItemFilterToClips(List<ClipEntry> entries, ClockItem item, string type)
        {
            var filtered = entries.AsEnumerable();

            if (type == "Category")
            {
                filtered = filtered.Where(e =>
                    !string.IsNullOrEmpty(e.Categories) &&
                    e.Categories.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Any(c => c.Trim().Equals(item.Value, StringComparison.OrdinalIgnoreCase)));
            }
            else if (type == "Genre")
            {
                filtered = filtered.Where(e =>
                    !string.IsNullOrEmpty(e.Genre) &&
                    e.Genre.Trim().Equals(item.Value, StringComparison.OrdinalIgnoreCase));
            }
            else if (type == "Category+Genre")
            {
                string[] parts = item.Value.Split(new[] { " + " }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    string category = parts[0];
                    string genre = parts[1];
                    filtered = filtered.Where(e =>
                        !string.IsNullOrEmpty(e.Categories) &&
                        e.Categories.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Any(c => c.Trim().Equals(category, StringComparison.OrdinalIgnoreCase)) &&
                        !string.IsNullOrEmpty(e.Genre) &&
                        e.Genre.Trim().Equals(genre, StringComparison.OrdinalIgnoreCase));
                }
            }

            return filtered.ToList();
        }

        private string GetItemsPreview(List<ClockItem> items)
        {
            if (items == null || items.Count == 0)
                return LanguageManager.GetString("Clocks.NoElements", "Nessun elemento");

            var preview = new List<string>();
            int maxItems = Math.Min(items.Count, 3);

            for (int i = 0; i < maxItems; i++)
            {
                var item = items[i];
                string source = "";
                string type = item.Type;

                if (type.StartsWith("Music_"))
                {
                    source = "🎵";
                    type = type.Substring(6);
                }
                else if (type.StartsWith("Clips_"))
                {
                    source = "⚡";
                    type = type.Substring(6);
                }

                string value = !string.IsNullOrEmpty(item.Value) ? item.Value : item.CategoryName;
                preview.Add($"{source} {value}");
            }

            string result = string.Join(" → ", preview);
            if (items.Count > 3)
                result += $" ... (+{items.Count - 3})";

            return result;
        }

        private void BtnEditCard_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            ClockEntry entry = btn.Tag as ClockEntry;

            ClockEditorForm editorForm = new ClockEditorForm(entry);
            if (editorForm.ShowDialog() == DialogResult.OK)
            {
                RefreshClocks();
                StatusChanged?.Invoke(this, LanguageManager.GetString("Clocks.ClockUpdated", "✅ Clock aggiornato"));
            }
        }

        private void BtnSetDefaultCard_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            ClockEntry entry = btn.Tag as ClockEntry;

            var allClocks = DbcManager.LoadFromCsv<ClockEntry>("Clocks.dbc");

            foreach (var clock in allClocks)
            {
                clock.IsDefault = (clock.ID == entry.ID) ? 1 : 0;
                DbcManager.Update("Clocks.dbc", clock);
            }

            RefreshClocks();
            StatusChanged?.Invoke(this, "⭐ " + string.Format(
                LanguageManager.GetString("Clocks.DefaultClockSet", "Clock predefinito: {0}"), entry.ClockName));
        }

        private void BtnDeleteCard_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            ClockEntry entry = btn.Tag as ClockEntry;

            if (entry.IsDefault == 1)
            {
                MessageBox.Show(
                    LanguageManager.GetString("Clocks.CannotDeleteDefaultMessage",
                        "❌ Non puoi eliminare il clock predefinito.\n\nImposta prima un altro clock come predefinito."),
                    LanguageManager.GetString("Clocks.OperationNotAllowed", "Operazione Non Consentita"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var schedules = DbcManager.LoadFromCsv<ScheduleEntry>("Schedules.dbc");
            var usedInSchedules = schedules.Where(s =>
                s.Type == "PlayClock" && s.ClockName == entry.ClockName).ToList();

            if (usedInSchedules.Count > 0)
            {
                MessageBox.Show(
                    string.Format(LanguageManager.GetString("Clocks.ClockInUseMessage",
                        "❌ Questo clock è utilizzato in {0} schedulazione/i.\n\nElimina o modifica prima le schedulazioni che lo utilizzano."),
                        usedInSchedules.Count),
                    LanguageManager.GetString("Clocks.ClockInUse", "Clock In Uso"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                string.Format(LanguageManager.GetString("Clocks.ConfirmDeleteMessage",
                    "🗑️ Eliminare il clock '{0}'?\n\nQuesta operazione non può essere annullata."), entry.ClockName),
                LanguageManager.GetString("Clocks.ConfirmDelete", "Conferma Eliminazione"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                bool success = DbcManager.Delete<ClockEntry>("Clocks.dbc", entry.ID);

                if (success)
                {
                    RefreshClocks();
                    StatusChanged?.Invoke(this, LanguageManager.GetString("Clocks.ClockDeleted", "✅ Clock eliminato"));
                }
                else
                {
                    MessageBox.Show(
                        LanguageManager.GetString("Clocks.DeleteError", "❌ Errore durante l'eliminazione del clock"),
                        LanguageManager.GetString("Common.Error", "Errore"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

    }
}
