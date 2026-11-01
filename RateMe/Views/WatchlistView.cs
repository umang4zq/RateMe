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
    public class WatchlistView : UserControl
    {
        private readonly DatabaseService _db;
        private readonly AuthService _auth;
        private readonly Action<string> _showToast;
        private readonly Action<MediaItem> _openMediaDetail;

        private FlowLayoutPanel _flowWatchlist;
        private Label _lblEmpty;
        private PillButton _btnFilterAll;
        private PillButton _btnFilterPlan;
        private PillButton _btnFilterWatching;
        private PillButton _btnFilterCompleted;
        private string _activeFilter = "All";

        public WatchlistView(DatabaseService db, AuthService auth, Action<string> showToast, Action<MediaItem> openMediaDetail)
        {
            _db = db;
            _auth = auth;
            _showToast = showToast;
            _openMediaDetail = openMediaDetail;

            InitializeComponent();
            LoadWatchlistItems();
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

            // Header Banner
            var pnlBanner = new RoundedCard
            {
                Location = new Point(32, 20),
                Size = new Size(910, 100),
                BorderRadius = 14,
                DrawShadow = true
            };

            var lblTitle = new Label
            {
                Text = "MY WATCHLIST",
                Font = Theme.HeaderDisplay,
                ForeColor = Theme.StarbucksGreen,
                Location = new Point(20, 14),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = "Track titles you plan to watch, are currently watching, or have completed.",
                Font = Theme.Small,
                ForeColor = Theme.TextSecondary,
                Location = new Point(22, 50),
                AutoSize = true
            };

            // Filter pills
            _btnFilterAll = new PillButton
            {
                Text = "All",
                Style = PillButtonStyle.PrimaryFilled,
                Location = new Point(540, 32),
                Size = new Size(60, 34)
            };
            _btnFilterAll.Click += (s, e) => SetFilter("All");

            _btnFilterPlan = new PillButton
            {
                Text = "Plan to Watch",
                Style = PillButtonStyle.PrimaryOutlined,
                Location = new Point(608, 32),
                Size = new Size(100, 34)
            };
            _btnFilterPlan.Click += (s, e) => SetFilter("PlanToWatch");

            _btnFilterWatching = new PillButton
            {
                Text = "Watching",
                Style = PillButtonStyle.PrimaryOutlined,
                Location = new Point(716, 32),
                Size = new Size(85, 34)
            };
            _btnFilterWatching.Click += (s, e) => SetFilter("Watching");

            _btnFilterCompleted = new PillButton
            {
                Text = "Completed",
                Style = PillButtonStyle.PrimaryOutlined,
                Location = new Point(808, 32),
                Size = new Size(90, 34)
            };
            _btnFilterCompleted.Click += (s, e) => SetFilter("Completed");

            pnlBanner.Controls.Add(lblTitle);
            pnlBanner.Controls.Add(lblSubtitle);
            pnlBanner.Controls.Add(_btnFilterAll);
            pnlBanner.Controls.Add(_btnFilterPlan);
            pnlBanner.Controls.Add(_btnFilterWatching);
            pnlBanner.Controls.Add(_btnFilterCompleted);

            _lblEmpty = new Label
            {
                Text = "Your watchlist is empty. Explore movies and click '+ Add to Watchlist' to save titles here!",
                Font = Theme.Body,
                ForeColor = Theme.TextSecondary,
                Location = new Point(36, 140),
                Size = new Size(800, 40),
                Visible = false
            };

            _flowWatchlist = new FlowLayoutPanel
            {
                Location = new Point(28, 140),
                Size = new Size(920, 800),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent,
                WrapContents = true
            };

            scrollHost.Controls.Add(pnlBanner);
            scrollHost.Controls.Add(_lblEmpty);
            scrollHost.Controls.Add(_flowWatchlist);

            Controls.Add(scrollHost);
        }

        private void SetFilter(string filter)
        {
            _activeFilter = filter;
            _btnFilterAll.Style = filter == "All" ? PillButtonStyle.PrimaryFilled : PillButtonStyle.PrimaryOutlined;
            _btnFilterPlan.Style = filter == "PlanToWatch" ? PillButtonStyle.PrimaryFilled : PillButtonStyle.PrimaryOutlined;
            _btnFilterWatching.Style = filter == "Watching" ? PillButtonStyle.PrimaryFilled : PillButtonStyle.PrimaryOutlined;
            _btnFilterCompleted.Style = filter == "Completed" ? PillButtonStyle.PrimaryFilled : PillButtonStyle.PrimaryOutlined;

            LoadWatchlistItems();
        }

        public void LoadWatchlistItems()
        {
            _flowWatchlist.Controls.Clear();

            if (!_auth.IsLoggedIn)
            {
                _lblEmpty.Text = "Please Sign In to view and manage your personal Watchlist.";
                _lblEmpty.Visible = true;
                return;
            }

            var items = _db.GetWatchlistForUser(_auth.CurrentUser.UserId);
            if (_activeFilter != "All")
            {
                items = items.Where(w => string.Equals(w.Status, _activeFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (items.Count == 0)
            {
                _lblEmpty.Text = $"No titles in this section ({_activeFilter}). Explore titles to add them!";
                _lblEmpty.Visible = true;
                return;
            }

            _lblEmpty.Visible = false;

            foreach (var item in items)
            {
                if (item.Media != null)
                {
                    _flowWatchlist.Controls.Add(CreateWatchlistCard(item));
                }
            }
        }

        private Control CreateWatchlistCard(WatchlistItem item)
        {
            var media = item.Media;

            var card = new RoundedCard
            {
                Size = new Size(440, 130),
                Margin = new Padding(8, 8, 8, 14),
                BorderRadius = 12,
                HoverElevate = true,
                DrawShadow = true,
                Cursor = Cursors.Hand
            };

            var pic = new PictureBox
            {
                Location = new Point(12, 12),
                Size = new Size(72, 106),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(20, 38, 33),
                Cursor = Cursors.Hand
            };
            pic.Image = ImageCache.GetPlaceholder(media.Title, 72, 106);
            LoadPosterAsync(pic, media.PosterUrl, media.Title);

            var lblTitle = new Label
            {
                Text = media.Title,
                Font = Theme.BodyBold,
                ForeColor = Theme.TextPrimary,
                Location = new Point(96, 12),
                Size = new Size(240, 22),
                AutoEllipsis = true,
                Cursor = Cursors.Hand
            };

            var lblMeta = new Label
            {
                Text = $"{media.Year}  •  {media.Type}  •  {media.Genre ?? "Cinema"}",
                Font = Theme.Small,
                ForeColor = Theme.TextSecondary,
                Location = new Point(96, 36),
                Size = new Size(240, 18),
                AutoEllipsis = true,
                Cursor = Cursors.Hand
            };

            var badge = new RatingBadge
            {
                RatingText = $"{media.DisplayScore} ★",
                IsGold = true,
                Location = new Point(96, 62),
                Size = new Size(58, 22),
                Cursor = Cursors.Hand
            };

            var btnStatus = new PillButton
            {
                Text = item.Status == "Completed" ? "Completed ✓" : (item.Status == "Watching" ? "Watching ▶" : "Plan to Watch"),
                Style = PillButtonStyle.PrimaryOutlined,
                Location = new Point(164, 62),
                Size = new Size(115, 24),
                Font = Theme.SmallBold
            };
            btnStatus.Click += (s, e) =>
            {
                // Cycle status: PlanToWatch -> Watching -> Completed -> PlanToWatch
                string nextStatus = item.Status == "PlanToWatch" ? "Watching" : (item.Status == "Watching" ? "Completed" : "PlanToWatch");
                _db.AddToWatchlist(_auth.CurrentUser.UserId, media.MediaId, nextStatus);
                _showToast?.Invoke($"Status updated to {nextStatus}");
                LoadWatchlistItems();
            };

            var btnRemove = new PillButton
            {
                Text = "Remove ✕",
                Style = PillButtonStyle.Destructive,
                Location = new Point(345, 12),
                Size = new Size(80, 28),
                Font = Theme.Small
            };
            btnRemove.Click += (s, e) =>
            {
                _db.RemoveFromWatchlist(_auth.CurrentUser.UserId, media.MediaId);
                _showToast?.Invoke($"Removed {media.Title} from Watchlist");
                LoadWatchlistItems();
            };

            Action openAction = () => _openMediaDetail?.Invoke(media);
            card.Click += (s, e) => openAction();
            pic.Click += (s, e) => openAction();
            lblTitle.Click += (s, e) => openAction();
            lblMeta.Click += (s, e) => openAction();
            badge.Click += (s, e) => openAction();

            card.Controls.Add(pic);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblMeta);
            card.Controls.Add(badge);
            card.Controls.Add(btnStatus);
            card.Controls.Add(btnRemove);

            return card;
        }

        private async void LoadPosterAsync(PictureBox pb, string url, string title)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var img = await ImageCache.GetImageAsync(url, title, 72, 106);
                if (img != null && !pb.IsDisposed)
                {
                    pb.Image = img;
                }
            }
        }
    }
}

