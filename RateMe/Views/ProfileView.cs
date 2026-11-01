using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RateMe.Models;
using RateMe.Services;
using RateMe.UI;
using RateMe.UI.Controls;

namespace RateMe.Views
{
    public class ProfileView : UserControl
    {
        private readonly DatabaseService _db;
        private readonly AuthService _auth;
        private readonly Action<string> _showToast;
        private readonly Action<MediaItem> _openMediaDetail;
        private readonly Action _openAuth;

        private FlowLayoutPanel _flowRatings;
        private Label _lblEmpty;
        private Label _lblName;
        private Label _lblHandle;
        private Label _lblBio;
        private Label _lblStatRatings;
        private Label _lblStatAvg;
        private Label _lblStatWatchlist;

        public ProfileView(DatabaseService db, AuthService auth, Action<string> showToast, Action<MediaItem> openMediaDetail, Action openAuth)
        {
            _db = db;
            _auth = auth;
            _showToast = showToast;
            _openMediaDetail = openMediaDetail;
            _openAuth = openAuth;

            InitializeComponent();
            LoadUserData();
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

            // 1. Profile Header Card
            var cardProfile = new RoundedCard
            {
                Location = new Point(32, 20),
                Size = new Size(910, 150),
                BorderRadius = 14,
                DrawShadow = true
            };

            // Avatar circle placeholder
            var pnlAvatar = new Panel
            {
                Location = new Point(24, 20),
                Size = new Size(100, 100),
                BackColor = Theme.StarbucksGreen
            };
            pnlAvatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Theme.GreenAccent))
                {
                    e.Graphics.FillEllipse(brush, 2, 2, 96, 96);
                }
                string initial = _auth.CurrentUser?.DisplayName?.Substring(0, 1).ToUpper() ?? "U";
                TextRenderer.DrawText(e.Graphics, initial, new Font(Theme.PrimaryFontFamily, 32f, FontStyle.Bold),
                    new Rectangle(0, 0, 100, 100), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            _lblName = new Label
            {
                Text = "Guest User",
                Font = Theme.HeaderDisplay,
                ForeColor = Theme.StarbucksGreen,
                Location = new Point(140, 22),
                Size = new Size(500, 32),
                AutoEllipsis = true
            };

            _lblHandle = new Label
            {
                Text = "@guest  •  Member since 2026",
                Font = Theme.Small,
                ForeColor = Theme.TextSecondary,
                Location = new Point(142, 56),
                Size = new Size(500, 20)
            };

            _lblBio = new Label
            {
                Text = "Film enthusiast, coffee lover, and cinephile.",
                Font = Theme.Body,
                ForeColor = Theme.TextPrimary,
                Location = new Point(142, 80),
                Size = new Size(580, 42),
                AutoEllipsis = true
            };

            var btnEdit = new PillButton
            {
                Text = "Edit Profile",
                Style = PillButtonStyle.PrimaryOutlined,
                Location = new Point(760, 50),
                Size = new Size(120, 36)
            };
            btnEdit.Click += OnEditProfileClick;

            cardProfile.Controls.Add(pnlAvatar);
            cardProfile.Controls.Add(_lblName);
            cardProfile.Controls.Add(_lblHandle);
            cardProfile.Controls.Add(_lblBio);
            cardProfile.Controls.Add(btnEdit);

            // 2. Stats Row
            var pnlStats = new Panel
            {
                Location = new Point(32, 185),
                Size = new Size(910, 85),
                BackColor = Color.Transparent
            };

            var stat1 = CreateStatBox(0, "TOTAL RATED", out _lblStatRatings);
            var stat2 = CreateStatBox(310, "AVERAGE SCORE", out _lblStatAvg);
            var stat3 = CreateStatBox(620, "WATCHLIST SAVED", out _lblStatWatchlist);

            pnlStats.Controls.Add(stat1);
            pnlStats.Controls.Add(stat2);
            pnlStats.Controls.Add(stat3);

            // 3. Ratings History List
            var lblHistoryTitle = new Label
            {
                Text = "MY RATINGS & WRITTEN REVIEWS",
                Font = Theme.SmallBold,
                ForeColor = Theme.StarbucksGreen,
                Location = new Point(36, 285),
                AutoSize = true
            };

            _lblEmpty = new Label
            {
                Text = "You haven't rated any titles yet. Explore movies or series to rate them!",
                Font = Theme.Body,
                ForeColor = Theme.TextSecondary,
                Location = new Point(36, 315),
                Size = new Size(800, 30),
                Visible = false
            };

            _flowRatings = new FlowLayoutPanel
            {
                Location = new Point(28, 315),
                Size = new Size(920, 800),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent,
                WrapContents = true
            };

            scrollHost.Controls.Add(cardProfile);
            scrollHost.Controls.Add(pnlStats);
            scrollHost.Controls.Add(lblHistoryTitle);
            scrollHost.Controls.Add(_lblEmpty);
            scrollHost.Controls.Add(_flowRatings);

            Controls.Add(scrollHost);
        }

        private Control CreateStatBox(int x, string label, out Label outValue)
        {
            var card = new RoundedCard
            {
                Location = new Point(x, 0),
                Size = new Size(290, 80),
                BorderRadius = 12,
                DrawShadow = true
            };

            var lblHeader = new Label
            {
                Text = label,
                Font = Theme.SmallBold,
                ForeColor = Theme.TextSecondary,
                Location = new Point(16, 12),
                AutoSize = true
            };

            var lblVal = new Label
            {
                Text = "0",
                Font = new Font(Theme.SerifFontFamily, 18f, FontStyle.Bold),
                ForeColor = Theme.Gold,
                Location = new Point(16, 36),
                Size = new Size(240, 32)
            };

            outValue = lblVal;
            card.Controls.Add(lblHeader);
            card.Controls.Add(lblVal);
            return card;
        }

        public void LoadUserData()
        {
            if (!_auth.IsLoggedIn)
            {
                _lblName.Text = "Guest Cinephile";
                _lblHandle.Text = "Not signed in";
                _lblBio.Text = "Sign in to track your personal movie ratings and build a watchlist.";
                _lblStatRatings.Text = "0";
                _lblStatAvg.Text = "-- ★";
                _lblStatWatchlist.Text = "0";
                _flowRatings.Controls.Clear();
                _lblEmpty.Text = "Please Sign In to view your profile ratings and statistics.";
                _lblEmpty.Visible = true;
                return;
            }

            var user = _auth.CurrentUser;
            _lblName.Text = user.DisplayName ?? user.Username;
            _lblHandle.Text = $"@{user.Username}  •  {user.Role}  •  Joined {user.CreatedAt:MMM yyyy}";
            _lblBio.Text = string.IsNullOrWhiteSpace(user.Bio) ? "No bio written yet. Click 'Edit Profile' to add your bio!" : user.Bio;

            var ratings = _db.GetUserRatings(user.UserId);
            var watchlist = _db.GetWatchlistForUser(user.UserId);

            _lblStatRatings.Text = ratings.Count.ToString();
            _lblStatAvg.Text = ratings.Count > 0 ? $"{ratings.Average(r => r.Score):0.0} ★" : "-- ★";
            _lblStatWatchlist.Text = watchlist.Count.ToString();

            _flowRatings.Controls.Clear();
            if (ratings.Count == 0)
            {
                _lblEmpty.Text = "You haven't rated any titles yet. Explore movies or series to rate them!";
                _lblEmpty.Visible = true;
                return;
            }

            _lblEmpty.Visible = false;
            foreach (var r in ratings)
            {
                _flowRatings.Controls.Add(CreateRatingCard(r));
            }
        }

        private Control CreateRatingCard(Rating r)
        {
            var card = new RoundedCard
            {
                Size = new Size(440, 120),
                Margin = new Padding(8, 8, 8, 14),
                BorderRadius = 12,
                HoverElevate = true,
                DrawShadow = true,
                Cursor = Cursors.Hand
            };

            var pic = new PictureBox
            {
                Location = new Point(12, 12),
                Size = new Size(65, 96),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(20, 38, 33),
                Cursor = Cursors.Hand
            };
            pic.Image = ImageCache.GetPlaceholder(r.MediaTitle, 65, 96);
            LoadPosterAsync(pic, r.MediaPosterUrl, r.MediaTitle);

            var lblTitle = new Label
            {
                Text = r.MediaTitle ?? "Film Title",
                Font = Theme.BodyBold,
                ForeColor = Theme.TextPrimary,
                Location = new Point(88, 12),
                Size = new Size(250, 22),
                AutoEllipsis = true,
                Cursor = Cursors.Hand
            };

            var badge = new RatingBadge
            {
                RatingText = $"{r.Score:0.0} ★",
                IsGold = true,
                Location = new Point(88, 38),
                Size = new Size(58, 22),
                Cursor = Cursors.Hand
            };

            var lblDate = new Label
            {
                Text = r.UpdatedAt.ToString("MMM dd, yyyy"),
                Font = Theme.Small,
                ForeColor = Theme.TextSecondary,
                Location = new Point(155, 41),
                Size = new Size(120, 18),
                Cursor = Cursors.Hand
            };

            var lblReview = new Label
            {
                Text = string.IsNullOrWhiteSpace(r.ReviewText) ? "Rated without a written review." : r.ReviewText,
                Font = Theme.Small,
                ForeColor = Theme.TextSecondary,
                Location = new Point(88, 66),
                Size = new Size(335, 42),
                AutoEllipsis = true,
                Cursor = Cursors.Hand
            };

            card.Click += (s, e) => OpenMediaDetailForRating(r);
            pic.Click += (s, e) => OpenMediaDetailForRating(r);
            lblTitle.Click += (s, e) => OpenMediaDetailForRating(r);
            badge.Click += (s, e) => OpenMediaDetailForRating(r);
            lblReview.Click += (s, e) => OpenMediaDetailForRating(r);

            card.Controls.Add(pic);
            card.Controls.Add(lblTitle);
            card.Controls.Add(badge);
            card.Controls.Add(lblDate);
            card.Controls.Add(lblReview);

            return card;
        }

        private void OpenMediaDetailForRating(Rating r)
        {
            var media = _db.GetCuratedMedia().FirstOrDefault(m => m.MediaId == r.MediaId) 
                        ?? new MediaItem { MediaId = r.MediaId, Title = r.MediaTitle, ImdbId = r.MediaImdbId, PosterUrl = r.MediaPosterUrl };
            _openMediaDetail?.Invoke(media);
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

        private void OnEditProfileClick(object sender, EventArgs e)
        {
            if (!_auth.IsLoggedIn)
            {
                _openAuth?.Invoke();
                return;
            }

            using (var dlg = new Form())
            {
                dlg.Text = "Edit Profile";
                dlg.Size = new Size(400, 320);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.BackColor = Theme.PageCanvas;

                var lblName = new Label { Text = "Display Name:", Location = new Point(24, 20), AutoSize = true, Font = Theme.SmallBold, ForeColor = Theme.TextPrimary };
                var txtName = new ModernTextBox { Text = _auth.CurrentUser.DisplayName, Location = new Point(24, 42), Size = new Size(336, 38) };

                var lblBio = new Label { Text = "Bio:", Location = new Point(24, 90), AutoSize = true, Font = Theme.SmallBold, ForeColor = Theme.TextPrimary };
                var txtBio = new ModernTextBox { Text = _auth.CurrentUser.Bio, Multiline = true, Location = new Point(24, 112), Size = new Size(336, 75) };

                var btnSave = new PillButton { Text = "Save Changes", Style = PillButtonStyle.PrimaryFilled, Location = new Point(24, 205), Size = new Size(160, 40) };
                btnSave.Click += (s, ev) =>
                {
                    _auth.UpdateProfile(txtName.Text.Trim(), txtBio.Text.Trim(), out _);
                    _showToast?.Invoke("Profile updated successfully!");
                    dlg.Close();
                    LoadUserData();
                };

                dlg.Controls.Add(lblName);
                dlg.Controls.Add(txtName);
                dlg.Controls.Add(lblBio);
                dlg.Controls.Add(txtBio);
                dlg.Controls.Add(btnSave);

                dlg.ShowDialog(this);
            }
        }
    }
}

