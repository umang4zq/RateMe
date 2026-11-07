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

        // Right Action Controls (Separate Admin, Theme, Sign In, Sign Up, Profile, Exit)
        private FlowLayoutPanel _flowRightActions;
        private PillButton _btnAdmin;
        private PillButton _btnThemeToggle;
        private PillButton _btnSignIn;
        private PillButton _btnSignUp;
        private PillButton _btnUserProfile;
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
                BackColor = Theme.HouseGreen,
                Location = new Point(24, 18),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            _lblLogo.Click += (s, e) => SwitchView("Explore");

            // Navigation Tabs Flow Layout (Public Browsing Tabs only)
            _flowNavTabs = new FlowLayoutPanel
            {
                Location = new Point(155, 16),
                Size = new Size(520, 42),
                BackColor = Theme.HouseGreen,
                WrapContents = false
            };

            _btnNavExplore = CreateNavPill("Explore", () => SwitchView("Explore"));
            _btnNavSearch = CreateNavPill("Search", () => SwitchView("Search"));
            _btnNavWatchlist = CreateNavPill("Watchlist", () => SwitchView("Watchlist"));
            _btnNavProfile = CreateNavPill("Profile", () => SwitchView("Profile"));
            _btnNavSettings = CreateNavPill("Settings", () => SwitchView("Settings"));

            _flowNavTabs.Controls.Add(_btnNavExplore);
            _flowNavTabs.Controls.Add(_btnNavSearch);
            _flowNavTabs.Controls.Add(_btnNavWatchlist);
            _flowNavTabs.Controls.Add(_btnNavProfile);
            _flowNavTabs.Controls.Add(_btnNavSettings);

            // Right Action Controls (Separate Admin + Theme + Sign In + Sign Up / Profile)
            _flowRightActions = new FlowLayoutPanel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Size = new Size(520, 42),
                Location = new Point(ClientSize.Width - 530, 16),
                BackColor = Theme.HouseGreen,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };

            // 1. Exit / Sign Out Button (Visible when logged in)
            _btnSignOut = new PillButton
            {
                Text = "Exit",
                Style = PillButtonStyle.WhiteOutlined,
                Size = new Size(65, 36),
                Margin = new Padding(6, 0, 0, 0),
                Visible = false
            };
            _btnSignOut.Click += (s, e) =>
            {
                _auth.Logout();
                ShowToast("Signed out successfully.");
            };

            // 2. User Profile Pill (Visible when logged in)
            _btnUserProfile = new PillButton
            {
                Text = "👤 Account",
                Style = PillButtonStyle.WhiteFilled,
                Size = new Size(130, 36),
                Margin = new Padding(6, 0, 0, 0),
                Visible = false
            };
            _btnUserProfile.Click += (s, e) => SwitchView("Profile");

            // 3. Dedicated "Sign Up" Button (Visible when logged out)
            _btnSignUp = new PillButton
            {
                Text = "Join now",
                Style = PillButtonStyle.WhiteFilled,
                Size = new Size(95, 36),
                Margin = new Padding(6, 0, 0, 0)
            };
            _btnSignUp.Click += (s, e) => OpenSignUpDialog();

            // 4. Dedicated "Sign In" Button (Visible when logged out)
            _btnSignIn = new PillButton
            {
                Text = "Sign In",
                Style = PillButtonStyle.WhiteOutlined,
                Size = new Size(85, 36),
                Margin = new Padding(6, 0, 0, 0)
            };
            _btnSignIn.Click += (s, e) => OpenSignInDialog();

            // 5. Theme Toggle Button
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

            // 6. Separate Dedicated Admin Button
            _btnAdmin = new PillButton
            {
                Text = "Admin ⚙",
                Style = PillButtonStyle.WhiteOutlined,
                Size = new Size(98, 36),
                Margin = new Padding(8, 0, 8, 0),
                Font = new Font(Theme.PrimaryFontFamily, 9.5f, FontStyle.Bold)
            };
            _btnAdmin.Click += (s, e) => OnNavAdminClick();

            // Add in Right-to-Left order:
            _flowRightActions.Controls.Add(_btnSignOut);
            _flowRightActions.Controls.Add(_btnUserProfile);
            _flowRightActions.Controls.Add(_btnSignUp);
            _flowRightActions.Controls.Add(_btnSignIn);
            _flowRightActions.Controls.Add(_btnThemeToggle);
            _flowRightActions.Controls.Add(_btnAdmin);

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
                    _flowRightActions.Location = new Point(ClientSize.Width - 530, 16);
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
            _btnAdmin.Style = viewName == "Admin" ? PillButtonStyle.WhiteFilled : PillButtonStyle.WhiteOutlined;

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
                    _profileView = new ProfileView(_db, _auth, toastAction, OpenMediaDetailDialog, OpenSignInDialog);
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
                using (var dlg = new AuthDialog(_auth, startInSignUp: false))
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

        private void OpenSignInDialog()
        {
            if (_auth.IsLoggedIn)
            {
                SwitchView("Profile");
                return;
            }

            using (var dlg = new AuthDialog(_auth, startInSignUp: false))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    ShowToast($"Welcome back, {_auth.CurrentUser.DisplayName}!");
                    UpdateAuthUI();
                }
            }
        }

        private void OpenSignUpDialog()
        {
            if (_auth.IsLoggedIn)
            {
                SwitchView("Profile");
                return;
            }

            using (var dlg = new AuthDialog(_auth, startInSignUp: true))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    ShowToast($"Account created! Welcome, {_auth.CurrentUser.DisplayName}!");
                    UpdateAuthUI();
                }
            }
        }

        private void UpdateAuthUI()
        {
            if (_auth.IsLoggedIn)
            {
                _btnSignIn.Visible = false;
                _btnSignUp.Visible = false;

                _btnUserProfile.Text = "👤 " + (_auth.CurrentUser.DisplayName ?? _auth.CurrentUser.Username);
                _btnUserProfile.Visible = true;
                _btnSignOut.Visible = true;

                _btnAdmin.Visible = true;
                _btnAdmin.Text = _auth.IsAdmin ? "Admin ★ ⚙" : "Admin ⚙";
            }
            else
            {
                _btnSignIn.Visible = true;
                _btnSignUp.Visible = true;
                _btnUserProfile.Visible = false;
                _btnSignOut.Visible = false;

                _btnAdmin.Visible = true;
                _btnAdmin.Text = "Admin ⚙";
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
            SwitchView("Search");
        }

        private void ShowToast(string message, bool isSuccess = true)
        {
            _toast.ShowToast(this, message, isSuccess);
        }
    }
}
