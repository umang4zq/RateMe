using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using RateMe.Models;
using RateMe.Services;
using RateMe.UI;
using RateMe.UI.Controls;
using RateMe.Views;

namespace RateMe
{
    public partial class Form1 : Form
    {
        private readonly DatabaseService _db;
        private readonly AuthService _auth;
        private readonly OmdbService _omdb;

        // UI Shell Controls
        private Panel _pnlTopNav;
        private Label _lblLogo;
        private FlowLayoutPanel _flowNavTabs;
        private PillButton _btnNavExplore;
        private PillButton _btnNavSearch;
        private PillButton _btnNavWatchlist;
        private PillButton _btnNavProfile;
        private PillButton _btnNavSettings;
        private PillButton _btnNavAdmin;

        private FlowLayoutPanel _flowRightActions;
        private PillButton _btnThemeToggle;
        private PillButton _btnAuth;
        private PillButton _btnSignOut;

        private Panel _pnlContentHost;
        private FloatingFrapButton _btnFrap;
        private ToastNotification _toast;

        // Active Views
        private ExploreView _exploreView;
        private SearchView _searchView;
        private WatchlistView _watchlistView;
        private ProfileView _profileView;
        private SettingsView _settingsView;
        private AdminView _adminView;
        private string _activeViewName = "Explore";

        public Form1()
        {
            InitializeComponent();

            _db = new DatabaseService();
            _auth = new AuthService(_db);
            _omdb = new OmdbService("cf367844");

            BuildShellUI();
            HookEvents();
            SwitchView("Explore");
            UpdateAuthUI();
        }

        private void BuildShellUI()
        {
            BackColor = Theme.PageCanvas;

            // 1. Top Navigation Bar (Global Nav from rate-me-design.md)
            _pnlTopNav = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = Theme.HouseGreen
            };

            // Logo & Brand mark
            _lblLogo = new Label
            {
                Text = "RateMe ★",
                Font = new Font(Theme.SerifFontFamily, 16f, FontStyle.Bold),
                ForeColor = Theme.Gold,
                Location = new Point(24, 18),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            _lblLogo.Click += (s, e) => SwitchView("Explore");

            // Navigation Tabs Flow Layout
            _flowNavTabs = new FlowLayoutPanel
            {
                Location = new Point(160, 16),
                Size = new Size(640, 42),
                BackColor = Theme.HouseGreen,
                WrapContents = false
            };

            _btnNavExplore = CreateNavPill("Explore", () => SwitchView("Explore"));
            _btnNavSearch = CreateNavPill("Search", () => SwitchView("Search"));
            _btnNavWatchlist = CreateNavPill("Watchlist", () => SwitchView("Watchlist"));
            _btnNavProfile = CreateNavPill("Profile", () => SwitchView("Profile"));
            _btnNavSettings = CreateNavPill("Settings", () => SwitchView("Settings"));
            _btnNavAdmin = CreateNavPill("Admin ⚙", OnNavAdminClick);
            _btnNavAdmin.Size = new Size(100, 36);

            _flowNavTabs.Controls.Add(_btnNavExplore);
            _flowNavTabs.Controls.Add(_btnNavSearch);
            _flowNavTabs.Controls.Add(_btnNavWatchlist);
            _flowNavTabs.Controls.Add(_btnNavProfile);
            _flowNavTabs.Controls.Add(_btnNavSettings);
            _flowNavTabs.Controls.Add(_btnNavAdmin);

            // Right Action Controls
            _flowRightActions = new FlowLayoutPanel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Size = new Size(340, 42),
                Location = new Point(ClientSize.Width - 360, 16),
                BackColor = Theme.HouseGreen,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };

            _btnSignOut = new PillButton
            {
                Text = "Exit",
                Style = PillButtonStyle.WhiteOutlined,
                Size = new Size(68, 36),
                Margin = new Padding(6, 0, 0, 0),
                Visible = false
            };
            _btnSignOut.Click += (s, e) =>
            {
                _auth.Logout();
                ShowToast("Signed out successfully.");
            };

            _btnAuth = new PillButton
            {
                Text = "Sign In",
                Style = PillButtonStyle.WhiteFilled,
                Size = new Size(130, 36),
                Margin = new Padding(6, 0, 0, 0)
            };
            _btnAuth.Click += OnAuthButtonClick;

            _btnThemeToggle = new PillButton
            {
                Text = Theme.IsDarkMode ? "🌙 Dark" : "☀ Light",
                Style = PillButtonStyle.WhiteOutlined,
                Size = new Size(82, 36),
                Margin = new Padding(6, 0, 0, 0),
                Font = new Font(Theme.PrimaryFontFamily, 9f, FontStyle.Bold)
            };
            _btnThemeToggle.Click += (s, e) =>
            {
                Theme.SetDarkMode(!Theme.IsDarkMode);
                _btnThemeToggle.Text = Theme.IsDarkMode ? "🌙 Dark" : "☀ Light";
                ShowToast(Theme.IsDarkMode ? "Dark Mode Activated 🌙" : "Warm Light Canvas Activated ☀");
            };

            _flowRightActions.Controls.Add(_btnSignOut);
            _flowRightActions.Controls.Add(_btnAuth);
            _flowRightActions.Controls.Add(_btnThemeToggle);

            _pnlTopNav.Controls.Add(_lblLogo);
            _pnlTopNav.Controls.Add(_flowNavTabs);
            _pnlTopNav.Controls.Add(_flowRightActions);

            // 2. Content Host Panel
            _pnlContentHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.PageCanvas
            };

            // 3. Floating Frap Button (Signature element from rate-me-design.md)
            _btnFrap = new FloatingFrapButton
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(ClientSize.Width - 80, ClientSize.Height - 80)
            };
            _btnFrap.Click += OnFrapButtonClick;

            // 4. Toast Notification
            _toast = new ToastNotification();

            Controls.Add(_btnFrap);
            Controls.Add(_pnlContentHost);
            Controls.Add(_pnlTopNav);

            _btnFrap.BringToFront();
        }

        private PillButton CreateNavPill(string title, Action onClick)
        {
            var btn = new PillButton
            {
                Text = title,
                Style = PillButtonStyle.WhiteOutlined,
                Size = new Size(95, 36),
                Margin = new Padding(4, 0, 4, 0)
            };
            btn.Click += (s, e) => onClick();
            return btn;
        }

        private void HookEvents()
        {
            Theme.ThemeChanged += OnThemeChanged;
            _auth.AuthStateChanged += (user) => UpdateAuthUI();
            Resize += (s, e) =>
            {
                if (_flowRightActions != null)
                {
                    _flowRightActions.Location = new Point(ClientSize.Width - 360, 16);
                }
                if (_btnFrap != null)
                {
                    _btnFrap.Location = new Point(ClientSize.Width - 80, ClientSize.Height - 80);
                    _btnFrap.BringToFront();
                }
            };
        }

        private void OnThemeChanged()
        {
            BackColor = Theme.PageCanvas;
            _pnlContentHost.BackColor = Theme.PageCanvas;
            _btnThemeToggle.Text = Theme.IsDarkMode ? "🌙 Dark" : "☀ Light";

            SwitchView(_activeViewName, forceRefresh: true);
        }

        private void SwitchView(string viewName, bool forceRefresh = false)
        {
            if (_activeViewName == viewName && !forceRefresh && _pnlContentHost.Controls.Count > 0)
                return;

            _activeViewName = viewName;

            // Update nav pill styles
            _btnNavExplore.Style = viewName == "Explore" ? PillButtonStyle.WhiteFilled : PillButtonStyle.WhiteOutlined;
            _btnNavSearch.Style = viewName == "Search" ? PillButtonStyle.WhiteFilled : PillButtonStyle.WhiteOutlined;
            _btnNavWatchlist.Style = viewName == "Watchlist" ? PillButtonStyle.WhiteFilled : PillButtonStyle.WhiteOutlined;
            _btnNavProfile.Style = viewName == "Profile" ? PillButtonStyle.WhiteFilled : PillButtonStyle.WhiteOutlined;
            _btnNavSettings.Style = viewName == "Settings" ? PillButtonStyle.WhiteFilled : PillButtonStyle.WhiteOutlined;
            _btnNavAdmin.Style = viewName == "Admin" ? PillButtonStyle.WhiteFilled : PillButtonStyle.WhiteOutlined;

            _pnlContentHost.Controls.Clear();

            UserControl viewToHost = null;
            Action<string> toastAction = (msg) => ShowToast(msg);

            switch (viewName)
            {
                case "Explore":
                    _exploreView = new ExploreView(_db, _auth, _omdb, toastAction, OpenMediaDetailDialog);
                    viewToHost = _exploreView;
                    break;

                case "Search":
                    _searchView = new SearchView(_db, _auth, _omdb, toastAction, OpenMediaDetailDialog);
                    viewToHost = _searchView;
                    break;

                case "Watchlist":
                    _watchlistView = new WatchlistView(_db, _auth, toastAction, OpenMediaDetailDialog);
                    viewToHost = _watchlistView;
                    break;

                case "Profile":
                    _profileView = new ProfileView(_db, _auth, toastAction, OpenMediaDetailDialog, () => OnAuthButtonClick(null, null));
                    viewToHost = _profileView;
                    break;

                case "Settings":
                    _settingsView = new SettingsView(_db, _omdb, toastAction, OnThemeChanged);
                    viewToHost = _settingsView;
                    break;

                case "Admin":
                    _adminView = new AdminView(_db, _auth, _omdb, toastAction, OpenMediaDetailDialog);
                    viewToHost = _adminView;
                    break;
            }

            if (viewToHost != null)
            {
                _pnlContentHost.Controls.Add(viewToHost);
            }

            _btnFrap.BringToFront();
        }

        private void OnNavAdminClick()
        {
            if (!_auth.IsLoggedIn || !_auth.IsAdmin)
            {
                using (var dlg = new AuthDialog(_auth))
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        if (_auth.IsAdmin)
                        {
                            ShowToast("Administrator Access Granted ⚙");
                            SwitchView("Admin");
                        }
                        else
                        {
                            ShowToast("Access Denied: Admin role required (Try admin / Admin@123).", false);
                        }
                    }
                }
            }
            else
            {
                SwitchView("Admin");
            }
        }

        private void OpenMediaDetailDialog(MediaItem media)
        {
            Action<string> toastAction = (msg) => ShowToast(msg);
            using (var dlg = new MediaDetailDialog(media, _db, _auth, _omdb, toastAction))
            {
                dlg.ShowDialog(this);
            }

            if (_activeViewName == "Explore") _exploreView?.LoadMediaCards();
            if (_activeViewName == "Watchlist") _watchlistView?.LoadWatchlistItems();
            if (_activeViewName == "Profile") _profileView?.LoadUserData();
            if (_activeViewName == "Admin") _adminView?.RefreshAllData();
        }

        private void OnAuthButtonClick(object sender, EventArgs e)
        {
            if (_auth.IsLoggedIn)
            {
                SwitchView("Profile");
            }
            else
            {
                using (var dlg = new AuthDialog(_auth))
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        ShowToast($"Welcome back, {_auth.CurrentUser.DisplayName}!");
                        UpdateAuthUI();
                    }
                }
            }
        }

        private void UpdateAuthUI()
        {
            if (_auth.IsLoggedIn)
            {
                _btnAuth.Text = _auth.CurrentUser.DisplayName ?? _auth.CurrentUser.Username;
                _btnAuth.Style = PillButtonStyle.WhiteFilled;
                _btnSignOut.Visible = true;

                if (_auth.IsAdmin)
                {
                    _btnNavAdmin.Text = "Admin ⚙";
                }
            }
            else
            {
                _btnAuth.Text = "Sign In";
                _btnAuth.Style = PillButtonStyle.WhiteFilled;
                _btnSignOut.Visible = false;
            }

            if (_activeViewName == "Profile") _profileView?.LoadUserData();
            if (_activeViewName == "Watchlist") _watchlistView?.LoadWatchlistItems();
            if (_activeViewName == "Admin" && (!_auth.IsLoggedIn || !_auth.IsAdmin))
            {
                SwitchView("Explore");
            }
        }

        private void OnFrapButtonClick(object sender, EventArgs e)
        {
            var curated = _db.GetCuratedMedia();
            if (curated.Count > 0)
            {
                var rand = new Random();
                var randomPick = curated[rand.Next(curated.Count)];
                ShowToast($"Random Pick: {randomPick.Title} ★");
                OpenMediaDetailDialog(randomPick);
            }
            else
            {
                SwitchView("Search");
            }
        }

        public void ShowToast(string message)
        {
            ShowToast(message, true);
        }

        public void ShowToast(string message, bool isSuccess)
        {
            _toast.ShowToast(this, message, isSuccess);
        }
    }
}
