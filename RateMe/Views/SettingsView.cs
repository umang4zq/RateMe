using System;
using System.Drawing;
using System.Windows.Forms;
using RateMe.Services;
using RateMe.UI;
using RateMe.UI.Controls;

namespace RateMe.Views
{
    public class SettingsView : UserControl
    {
        private readonly DatabaseService _db;
        private readonly OmdbService _omdb;
        private readonly Action<string> _showToast;
        private readonly Action _onThemeChanged;

        private PillButton _btnDarkModeToggle;
        private ModernTextBox _txtApiKey;
        private PillButton _btnTestApi;
        private ModernTextBox _txtDbConn;
        private PillButton _btnTestDb;
        private Label _lblDbStatus;

        public SettingsView(DatabaseService db, OmdbService omdb, Action<string> showToast, Action onThemeChanged)
        {
            _db = db;
            _omdb = omdb;
            _showToast = showToast;
            _onThemeChanged = onThemeChanged;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.PageCanvas;

            var scrollHost = new KineticScrollPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.PageCanvas
            };

            // 1. Appearance / Dark Mode Card
            var cardAppearance = new RoundedCard
            {
                Location = new Point(32, 20),
                Size = new Size(910, 120),
                BorderRadius = 14,
                DrawShadow = true
            };

            var lblAppHeader = new Label
            {
                Text = "APPEARANCE & THEME",
                Font = Theme.SmallBold,
                ForeColor = Theme.StarbucksGreen,
                Location = new Point(20, 14),
                AutoSize = true
            };

            var lblAppDesc = new Label
            {
                Text = "Switch between Starbucks Warm Cream light canvas and House Green dark mode.",
                Font = Theme.Small,
                ForeColor = Theme.TextSecondary,
                Location = new Point(20, 38),
                AutoSize = true
            };

            _btnDarkModeToggle = new PillButton
            {
                Text = Theme.IsDarkMode ? "Dark Mode: Active 🌙 (Click to Switch)" : "Light Mode: Active ☀ (Click to Switch)",
                Style = Theme.IsDarkMode ? PillButtonStyle.PrimaryFilled : PillButtonStyle.PrimaryOutlined,
                Location = new Point(20, 68),
                Size = new Size(280, 38)
            };
            _btnDarkModeToggle.Click += OnToggleDarkMode;

            cardAppearance.Controls.Add(lblAppHeader);
            cardAppearance.Controls.Add(lblAppDesc);
            cardAppearance.Controls.Add(_btnDarkModeToggle);

            // 2. OMDb API Key Card
            var cardApi = new RoundedCard
            {
                Location = new Point(32, 155),
                Size = new Size(910, 140),
                BorderRadius = 14,
                DrawShadow = true
            };

            var lblApiHeader = new Label
            {
                Text = "OMDB MOVIES & SERIES API CONFIGURATION",
                Font = Theme.SmallBold,
                ForeColor = Theme.StarbucksGreen,
                Location = new Point(20, 14),
                AutoSize = true
            };

            var lblApiDesc = new Label
            {
                Text = "RateMe connects to OMDb API for global movie, TV series, posters, and metadata.",
                Font = Theme.Small,
                ForeColor = Theme.TextSecondary,
                Location = new Point(20, 38),
                AutoSize = true
            };

            _txtApiKey = new ModernTextBox
            {
                Text = _omdb.ApiKey,
                Location = new Point(20, 68),
                Size = new Size(420, 38)
            };

            _btnTestApi = new PillButton
            {
                Text = "Test & Save API Key",
                Style = PillButtonStyle.PrimaryFilled,
                Location = new Point(455, 68),
                Size = new Size(180, 38)
            };
            _btnTestApi.Click += OnTestApiClick;

            cardApi.Controls.Add(lblApiHeader);
            cardApi.Controls.Add(lblApiDesc);
            cardApi.Controls.Add(_txtApiKey);
            cardApi.Controls.Add(_btnTestApi);

            // 3. Database Connection Card
            var cardDb = new RoundedCard
            {
                Location = new Point(32, 310),
                Size = new Size(910, 160),
                BorderRadius = 14,
                DrawShadow = true
            };

            var lblDbHeader = new Label
            {
                Text = "MICROSOFT SQL SERVER / LOCALDB CONFIGURATION",
                Font = Theme.SmallBold,
                ForeColor = Theme.StarbucksGreen,
                Location = new Point(20, 14),
                AutoSize = true
            };

            _lblDbStatus = new Label
            {
                Text = _db.IsUsingSqlServer 
                    ? "Status: Connected to Microsoft SQL Server / LocalDB ✓" 
                    : "Status: Using Local JSON Store (SQL Server instance not detected or not started).",
                Font = Theme.SmallBold,
                ForeColor = _db.IsUsingSqlServer ? Theme.GreenAccent : Theme.Gold,
                Location = new Point(20, 38),
                Size = new Size(800, 20)
            };

            _txtDbConn = new ModernTextBox
            {
                Text = _db.ConnectionString,
                Location = new Point(20, 68),
                Size = new Size(620, 38)
            };

            _btnTestDb = new PillButton
            {
                Text = "Test Connection",
                Style = PillButtonStyle.PrimaryOutlined,
                Location = new Point(655, 68),
                Size = new Size(160, 38)
            };
            _btnTestDb.Click += OnTestDbClick;

            var lblSqlHint = new Label
            {
                Text = "Schema script 'RateMe.sql' is located in the solution folder. Run it in SSMS or VS 2019 to create RateMeDb.",
                Font = Theme.Small,
                ForeColor = Theme.TextSecondary,
                Location = new Point(20, 118),
                Size = new Size(800, 20)
            };

            cardDb.Controls.Add(lblDbHeader);
            cardDb.Controls.Add(_lblDbStatus);
            cardDb.Controls.Add(_txtDbConn);
            cardDb.Controls.Add(_btnTestDb);
            cardDb.Controls.Add(lblSqlHint);

            // 4. About & Design System Card
            var cardAbout = new RoundedCard
            {
                Location = new Point(32, 485),
                Size = new Size(910, 110),
                BorderRadius = 14,
                DrawShadow = true
            };

            var lblAboutHeader = new Label
            {
                Text = "ABOUT RATEME DESKTOP APP",
                Font = Theme.SmallBold,
                ForeColor = Theme.StarbucksGreen,
                Location = new Point(20, 14),
                AutoSize = true
            };

            var lblAboutText = new Label
            {
                Text = "RateMe v1.0.0 • Built with C# Windows Forms (.NET Framework 4.7.2) for Visual Studio 2019.\n" +
                       "Design System inspired strictly by Starbucks Flagship Retail Design (SoDoSans typography, 50px pill buttons, House Green & Gold accents).",
                Font = Theme.Body,
                ForeColor = Theme.TextPrimary,
                Location = new Point(20, 38),
                Size = new Size(870, 50)
            };

            cardAbout.Controls.Add(lblAboutHeader);
            cardAbout.Controls.Add(lblAboutText);

            scrollHost.Controls.Add(cardAppearance);
            scrollHost.Controls.Add(cardApi);
            scrollHost.Controls.Add(cardDb);
            scrollHost.Controls.Add(cardAbout);

            Controls.Add(scrollHost);
        }

        private void OnToggleDarkMode(object sender, EventArgs e)
        {
            Theme.SetDarkMode(!Theme.IsDarkMode);
            _btnDarkModeToggle.Text = Theme.IsDarkMode ? "Dark Mode: Active 🌙 (Click to Switch)" : "Light Mode: Active ☀ (Click to Switch)";
            _btnDarkModeToggle.Style = Theme.IsDarkMode ? PillButtonStyle.PrimaryFilled : PillButtonStyle.PrimaryOutlined;
            _showToast?.Invoke(Theme.IsDarkMode ? "Switched to Dark Mode 🌙" : "Switched to Warm Light Canvas ☀");
            _onThemeChanged?.Invoke();
        }

        private async void OnTestApiClick(object sender, EventArgs e)
        {
            string key = _txtApiKey.Text.Trim();
            _btnTestApi.Text = "Testing...";
            bool valid = await _omdb.TestApiKeyAsync(key);
            if (valid)
            {
                _omdb.ApiKey = key;
                _showToast?.Invoke("OMDb API Key validated & saved! ✓");
                _btnTestApi.Text = "Valid & Saved ✓";
            }
            else
            {
                MessageBox.Show("Could not validate the OMDb API key with http://www.omdbapi.com/.", "API Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _btnTestApi.Text = "Test & Save API Key";
            }
        }

        private void OnTestDbClick(object sender, EventArgs e)
        {
            string conn = _txtDbConn.Text.Trim();
            bool ok = _db.TryConnectSqlServer(conn);
            if (ok)
            {
                _db.ConnectionString = conn;
                _lblDbStatus.Text = "Status: Successfully connected to SQL Server! ✓";
                _lblDbStatus.ForeColor = Theme.GreenAccent;
                _showToast?.Invoke("SQL Server connection established! ✓");
            }
            else
            {
                _lblDbStatus.Text = "Status: Could not connect to SQL Server. Operating in local JSON cache mode.";
                _lblDbStatus.ForeColor = Theme.DestructiveRed;
                MessageBox.Show("Could not connect to the specified SQL Server database. Make sure SQL Server or LocalDB is running, and that RateMeDb exists (run RateMe.sql). RateMe will continue working smoothly using its local storage.", "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}

