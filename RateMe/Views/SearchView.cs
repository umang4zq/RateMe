using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RateMe.Models;
using RateMe.Services;
using RateMe.UI;
using RateMe.UI.Controls;

namespace RateMe.Views
{
    public class SearchView : UserControl
    {
        private readonly DatabaseService _db;
        private readonly AuthService _auth;
        private readonly OmdbService _omdb;
        private readonly Action<string> _showToast;
        private readonly Action<MediaItem> _openMediaDetail;

        private ModernTextBox _txtSearch;
        private PillButton _btnSearch;
        private PillButton _btnTypeAll;
        private PillButton _btnTypeMovie;
        private PillButton _btnTypeSeries;
        private FlowLayoutPanel _flowResults;
        private Label _lblStatus;
        private readonly Timer _debounceTimer = new Timer();
        private string _selectedType = null; // null = all

        public SearchView(DatabaseService db, AuthService auth, OmdbService omdb, Action<string> showToast, Action<MediaItem> openMediaDetail)
        {
            _db = db;
            _auth = auth;
            _omdb = omdb;
            _showToast = showToast;
            _openMediaDetail = openMediaDetail;

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

            // Search Header Bar
            var pnlHeader = new RoundedCard
            {
                Location = new Point(32, 20),
                Size = new Size(910, 110),
                BorderRadius = 14,
                DrawShadow = true
            };

            var lblSearchHeading = new Label
            {
                Text = "SEARCH MOVIES & TV SERIES",
                Font = Theme.SmallBold,
                ForeColor = Theme.StarbucksGreen,
                Location = new Point(18, 12),
                AutoSize = true
            };

            _txtSearch = new ModernTextBox
            {
                PlaceholderText = "Search by movie title, series, or franchise (e.g. Batman, Avatar, Stranger Things)...",
                Location = new Point(18, 36),
                Size = new Size(580, 40)
            };
            _txtSearch.TextChangedCustom += (s, e) =>
            {
                _debounceTimer.Stop();
                _debounceTimer.Start();
            };

            _btnSearch = new PillButton
            {
                Text = "Search",
                Style = PillButtonStyle.PrimaryFilled,
                Location = new Point(610, 36),
                Size = new Size(110, 40)
            };
            _btnSearch.Click += (s, e) => ExecuteSearch();

            // Type filter pills
            _btnTypeAll = new PillButton
            {
                Text = "All",
                Style = PillButtonStyle.PrimaryFilled,
                Location = new Point(730, 40),
                Size = new Size(50, 32)
            };
            _btnTypeAll.Click += (s, e) => SetTypeFilter(null);

            _btnTypeMovie = new PillButton
            {
                Text = "Movies",
                Style = PillButtonStyle.PrimaryOutlined,
                Location = new Point(786, 40),
                Size = new Size(68, 32)
            };
            _btnTypeMovie.Click += (s, e) => SetTypeFilter("movie");

            _btnTypeSeries = new PillButton
            {
                Text = "Series",
                Style = PillButtonStyle.PrimaryOutlined,
                Location = new Point(860, 40),
                Size = new Size(68, 32)
            };
            _btnTypeSeries.Click += (s, e) => SetTypeFilter("series");

            pnlHeader.Controls.Add(lblSearchHeading);
            pnlHeader.Controls.Add(_txtSearch);
            pnlHeader.Controls.Add(_btnSearch);
            pnlHeader.Controls.Add(_btnTypeAll);
            pnlHeader.Controls.Add(_btnTypeMovie);
            pnlHeader.Controls.Add(_btnTypeSeries);

            // Status label
            _lblStatus = new Label
            {
                Text = "Type a title above to search the live OMDb movie & series catalog.",
                Font = Theme.Body,
                ForeColor = Theme.TextSecondary,
                Location = new Point(34, 140),
                Size = new Size(900, 26)
            };

            // Results Flow Layout
            _flowResults = new FlowLayoutPanel
            {
                Location = new Point(28, 175),
                Size = new Size(920, 800),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent,
                WrapContents = true
            };

            scrollHost.Controls.Add(pnlHeader);
            scrollHost.Controls.Add(_lblStatus);
            scrollHost.Controls.Add(_flowResults);

            Controls.Add(scrollHost);

            _debounceTimer.Interval = 500;
            _debounceTimer.Tick += (s, e) =>
            {
                _debounceTimer.Stop();
                ExecuteSearch();
            };
        }

        private void SetTypeFilter(string type)
        {
            _selectedType = type;
            _btnTypeAll.Style = type == null ? PillButtonStyle.PrimaryFilled : PillButtonStyle.PrimaryOutlined;
            _btnTypeMovie.Style = type == "movie" ? PillButtonStyle.PrimaryFilled : PillButtonStyle.PrimaryOutlined;
            _btnTypeSeries.Style = type == "series" ? PillButtonStyle.PrimaryFilled : PillButtonStyle.PrimaryOutlined;

            ExecuteSearch();
        }

        private async void ExecuteSearch()
        {
            string query = _txtSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                _flowResults.Controls.Clear();
                _lblStatus.Text = "Type a title above to search the live OMDb movie & series catalog.";
                return;
            }

            _lblStatus.Text = $"Searching for \"{query}\"...";

            var results = await _omdb.SearchAsync(query, _selectedType);

            _flowResults.Controls.Clear();
            if (results.Count == 0)
            {
                _lblStatus.Text = $"No results found for \"{query}\". Try another keyword.";
                return;
            }

            _lblStatus.Text = $"Found {results.Count} results for \"{query}\":";

            foreach (var media in results)
            {
                var card = CreateSearchResultCard(media);
                _flowResults.Controls.Add(card);
            }
        }

        private Control CreateSearchResultCard(MediaItem media)
        {
            var card = new RoundedCard
            {
                Size = new Size(210, 315),
                Margin = new Padding(8, 8, 8, 16),
                BorderRadius = 12,
                HoverElevate = true,
                DrawShadow = true,
                Cursor = Cursors.Hand
            };

            var pic = new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(190, 195),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(20, 38, 33),
                Cursor = Cursors.Hand
            };
            pic.Image = ImageCache.GetPlaceholder(media.Title, 190, 195);
            LoadPosterAsync(pic, media.PosterUrl, media.Title);

            var badge = new RatingBadge
            {
                RatingText = media.Type == "series" ? "Series" : "Movie",
                IsGold = false,
                Location = new Point(14, 212),
                Size = new Size(62, 22),
                Cursor = Cursors.Hand
            };

            var lblYear = new Label
            {
                Text = media.Year ?? "",
                Font = Theme.Small,
                ForeColor = Theme.TextSecondary,
                Location = new Point(80, 215),
                Size = new Size(120, 18),
                TextAlign = ContentAlignment.TopRight,
                Cursor = Cursors.Hand
            };

            var lblTitle = new Label
            {
                Text = media.Title,
                Font = Theme.BodyBold,
                ForeColor = Theme.TextPrimary,
                Location = new Point(14, 238),
                Size = new Size(182, 38),
                AutoEllipsis = true,
                Cursor = Cursors.Hand
            };

            var lblAction = new Label
            {
                Text = "Click to View & Rate →",
                Font = Theme.SmallBold,
                ForeColor = Theme.StarbucksGreen,
                Location = new Point(14, 280),
                Size = new Size(182, 18),
                Cursor = Cursors.Hand
            };

            Action openAction = () => _openMediaDetail?.Invoke(media);
            card.Click += (s, e) => openAction();
            pic.Click += (s, e) => openAction();
            lblTitle.Click += (s, e) => openAction();
            lblAction.Click += (s, e) => openAction();
            badge.Click += (s, e) => openAction();

            card.Controls.Add(pic);
            card.Controls.Add(badge);
            card.Controls.Add(lblYear);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblAction);

            return card;
        }

        private async void LoadPosterAsync(PictureBox pb, string url, string title)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var img = await ImageCache.GetImageAsync(url, title, 190, 195);
                if (img != null && !pb.IsDisposed)
                {
                    pb.Image = img;
                }
            }
        }
    }
}

