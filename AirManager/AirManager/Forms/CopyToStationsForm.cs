using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AirManager.Services;
using AirManager.Themes;

namespace AirManager.Forms
{
    public partial class CopyToStationsForm : Form
    {
        private readonly List<int> _selectedIds;
        private readonly string _archiveType;
        private List<StationConfig> _availableStations;

        private Label lblSummary;
        private Label lblStations;
        private CheckedListBox clbStations;
        private Button btnSelectAllStations;
        private Label lblFields;
        private CheckedListBox clbFields;
        private Button btnSelectAllFields;
        private Button btnOK;
        private Button btnCancel;

        /// <summary>
        /// Selected target station IDs
        /// </summary>
        public List<string> SelectedStationIds { get; private set; } = new List<string>();

        /// <summary>
        /// Data field selection flags
        /// </summary>
        public bool CopyGenre { get; private set; }
        public bool CopyCategories { get; private set; }
        public bool CopyMarkers { get; private set; }
        public bool CopyHours { get; private set; }
        public bool CopyDays { get; private set; }
        public bool CopyMonths { get; private set; }

        public CopyToStationsForm(List<int> selectedIds, string archiveType)
        {
            InitializeComponent();

            _selectedIds = selectedIds;
            _archiveType = archiveType;

            this.Size = new Size(580, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = AppTheme.BgLight;
            this.ForeColor = AppTheme.TextPrimary;

            InitializeControls();
            LoadStations();
            ApplyLanguage();

            LanguageManager.LanguageChanged += (s, e) => ApplyLanguage();
        }

        private void InitializeControls()
        {
            // Summary label
            lblSummary = new Label
            {
                Location = new Point(20, 15),
                Size = new Size(540, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = AppTheme.AccentPrimary
            };
            this.Controls.Add(lblSummary);

            // Stations section
            lblStations = new Label
            {
                Location = new Point(20, 50),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White
            };
            this.Controls.Add(lblStations);

            clbStations = new CheckedListBox
            {
                Location = new Point(20, 75),
                Size = new Size(340, 140),
                Font = new Font("Segoe UI", 9),
                BackColor = AppTheme.BgInput,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                CheckOnClick = true
            };
            this.Controls.Add(clbStations);

            btnSelectAllStations = new Button
            {
                Location = new Point(370, 75),
                Size = new Size(180, 30),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                BackColor = AppTheme.ButtonSecondary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSelectAllStations.FlatAppearance.BorderSize = 0;
            btnSelectAllStations.Click += BtnSelectAllStations_Click;
            this.Controls.Add(btnSelectAllStations);

            // Fields section
            lblFields = new Label
            {
                Location = new Point(20, 230),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White
            };
            this.Controls.Add(lblFields);

            clbFields = new CheckedListBox
            {
                Location = new Point(20, 255),
                Size = new Size(340, 150),
                Font = new Font("Segoe UI", 9),
                BackColor = AppTheme.BgInput,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                CheckOnClick = true
            };
            this.Controls.Add(clbFields);

            btnSelectAllFields = new Button
            {
                Location = new Point(370, 255),
                Size = new Size(180, 30),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                BackColor = AppTheme.ButtonSecondary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSelectAllFields.FlatAppearance.BorderSize = 0;
            btnSelectAllFields.Click += BtnSelectAllFields_Click;
            this.Controls.Add(btnSelectAllFields);

            // OK / Cancel
            btnOK = new Button
            {
                Location = new Point(310, 430),
                Size = new Size(120, 38),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = AppTheme.ButtonSuccess,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.Click += BtnOK_Click;
            this.Controls.Add(btnOK);

            btnCancel = new Button
            {
                Location = new Point(440, 430),
                Size = new Size(120, 38),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = AppTheme.ButtonDanger,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; };
            this.Controls.Add(btnCancel);
        }

        private void LoadStations()
        {
            string activeId = StationRegistry.GetActiveStationId();
            _availableStations = StationRegistry.LoadAllStations()
                .Where(s => s.Id != activeId)
                .ToList();

            clbStations.Items.Clear();
            foreach (var station in _availableStations)
            {
                string display = $"{station.Name}  [{station.StationType}]";
                clbStations.Items.Add(display, false);
            }

            // Add data field items (all checked by default)
            clbFields.Items.Clear();
            clbFields.Items.Add("Genre", true);
            clbFields.Items.Add("Categories", true);
            clbFields.Items.Add("Markers (IN, INTRO, MIX, OUT)", true);
            clbFields.Items.Add("Airplay rules: Hours", true);
            clbFields.Items.Add("Airplay rules: Days", true);
            clbFields.Items.Add("Airplay rules: Months", true);
        }

        private void ApplyLanguage()
        {
            this.Text = LanguageManager.GetString("CopyToStations.Title");

            lblSummary.Text = string.Format(
                LanguageManager.GetString("CopyToStations.Summary"),
                _selectedIds.Count);

            lblStations.Text = LanguageManager.GetString("CopyToStations.TargetStations");
            lblFields.Text = LanguageManager.GetString("CopyToStations.DataFields");

            // Update field display names with translations
            if (clbFields.Items.Count >= 6)
            {
                bool[] checks = new bool[clbFields.Items.Count];
                for (int i = 0; i < checks.Length; i++)
                    checks[i] = clbFields.GetItemChecked(i);

                clbFields.Items.Clear();
                clbFields.Items.Add(LanguageManager.GetString("CopyToStations.Field.Genre"), checks[0]);
                clbFields.Items.Add(LanguageManager.GetString("CopyToStations.Field.Categories"), checks[1]);
                clbFields.Items.Add(LanguageManager.GetString("CopyToStations.Field.Markers"), checks[2]);
                clbFields.Items.Add(LanguageManager.GetString("CopyToStations.Field.Hours"), checks[3]);
                clbFields.Items.Add(LanguageManager.GetString("CopyToStations.Field.Days"), checks[4]);
                clbFields.Items.Add(LanguageManager.GetString("CopyToStations.Field.Months"), checks[5]);
            }

            UpdateSelectAllButtonText(btnSelectAllStations, clbStations,
                LanguageManager.GetString("CopyToStations.SelectAll"),
                LanguageManager.GetString("CopyToStations.DeselectAll"));
            UpdateSelectAllButtonText(btnSelectAllFields, clbFields,
                LanguageManager.GetString("CopyToStations.SelectAll"),
                LanguageManager.GetString("CopyToStations.DeselectAll"));

            btnOK.Text = "✓ " + LanguageManager.GetString("Common.OK");
            btnCancel.Text = "✖ " + LanguageManager.GetString("Common.Cancel");

            Console.WriteLine($"[CopyToStations] ✅ Lingua applicata: {LanguageManager.CurrentLanguage}");
        }

        private void UpdateSelectAllButtonText(Button btn, CheckedListBox clb, string selectAll, string deselectAll)
        {
            bool allChecked = clb.Items.Count > 0 && clb.CheckedItems.Count == clb.Items.Count;
            btn.Text = allChecked ? ("☐ " + deselectAll) : ("☑ " + selectAll);
        }

        private void BtnSelectAllStations_Click(object sender, EventArgs e)
        {
            bool allChecked = clbStations.Items.Count > 0 && clbStations.CheckedItems.Count == clbStations.Items.Count;
            for (int i = 0; i < clbStations.Items.Count; i++)
                clbStations.SetItemChecked(i, !allChecked);

            UpdateSelectAllButtonText(btnSelectAllStations, clbStations,
                LanguageManager.GetString("CopyToStations.SelectAll"),
                LanguageManager.GetString("CopyToStations.DeselectAll"));
        }

        private void BtnSelectAllFields_Click(object sender, EventArgs e)
        {
            bool allChecked = clbFields.Items.Count > 0 && clbFields.CheckedItems.Count == clbFields.Items.Count;
            for (int i = 0; i < clbFields.Items.Count; i++)
                clbFields.SetItemChecked(i, !allChecked);

            UpdateSelectAllButtonText(btnSelectAllFields, clbFields,
                LanguageManager.GetString("CopyToStations.SelectAll"),
                LanguageManager.GetString("CopyToStations.DeselectAll"));
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (clbStations.CheckedItems.Count == 0)
            {
                MessageBox.Show(
                    LanguageManager.GetString("CopyToStations.Validation.SelectStation"),
                    LanguageManager.GetString("Common.Warning"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (clbFields.CheckedItems.Count == 0)
            {
                MessageBox.Show(
                    LanguageManager.GetString("CopyToStations.Validation.SelectField"),
                    LanguageManager.GetString("Common.Warning"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Collect selected station IDs
            SelectedStationIds = new List<string>();
            for (int i = 0; i < clbStations.Items.Count; i++)
            {
                if (clbStations.GetItemChecked(i) && i < _availableStations.Count)
                {
                    SelectedStationIds.Add(_availableStations[i].Id);
                }
            }

            // Collect selected fields (by index)
            CopyGenre = clbFields.GetItemChecked(0);
            CopyCategories = clbFields.GetItemChecked(1);
            CopyMarkers = clbFields.GetItemChecked(2);
            CopyHours = clbFields.GetItemChecked(3);
            CopyDays = clbFields.GetItemChecked(4);
            CopyMonths = clbFields.GetItemChecked(5);

            this.DialogResult = DialogResult.OK;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                LanguageManager.LanguageChanged -= (s, e) => ApplyLanguage();
            }

            base.Dispose(disposing);
        }
    }
}
