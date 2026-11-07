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
    public class DoubleBufferedTableLayoutPanel : TableLayoutPanel
    {
        public DoubleBufferedTableLayoutPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint, true);
        }
    }

    public class ExploreView : UserControl
    {
        private readonly DatabaseService _db;
        private readonly AuthService _auth;
        private readonly OmdbService _omdb;
        private readonly Action<string> _showToast;
        private readonly Action<MediaItem> _openMediaDetail;

        private KineticScrollPanel _scrollContainer;
        private Panel _pnlHero;
        private FlowLayoutPanel _flowFilterBar;
        private DoubleBufferedTableLayoutPanel _gridPanel;
        private PillButton _btnFilterAll;
        private PillButton _btnFilterMovies;
        private PillButton _btnFilterSeries;
        private PillButton _btnFilterTopRated;
        private string _activeFilter = "All";

        public ExploreView(DatabaseService db, AuthService auth, OmdbService omdb, Action<string> showToast, Action<MediaItem> openMediaDetail)
        {
            _db = db;
            _auth = auth;
            _omdb = omdb;
            _showToast = showToast;
            _openMediaDetail = openMediaDetail;

            InitializeComponent();
            LoadMediaCards();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.PageCanvas;

            _scrollContainer = new KineticScrollPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.PageCanvas
            };

            // 1. Hero Feature Band (from rate-me-design.md)
            _pnlHero = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1100, 215),
                BackColor = Theme.HouseGreen
            };

            var lblHeroSuper = new Label
            {
                Text = "CURATED CINEMA & SERIES",
                Font = Theme.SmallBold,
                ForeColor = Theme.Gold,
                Location = new Point(32, 22),
                AutoSize = true
            };

            var lblHeroHeadline = new Label
            {
                Text = "Discover Your Next Obsession.",
                Font = new Font(Theme.SerifFontFamily, 22f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(32, 46),
                Size = new Size(600, 42)
            };

            var lblHeroDesc = new Label
            {
                Text = "Explore critically acclaimed movies and binge-worthy television series, track your personal ratings, and build your bespoke watchlist.",
                Font = Theme.Body,
                ForeColor = Theme.TextWhiteSoft,
                Location = new Point(32, 92),
                Size = new Size(580, 42)
            };

            var btnHeroAction = new PillButton
            {
                Text = "Explore Top Rated ★",
                Style = PillButtonStyle.WhiteFilled,
                Location = new Point(32, 146),
                Size = new Size(165, 38)
            };
            btnHeroAction.Click += (s, e) => SetFilter("TopRated");

            var btnHeroStaff = new PillButton
            {
                Text = "Staff Picks",
                Style = PillButtonStyle.WhiteOutlined,
                Location = new Point(210, 146),
                Size = new Size(125, 38)
            };
            btnHeroStaff.Click += (s, e) => SetFilter("All");

            _pnlHero.Controls.Add(lblHeroSuper);
            _pnlHero.Controls.Add(lblHeroHeadline);
            _pnlHero.Controls.Add(lblHeroDesc);
            _pnlHero.Controls.Add(btnHeroAction);
            _pnlHero.Controls.Add(btnHeroStaff);

            // 2. Filter Bar using FlowLayoutPanel
            _flowFilterBar = new FlowLayoutPanel
            {
                Location = new Point(24, 230),
                Size = new Size(1000, 46),
                BackColor = Color.Transparent,
                WrapContents = false
            };

            _btnFilterAll = new PillButton
            {
                Text = "All Releases",
                Style = PillButtonStyle.PrimaryFilled,
                Size = new Size(115, 34),
                Margin = new Padding(0, 4, 10, 4)
            };
            _btnFilterAll.Click += (s, e) => SetFilter("All");

            _btnFilterMovies = new PillButton
            {
                Text = "Movies",
                Style = PillButtonStyle.PrimaryOutlined,
                Size = new Size(95, 34),
                Margin = new Padding(0, 4, 10, 4)
            };
            _btnFilterMovies.Click += (s, e) => SetFilter("Movies");

            _btnFilterSeries = new PillButton
            {
                Text = "TV Series",
                Style = PillButtonStyle.PrimaryOutlined,
                Size = new Size(100, 34),
                Margin = new Padding(0, 4, 10, 4)
            };
            _btnFilterSeries.Click += (s, e) => SetFilter("Series");

            _btnFilterTopRated = new PillButton
            {
                Text = "Top Rated ★",
                Style = PillButtonStyle.PrimaryOutlined,
                Size = new Size(115, 34),
                Margin = new Padding(0, 4, 10, 4)
            };
            _btnFilterTopRated.Click += (s, e) => SetFilter("TopRated");

            _flowFilterBar.Controls.Add(_btnFilterAll);
            _flowFilterBar.Controls.Add(_btnFilterMovies);
            _flowFilterBar.Controls.Add(_btnFilterSeries);
            _flowFilterBar.Controls.Add(_btnFilterTopRated);

            // 3. Strict 5-Column Grid Layout (5 by 5 Grid)
            _gridPanel = new DoubleBufferedTableLayoutPanel
            {
                Location = new Point(24, 285),
                Size = new Size(1050, 1800),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent,
                ColumnCount = 5
            };

            for (int i = 0; i < 5; i++)
            {
                _gridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));
            }

            _scrollContainer.Controls.Add(_pnlHero);
            _scrollContainer.Controls.Add(_flowFilterBar);
            _scrollContainer.Controls.Add(_gridPanel);

            Controls.Add(_scrollContainer);

            _scrollContainer.Resize += (s, e) => UpdateLayoutWidths();
            Resize += (s, e) => UpdateLayoutWidths();
            HandleCreated += (s, e) => UpdateLayoutWidths();
        }

        private void UpdateLayoutWidths()
        {
            if (_scrollContainer == null) return;
            int clientWidth = _scrollContainer.ClientSize.Width;
            int availableWidth = Math.Max(800, clientWidth - 48);

            if (_gridPanel != null && _gridPanel.Width != availableWidth)
            {
                _gridPanel.Width = availableWidth;
            }
            if (_pnlHero != null && _pnlHero.Width != clientWidth)
            {
                _pnlHero.Width = Math.Max(800, clientWidth);
            }
            if (_flowFilterBar != null && _flowFilterBar.Width != availableWidth)
            {
                _flowFilterBar.Width = availableWidth;
            }
        }

        private void SetFilter(string filter)
        {
            _activeFilter = filter;

            _btnFilterAll.Style = filter == "All" ? PillButtonStyle.PrimaryFilled : PillButtonStyle.PrimaryOutlined;
            _btnFilterMovies.Style = filter == "Movies" ? PillButtonStyle.PrimaryFilled : PillButtonStyle.PrimaryOutlined;
            _btnFilterSeries.Style = filter == "Series" ? PillButtonStyle.PrimaryFilled : PillButtonStyle.PrimaryOutlined;
            _btnFilterTopRated.Style = filter == "TopRated" ? PillButtonStyle.PrimaryFilled : PillButtonStyle.PrimaryOutlined;

            LoadMediaCards();
        }

        public void LoadMediaCards()
        {
            if (_gridPanel == null) return;

            _gridPanel.SuspendLayout();
            _gridPanel.Controls.Clear();
            _gridPanel.RowStyles.Clear();

            var allItems = _db.GetCuratedMedia();

            IEnumerable<MediaItem> filtered = allItems;
            if (_activeFilter == "Movies")
            {
                filtered = allItems.Where(m => m.Type == "movie");
            }
            else if (_activeFilter == "Series")
            {
                filtered = allItems.Where(m => m.Type == "series");
            }
            else if (_activeFilter == "TopRated")
            {
                filtered = allItems.OrderByDescending(m => m.AverageUserRating > 0 ? m.AverageUserRating : (m.ImdbRating ?? 0));
            }

            var list = filtered.ToList();
            int totalCount = list.Count;
            int numRows = (totalCount + 4) / 5;
            if (numRows < 1) numRows = 1;

            _gridPanel.RowCount = numRows;
            for (int r = 0; r < numRows; r++)
            {
                _gridPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 355f));
            }

            for (int i = 0; i < totalCount; i++)
            {
                int col = i % 5;
                int row = i / 5;
                var card = CreateMediaCard(list[i]);
                _gridPanel.Controls.Add(card, col, row);
            }

            _gridPanel.ResumeLayout(true);
            UpdateLayoutWidths();
        }

        private Control CreateMediaCard(MediaItem media)
        {
            var card = new RoundedCard
            {
                Size = new Size(200, 340),
                Dock = DockStyle.Fill,
                Margin = new Padding(6, 6, 6, 12),
                BorderRadius = 12,
                HoverElevate = true,
                DrawShadow = true,
                Cursor = Cursors.Hand
            };

            var pic = new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(180, 220),
                Height = 220,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(20, 38, 33),
                Cursor = Cursors.Hand
            };
            pic.Image = ImageCache.GetPlaceholder(media.Title, 180, 220);
            LoadPosterAsync(pic, media.PosterUrl, media.Title);

            var badge = new RatingBadge
            {
                RatingText = $"{media.DisplayScore} ★",
                IsGold = true,
                Location = new Point(10, 238),
                Size = new Size(62, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Cursor = Cursors.Hand
            };

            var lblYear = new Label
            {
                Text = media.Year ?? "",
                Font = Theme.Small,
                ForeColor = Theme.TextSecondary,
                Location = new Point(110, 241),
                Size = new Size(80, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TextAlign = ContentAlignment.TopRight,
                Cursor = Cursors.Hand
            };

            var lblTitle = new Label
            {
                Text = media.Title,
                Font = Theme.BodyBold,
                ForeColor = Theme.TextPrimary,
                Location = new Point(10, 268),
                Size = new Size(180, 38),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Cursor = Cursors.Hand
            };

            var lblGenre = new Label
            {
                Text = media.Genre ?? (media.Type == "series" ? "Series" : "Feature"),
                Font = Theme.Small,
                ForeColor = Theme.StarbucksGreen,
                Location = new Point(10, 310),
                Size = new Size(180, 18),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Cursor = Cursors.Hand
            };

            Action openAction = () => _openMediaDetail?.Invoke(media);
            card.Click += (s, e) => openAction();
            pic.Click += (s, e) => openAction();
            lblTitle.Click += (s, e) => openAction();
            lblGenre.Click += (s, e) => openAction();
            badge.Click += (s, e) => openAction();

            card.Controls.Add(pic);
            card.Controls.Add(badge);
            card.Controls.Add(lblYear);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblGenre);

            return card;
        }

        private async void LoadPosterAsync(PictureBox pb, string url, string title)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var img = await ImageCache.GetImageAsync(url, title, 180, 220);
                if (img != null && !pb.IsDisposed)
                {
                    pb.Image = img;
                }
            }
        }
    }
}
