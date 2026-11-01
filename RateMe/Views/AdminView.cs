using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RateMe.Models;
using RateMe.Services;
using RateMe.UI;
using RateMe.UI.Controls;

namespace RateMe.Views
{
    public class AdminView : UserControl
    {
        private readonly DatabaseService _db;
        private readonly AuthService _auth;
        private readonly OmdbService _omdb;
        private readonly Action<string> _showToast;
        private readonly Action<MediaItem> _openMediaDetail;

        private KineticScrollPanel _scrollHost;
        private Label _lblStatUsers;
        private Label _lblStatMedia;
        private Label _lblStatRatings;
        private Label _lblStatDb;

        private FlowLayoutPanel _flowSubTabs;
        private PillButton _btnTabUsers;
        private PillButton _btnTabCatalog;
        private PillButton _btnTabReviews;
        private string _activeSubTab = "Users";

        // Tab containers
        private Panel _pnlUsersContainer;
        private Panel _pnlCatalogContainer;
        private Panel _pnlReviewsContainer;

        // User Tab controls
        private ModernTextBox _txtUserSearch;
        private FlowLayoutPanel _flowUsersList;
        private Label _lblUserCount;
        private string _userRoleFilter = "All";

        // Catalog Tab controls
        private ModernTextBox _txtImportImdbId;
        private PillButton _btnImportMovie;
        private FlowLayoutPanel _flowCatalogList;
        private Label _lblCatalogCount;

        // Moderation Tab controls
        private FlowLayoutPanel _flowReviewsList;
        private Label _lblReviewsCount;

        public AdminView(DatabaseService db, AuthService auth, OmdbService omdb, Action<string> showToast, Action<MediaItem> openMediaDetail)
        {
            _db = db;
            _auth = auth;
            _omdb = omdb;
            _showToast = showToast;
            _openMediaDetail = openMediaDetail;

            InitializeComponent();
            RefreshAllData();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.PageCanvas;

            _scrollHost = new KineticScrollPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.PageCanvas
            };

            // 1. Admin Header Hero Band
            var pnlHero = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1100, 190),
                BackColor = Theme.HouseGreen,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblHeroSuper = new Label
            {
                Text = "ADMINISTRATION & GOVERNANCE PORTAL",
                Font = Theme.SmallBold,
                ForeColor = Theme.Gold,
                Location = new Point(32, 20),
                AutoSize = true
            };

            var lblHeroTitle = new Label
            {
                Text = "System Control & User Lifecycle",
                Font = new Font(Theme.SerifFontFamily, 22f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(32, 44),
                Size = new Size(600, 40)
            };

            // Metrics Row
            var pnlMetrics = new FlowLayoutPanel
            {
                Location = new Point(32, 95),
                Size = new Size(1000, 75),
                BackColor = Color.Transparent,
                WrapContents = false
            };

            pnlMetrics.Controls.Add(CreateMetricCard("TOTAL USERS", "0", out _lblStatUsers));
            pnlMetrics.Controls.Add(CreateMetricCard("CATALOG ITEMS", "0", out _lblStatMedia));
            pnlMetrics.Controls.Add(CreateMetricCard("TOTAL REVIEWS", "0", out _lblStatRatings));
            pnlMetrics.Controls.Add(CreateMetricCard("DATABASE ENGINE", "SQL Server", out _lblStatDb, 220));

            pnlHero.Controls.Add(lblHeroSuper);
            pnlHero.Controls.Add(lblHeroTitle);
            pnlHero.Controls.Add(pnlMetrics);

            // 2. Sub Tabs Navigation
            _flowSubTabs = new FlowLayoutPanel
            {
                Location = new Point(32, 205),
                Size = new Size(1000, 46),
                BackColor = Color.Transparent,
                WrapContents = false
            };

            _btnTabUsers = new PillButton
            {
                Text = "User Management (CRUD)",
                Style = PillButtonStyle.PrimaryFilled,
                Size = new Size(190, 36),
                Margin = new Padding(0, 4, 10, 4)
            };
            _btnTabUsers.Click += (s, e) => SwitchSubTab("Users");

            _btnTabCatalog = new PillButton
            {
                Text = "Catalog & OMDb Import",
                Style = PillButtonStyle.PrimaryOutlined,
                Size = new Size(180, 36),
                Margin = new Padding(0, 4, 10, 4)
            };
            _btnTabCatalog.Click += (s, e) => SwitchSubTab("Catalog");

            _btnTabReviews = new PillButton
            {
                Text = "Review Moderation",
                Style = PillButtonStyle.PrimaryOutlined,
                Size = new Size(160, 36),
                Margin = new Padding(0, 4, 10, 4)
            };
            _btnTabReviews.Click += (s, e) => SwitchSubTab("Reviews");

            _flowSubTabs.Controls.Add(_btnTabUsers);
            _flowSubTabs.Controls.Add(_btnTabCatalog);
            _flowSubTabs.Controls.Add(_btnTabReviews);

            // 3. User Management Tab Container
            BuildUsersTab();

            // 4. Catalog Management Tab Container
            BuildCatalogTab();

            // 5. Review Moderation Tab Container
            BuildReviewsTab();

            _scrollHost.Controls.Add(pnlHero);
            _scrollHost.Controls.Add(_flowSubTabs);
            _scrollHost.Controls.Add(_pnlUsersContainer);
            _scrollHost.Controls.Add(_pnlCatalogContainer);
            _scrollHost.Controls.Add(_pnlReviewsContainer);

            Controls.Add(_scrollHost);

            SwitchSubTab("Users");
        }

        private Control CreateMetricCard(string title, string initialValue, out Label outValLabel, int width = 160)
        {
            var pnl = new Panel
            {
                Size = new Size(width, 68),
                BackColor = Color.FromArgb(40, Color.White),
                Margin = new Padding(0, 0, 16, 0)
            };

            var lblT = new Label
            {
                Text = title,
                Font = Theme.SmallBold,
                ForeColor = Theme.TextWhiteSoft,
                Location = new Point(12, 10),
                AutoSize = true
            };

            var lblV = new Label
            {
                Text = initialValue,
                Font = new Font(Theme.SerifFontFamily, 15f, FontStyle.Bold),
                ForeColor = Theme.Gold,
                Location = new Point(12, 32),
                Size = new Size(width - 24, 26),
                AutoEllipsis = true
            };

            outValLabel = lblV;
            pnl.Controls.Add(lblT);
            pnl.Controls.Add(lblV);
            return pnl;
        }

        #region SubTab 1: Users (CRUD)

        private void BuildUsersTab()
        {
            _pnlUsersContainer = new Panel
            {
                Location = new Point(32, 260),
                Size = new Size(1050, 900),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var pnlFilterBar = new RoundedCard
            {
                Location = new Point(0, 0),
                Size = new Size(1010, 68),
                BorderRadius = 12,
                DrawShadow = true
            };

            _txtUserSearch = new ModernTextBox
            {
                PlaceholderText = "Search by username, email, or display name...",
                Location = new Point(14, 15),
                Size = new Size(340, 38)
            };
            _txtUserSearch.TextChangedCustom += (s, e) => FilterAndRenderUsers();

            var btnFilterRoleAll = new PillButton
            {
                Text = "All Roles",
                Style = PillButtonStyle.PrimaryFilled,
                Location = new Point(370, 16),
                Size = new Size(85, 36)
            };
            var btnFilterAdmins = new PillButton
            {
                Text = "Admins",
                Style = PillButtonStyle.PrimaryOutlined,
                Location = new Point(462, 16),
                Size = new Size(80, 36)
            };
            var btnFilterRegular = new PillButton
            {
                Text = "Users",
                Style = PillButtonStyle.PrimaryOutlined,
                Location = new Point(548, 16),
                Size = new Size(75, 36)
            };

            btnFilterRoleAll.Click += (s, e) =>
            {
                _userRoleFilter = "All";
                btnFilterRoleAll.Style = PillButtonStyle.PrimaryFilled;
                btnFilterAdmins.Style = PillButtonStyle.PrimaryOutlined;
                btnFilterRegular.Style = PillButtonStyle.PrimaryOutlined;
                FilterAndRenderUsers();
            };
            btnFilterAdmins.Click += (s, e) =>
            {
                _userRoleFilter = "Admin";
                btnFilterRoleAll.Style = PillButtonStyle.PrimaryOutlined;
                btnFilterAdmins.Style = PillButtonStyle.PrimaryFilled;
                btnFilterRegular.Style = PillButtonStyle.PrimaryOutlined;
                FilterAndRenderUsers();
            };
            btnFilterRegular.Click += (s, e) =>
            {
                _userRoleFilter = "User";
                btnFilterRoleAll.Style = PillButtonStyle.PrimaryOutlined;
                btnFilterAdmins.Style = PillButtonStyle.PrimaryOutlined;
                btnFilterRegular.Style = PillButtonStyle.PrimaryFilled;
                FilterAndRenderUsers();
            };

            var btnAddUser = new PillButton
            {
                Text = "+ Add New User",
                Style = PillButtonStyle.PrimaryFilled,
                Location = new Point(840, 14),
                Size = new Size(150, 40)
            };
            btnAddUser.Click += OnAddNewUserClick;

            pnlFilterBar.Controls.Add(_txtUserSearch);
            pnlFilterBar.Controls.Add(btnFilterRoleAll);
            pnlFilterBar.Controls.Add(btnFilterAdmins);
            pnlFilterBar.Controls.Add(btnFilterRegular);
            pnlFilterBar.Controls.Add(btnAddUser);

            _lblUserCount = new Label
            {
                Text = "Showing all registered users:",
                Font = Theme.BodyBold,
                ForeColor = Theme.StarbucksGreen,
                Location = new Point(4, 80),
                AutoSize = true
            };

            _flowUsersList = new FlowLayoutPanel
            {
                Location = new Point(0, 105),
                Size = new Size(1010, 800),
                AutoSize = true,
                BackColor = Color.Transparent,
                WrapContents = true
            };

            _pnlUsersContainer.Controls.Add(pnlFilterBar);
            _pnlUsersContainer.Controls.Add(_lblUserCount);
            _pnlUsersContainer.Controls.Add(_flowUsersList);
        }

        private void FilterAndRenderUsers()
        {
            _flowUsersList.Controls.Clear();
            var allUsers = _db.GetAllUsers();
            string query = _txtUserSearch.Text.Trim().ToLowerInvariant();

            var filtered = allUsers.Where(u =>
            {
                bool matchesQuery = string.IsNullOrEmpty(query) ||
                                    (u.Username?.ToLowerInvariant().Contains(query) ?? false) ||
                                    (u.DisplayName?.ToLowerInvariant().Contains(query) ?? false) ||
                                    (u.Email?.ToLowerInvariant().Contains(query) ?? false);

                bool matchesRole = _userRoleFilter == "All" ||
                                   string.Equals(u.Role, _userRoleFilter, StringComparison.OrdinalIgnoreCase);

                return matchesQuery && matchesRole;
            }).ToList();

            _lblUserCount.Text = $"Showing {filtered.Count} of {allUsers.Count} registered user accounts:";

            foreach (var user in filtered)
            {
                _flowUsersList.Controls.Add(CreateUserCard(user));
            }
        }

        private Control CreateUserCard(User user)
        {
            var card = new RoundedCard
            {
                Size = new Size(490, 145),
                Margin = new Padding(0, 0, 14, 14),
                BorderRadius = 12,
                DrawShadow = true,
                HoverElevate = true
            };

            // Avatar circle
            var pnlAvatar = new Panel
            {
                Location = new Point(14, 14),
                Size = new Size(54, 54),
                BackColor = user.IsAdmin ? Theme.Gold : Theme.GreenAccent
            };
            pnlAvatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(user.IsAdmin ? Theme.Gold : Theme.GreenAccent))
                {
                    e.Graphics.FillEllipse(brush, 0, 0, 53, 53);
                }
                string initial = (user.DisplayName ?? user.Username ?? "U").Substring(0, 1).ToUpper();
                TextRenderer.DrawText(e.Graphics, initial, new Font(Theme.PrimaryFontFamily, 16f, FontStyle.Bold),
                    new Rectangle(0, 0, 54, 54), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            var lblName = new Label
            {
                Text = user.DisplayName ?? user.Username,
                Font = Theme.BodyBold,
                ForeColor = Theme.TextPrimary,
                Location = new Point(78, 14),
                Size = new Size(200, 20),
                AutoEllipsis = true
            };

            var lblHandle = new Label
            {
                Text = $"@{user.Username} • {user.Email}",
                Font = Theme.Small,
                ForeColor = Theme.TextSecondary,
                Location = new Point(78, 36),
                Size = new Size(240, 18),
                AutoEllipsis = true
            };

            // Role Badge (Gold for Admin, Green for User)
            var badgeRole = new RatingBadge
            {
                RatingText = user.Role.ToUpper(),
                IsGold = user.IsAdmin,
                Location = new Point(78, 58),
                Size = new Size(70, 22)
            };

            // Status Badge (Active or Inactive)
            var badgeStatus = new RatingBadge
            {
                RatingText = user.IsActive ? "ACTIVE ✓" : "SUSPENDED ⊘",
                IsGold = false,
                Location = new Point(154, 58),
                Size = new Size(95, 22)
            };

            var lblJoined = new Label
            {
                Text = $"Joined: {user.CreatedAt:MMM yyyy}",
                Font = Theme.Small,
                ForeColor = Theme.TextSecondary,
                Location = new Point(255, 60),
                Size = new Size(130, 18)
            };

            // Action Buttons
            var btnEdit = new PillButton
            {
                Text = "Edit ✎",
                Style = PillButtonStyle.PrimaryOutlined,
                Location = new Point(14, 95),
                Size = new Size(80, 32),
                Font = Theme.SmallBold
            };
            btnEdit.Click += (s, e) =>
            {
                using (var dlg = new UserEditDialog(_db, user))
                {
                    if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                    {
                        _showToast?.Invoke($"User @{user.Username} updated successfully!");
                        RefreshAllData();
                    }
                }
            };

            var btnResetPwd = new PillButton
            {
                Text = "Reset Pwd 🔑",
                Style = PillButtonStyle.DarkOutlined,
                Location = new Point(100, 95),
                Size = new Size(110, 32),
                Font = Theme.SmallBold
            };
            btnResetPwd.Click += (s, e) =>
            {
                OnResetPasswordClick(user);
            };

            var btnToggleStatus = new PillButton
            {
                Text = user.IsActive ? "Suspend ⊘" : "Activate ✓",
                Style = user.IsActive ? PillButtonStyle.DarkOutlined : PillButtonStyle.PrimaryFilled,
                Location = new Point(216, 95),
                Size = new Size(100, 32),
                Font = Theme.SmallBold
            };
            btnToggleStatus.Click += (s, e) =>
            {
                user.IsActive = !user.IsActive;
                _db.UpdateUserAdmin(user);
                _showToast?.Invoke($"Account @{user.Username} is now {(user.IsActive ? "Active" : "Suspended")}.");
                FilterAndRenderUsers();
            };

            var btnDelete = new PillButton
            {
                Text = "Delete ✕",
                Style = PillButtonStyle.Destructive,
                Location = new Point(390, 95),
                Size = new Size(85, 32),
                Font = Theme.SmallBold
            };
            btnDelete.Click += (s, e) =>
            {
                if (user.UserId == _auth.CurrentUser?.UserId)
                {
                    MessageBox.Show("You cannot delete your own active administrator account.", "Operation Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var res = MessageBox.Show($"Are you sure you want to permanently delete user account @{user.Username}? This will delete all of their reviews and watchlist entries.", "Confirm Delete User", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (res == DialogResult.Yes)
                {
                    _db.DeleteUser(user.UserId);
                    _showToast?.Invoke($"Deleted account @{user.Username}");
                    RefreshAllData();
                }
            };

            card.Controls.Add(pnlAvatar);
            card.Controls.Add(lblName);
            card.Controls.Add(lblHandle);
            card.Controls.Add(badgeRole);
            card.Controls.Add(badgeStatus);
            card.Controls.Add(lblJoined);
            card.Controls.Add(btnEdit);
            card.Controls.Add(btnResetPwd);
            card.Controls.Add(btnToggleStatus);
            card.Controls.Add(btnDelete);

            return card;
        }

        private void OnAddNewUserClick(object sender, EventArgs e)
        {
            using (var dlg = new UserEditDialog(_db, null))
            {
                if (dlg.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    _showToast?.Invoke($"New user @{dlg.CreatedOrUpdatedUser?.Username} created successfully!");
                    RefreshAllData();
                }
            }
        }

        private void OnResetPasswordClick(User user)
        {
            using (var dlg = new Form())
            {
                dlg.Text = $"Reset Password for @{user.Username}";
                dlg.Size = new Size(380, 240);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = Theme.PageCanvas;

                var lbl = new Label { Text = "Enter new password (min 6 chars):", Location = new Point(24, 20), AutoSize = true, Font = Theme.SmallBold, ForeColor = Theme.TextPrimary };
                var txt = new ModernTextBox { PlaceholderText = "New Password", UseSystemPasswordChar = true, Location = new Point(24, 46), Size = new Size(315, 38) };

                var btnConfirm = new PillButton { Text = "Reset Password", Style = PillButtonStyle.PrimaryFilled, Location = new Point(24, 110), Size = new Size(150, 40) };
                btnConfirm.Click += (s, ev) =>
                {
                    if (string.IsNullOrWhiteSpace(txt.Text) || txt.Text.Length < 6)
                    {
                        MessageBox.Show("Password must be at least 6 characters.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    _db.ResetUserPassword(user.UserId, txt.Text);
                    _showToast?.Invoke($"Password for @{user.Username} was reset successfully!");
                    dlg.Close();
                };

                dlg.Controls.Add(lbl);
                dlg.Controls.Add(txt);
                dlg.Controls.Add(btnConfirm);
                dlg.ShowDialog(FindForm());
            }
        }

        #endregion

        #region SubTab 2: Catalog Governance & Import

        private void BuildCatalogTab()
        {
            _pnlCatalogContainer = new Panel
            {
                Location = new Point(32, 260),
                Size = new Size(1050, 900),
                AutoSize = true,
                BackColor = Color.Transparent,
                Visible = false
            };

            var pnlImportBar = new RoundedCard
            {
                Location = new Point(0, 0),
                Size = new Size(1010, 80),
                BorderRadius = 12,
                DrawShadow = true
            };

            var lblImportHeading = new Label
            {
                Text = "IMPORT MOVIE OR TV SERIES BY IMDB ID",
                Font = Theme.SmallBold,
                ForeColor = Theme.StarbucksGreen,
                Location = new Point(14, 12),
                AutoSize = true
            };

            _txtImportImdbId = new ModernTextBox
            {
                PlaceholderText = "Enter IMDb ID (e.g. tt0137523 for Fight Club, tt0109830 for Forrest Gump)...",
                Location = new Point(14, 34),
                Size = new Size(580, 38)
            };

            _btnImportMovie = new PillButton
            {
                Text = "Import from OMDb ★",
                Style = PillButtonStyle.PrimaryFilled,
                Location = new Point(610, 34),
                Size = new Size(180, 38)
            };
            _btnImportMovie.Click += OnImportMovieClick;

            pnlImportBar.Controls.Add(lblImportHeading);
            pnlImportBar.Controls.Add(_txtImportImdbId);
            pnlImportBar.Controls.Add(_btnImportMovie);

            _lblCatalogCount = new Label
            {
                Text = "Current Catalog Titles:",
                Font = Theme.BodyBold,
                ForeColor = Theme.StarbucksGreen,
                Location = new Point(4, 95),
                AutoSize = true
            };

            _flowCatalogList = new FlowLayoutPanel
            {
                Location = new Point(0, 125),
                Size = new Size(1010, 800),
                AutoSize = true,
                BackColor = Color.Transparent,
                WrapContents = true
            };

            _pnlCatalogContainer.Controls.Add(pnlImportBar);
            _pnlCatalogContainer.Controls.Add(_lblCatalogCount);
            _pnlCatalogContainer.Controls.Add(_flowCatalogList);
        }

        private async void OnImportMovieClick(object sender, EventArgs e)
        {
            string imdbId = _txtImportImdbId.Text.Trim();
            if (string.IsNullOrWhiteSpace(imdbId))
            {
                MessageBox.Show("Please enter an IMDb ID (e.g. tt0137523).", "Catalog Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _btnImportMovie.Text = "Importing...";
            var media = await _omdb.GetDetailByIdAsync(imdbId);
            _btnImportMovie.Text = "Import from OMDb ★";

            if (media != null)
            {
                _db.GetOrCreateMediaItem(media);
                _showToast?.Invoke($"Imported '{media.Title}' ({media.Year}) into catalog!");
                _txtImportImdbId.Text = "";
                RefreshAllData();
            }
            else
            {
                MessageBox.Show($"Could not find title with IMDb ID '{imdbId}'. Please check the ID.", "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RenderCatalog()
        {
            _flowCatalogList.Controls.Clear();
            var items = _db.GetCuratedMedia();

            _lblCatalogCount.Text = $"Showing {items.Count} titles currently in the RateMe database catalog:";

            foreach (var item in items)
            {
                var card = new RoundedCard
                {
                    Size = new Size(490, 120),
                    Margin = new Padding(0, 0, 14, 14),
                    BorderRadius = 12,
                    DrawShadow = true,
                    HoverElevate = true
                };

                var pic = new PictureBox
                {
                    Location = new Point(12, 12),
                    Size = new Size(65, 96),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.FromArgb(20, 38, 33)
                };
                pic.Image = ImageCache.GetPlaceholder(item.Title, 65, 96);
                LoadPosterAsync(pic, item.PosterUrl, item.Title);

                var lblTitle = new Label
                {
                    Text = item.Title,
                    Font = Theme.BodyBold,
                    ForeColor = Theme.TextPrimary,
                    Location = new Point(88, 12),
                    Size = new Size(290, 20),
                    AutoEllipsis = true
                };

                var lblMeta = new Label
                {
                    Text = $"{item.Year} • {item.Type} • {item.Genre}",
                    Font = Theme.Small,
                    ForeColor = Theme.TextSecondary,
                    Location = new Point(88, 34),
                    Size = new Size(290, 18),
                    AutoEllipsis = true
                };

                var badgeScore = new RatingBadge
                {
                    RatingText = $"{item.DisplayScore} ★",
                    IsGold = true,
                    Location = new Point(88, 58),
                    Size = new Size(58, 22)
                };

                var btnView = new PillButton
                {
                    Text = "View & Rate",
                    Style = PillButtonStyle.PrimaryOutlined,
                    Location = new Point(155, 58),
                    Size = new Size(100, 24),
                    Font = Theme.SmallBold
                };
                btnView.Click += (s, e) => _openMediaDetail?.Invoke(item);

                var btnDelete = new PillButton
                {
                    Text = "Delete ✕",
                    Style = PillButtonStyle.Destructive,
                    Location = new Point(390, 12),
                    Size = new Size(85, 30),
                    Font = Theme.SmallBold
                };
                btnDelete.Click += (s, e) =>
                {
                    var res = MessageBox.Show($"Remove '{item.Title}' from the database catalog? Associated user ratings will also be removed.", "Confirm Delete Title", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (res == DialogResult.Yes)
                    {
                        _db.DeleteMediaItem(item.MediaId);
                        _showToast?.Invoke($"Removed '{item.Title}' from catalog.");
                        RefreshAllData();
                    }
                };

                card.Controls.Add(pic);
                card.Controls.Add(lblTitle);
                card.Controls.Add(lblMeta);
                card.Controls.Add(badgeScore);
                card.Controls.Add(btnView);
                card.Controls.Add(btnDelete);

                _flowCatalogList.Controls.Add(card);
            }
        }

        #endregion

        #region SubTab 3: Review Moderation

        private void BuildReviewsTab()
        {
            _pnlReviewsContainer = new Panel
            {
                Location = new Point(32, 260),
                Size = new Size(1050, 900),
                AutoSize = true,
                BackColor = Color.Transparent,
                Visible = false
            };

            _lblReviewsCount = new Label
            {
                Text = "Recent Community Reviews & Ratings for Moderation:",
                Font = Theme.BodyBold,
                ForeColor = Theme.StarbucksGreen,
                Location = new Point(4, 0),
                AutoSize = true
            };

            _flowReviewsList = new FlowLayoutPanel
            {
                Location = new Point(0, 30),
                Size = new Size(1010, 800),
                AutoSize = true,
                BackColor = Color.Transparent,
                WrapContents = true
            };

            _pnlReviewsContainer.Controls.Add(_lblReviewsCount);
            _pnlReviewsContainer.Controls.Add(_flowReviewsList);
        }

        private void RenderReviews()
        {
            _flowReviewsList.Controls.Clear();
            var ratings = _db.GetAllRatingsWithDetails();

            _lblReviewsCount.Text = $"Showing {ratings.Count} total community reviews and ratings across all titles:";

            if (ratings.Count == 0)
            {
                var lblEmpty = new Label
                {
                    Text = "No community reviews submitted yet.",
                    Font = Theme.Body,
                    ForeColor = Theme.TextSecondary,
                    Location = new Point(10, 10),
                    AutoSize = true
                };
                _flowReviewsList.Controls.Add(lblEmpty);
                return;
            }

            foreach (var r in ratings)
            {
                var card = new RoundedCard
                {
                    Size = new Size(1000, 95),
                    Margin = new Padding(0, 0, 0, 12),
                    BorderRadius = 10,
                    DrawShadow = false
                };

                var lblAuthor = new Label
                {
                    Text = $"{r.UserDisplayName ?? r.Username} on '{r.MediaTitle ?? "Film"}'",
                    Font = Theme.BodyBold,
                    ForeColor = Theme.StarbucksGreen,
                    Location = new Point(14, 12),
                    Size = new Size(500, 20),
                    AutoEllipsis = true
                };

                var badge = new RatingBadge
                {
                    RatingText = $"{r.Score:0.0} ★",
                    IsGold = true,
                    Location = new Point(530, 10),
                    Size = new Size(58, 22)
                };

                var lblDate = new Label
                {
                    Text = r.CreatedAt.ToString("MMM dd, yyyy HH:mm"),
                    Font = Theme.Small,
                    ForeColor = Theme.TextSecondary,
                    Location = new Point(620, 14),
                    Size = new Size(180, 18)
                };

                var btnDeleteReview = new PillButton
                {
                    Text = "Remove Review 🗑",
                    Style = PillButtonStyle.Destructive,
                    Location = new Point(840, 10),
                    Size = new Size(140, 30),
                    Font = Theme.SmallBold
                };
                btnDeleteReview.Click += (s, e) =>
                {
                    var res = MessageBox.Show($"Delete this review by @{r.Username}?", "Confirm Delete Review", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (res == DialogResult.Yes)
                    {
                        _db.DeleteRating(r.RatingId);
                        _showToast?.Invoke("Review deleted successfully.");
                        RefreshAllData();
                    }
                };

                var lblReview = new Label
                {
                    Text = string.IsNullOrWhiteSpace(r.ReviewText) ? "Rated without a written review." : r.ReviewText,
                    Font = Theme.Body,
                    ForeColor = Theme.TextPrimary,
                    Location = new Point(14, 38),
                    Size = new Size(960, 48),
                    AutoEllipsis = true
                };

                card.Controls.Add(lblAuthor);
                card.Controls.Add(badge);
                card.Controls.Add(lblDate);
                card.Controls.Add(btnDeleteReview);
                card.Controls.Add(lblReview);

                _flowReviewsList.Controls.Add(card);
            }
        }

        #endregion

        private void SwitchSubTab(string tab)
        {
            _activeSubTab = tab;

            _btnTabUsers.Style = tab == "Users" ? PillButtonStyle.PrimaryFilled : PillButtonStyle.PrimaryOutlined;
            _btnTabCatalog.Style = tab == "Catalog" ? PillButtonStyle.PrimaryFilled : PillButtonStyle.PrimaryOutlined;
            _btnTabReviews.Style = tab == "Reviews" ? PillButtonStyle.PrimaryFilled : PillButtonStyle.PrimaryOutlined;

            _pnlUsersContainer.Visible = tab == "Users";
            _pnlCatalogContainer.Visible = tab == "Catalog";
            _pnlReviewsContainer.Visible = tab == "Reviews";
        }

        public void RefreshAllData()
        {
            var stats = _db.GetAdminStats();
            _lblStatUsers.Text = $"{stats.TotalUsers} ({stats.ActiveUsers} active)";
            _lblStatMedia.Text = stats.TotalMedia.ToString();
            _lblStatRatings.Text = stats.TotalRatings.ToString();
            _lblStatDb.Text = stats.IsUsingSqlServer ? "SQL Server ✓" : "Local Storage";

            FilterAndRenderUsers();
            RenderCatalog();
            RenderReviews();
        }

        private async void LoadPosterAsync(PictureBox pb, string url, string title)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var img = await ImageCache.GetImageAsync(url, title, 65, 96);
                if (img != null && !pb.IsDisposed)
                {
                    pb.Image = img;
                }
            }
        }
    }
}

