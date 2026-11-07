using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using RateMe.Models;
using RateMe.Services;
using RateMe.UI;
using RateMe.UI.Controls;

namespace RateMe.Views
{
    public class MediaDetailDialog : Form
    {
        private readonly MediaItem _media;
        private readonly DatabaseService _db;
        private readonly AuthService _auth;
        private readonly OmdbService _omdb;
        private readonly Action<string> _showToast;

        private PictureBox _picPoster;
        private Label _lblBreadcrumb;
        private Label _lblTitle;
        private Label _lblMetadata;
        private RatingBadge _badgeImdb;
        private RatingBadge _badgeUserAvg;
        private Label _lblPlot;

        private StarRatingControl _starRating;
        private Label _lblScoreFeedback;
        private ModernTextBox _txtReview;
        private PillButton _btnSubmitRating;
        private PillButton _btnWatchlist;

        private Panel _pnlReviewsList;

        public MediaDetailDialog(MediaItem media, DatabaseService db, AuthService auth, OmdbService omdb, Action<string> showToast)
        {
            _media = media;
            _db = db;
            _auth = auth;
            _omdb = omdb;
            _showToast = showToast;

            InitializeComponent();
            LoadFullDetailsAsync();
            LoadReviews();
            UpdateWatchlistButton();
            LoadExistingUserRating();
        }

        private void InitializeComponent()
        {
            Text = $"{_media.Title} ({_media.Year}) - RateMe";
            Size = new Size(800, 750);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Theme.PageCanvas;

            var scrollHost = new KineticScrollPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.PageCanvas
            };

            // 1. Top House Green Header Band (Starbucks PDP signature)
            var pnlHeaderBand = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(785, 250),
                BackColor = Theme.HouseGreen
            };

            _lblBreadcrumb = new Label
            {
                Text = $"Explore  /  {(_media.Type == "series" ? "TV Series" : "Movies")}  /  {_media.Title}",
                Font = Theme.Small,
                ForeColor = Theme.TextWhiteSoft,
                BackColor = Color.Transparent,
                Location = new Point(24, 16),
                AutoSize = true
            };

            _picPoster = new PictureBox
            {
                Location = new Point(24, 44),
                Size = new Size(130, 185),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(20, 42, 36)
            };
            _picPoster.Image = ImageCache.GetPlaceholder(_media.Title, 130, 185);

            _lblTitle = new Label
            {
                Text = _media.Title,
                Font = new Font(Theme.SerifFontFamily, 20f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(175, 44),
                Size = new Size(570, 36),
                AutoEllipsis = true
            };

            _lblMetadata = new Label
            {
                Text = $"{_media.Year}  •  {_media.Runtime ?? "120 min"}  •  {_media.Genre ?? "Feature Film"}",
                Font = Theme.Body,
                ForeColor = Theme.TextWhiteSoft,
                BackColor = Color.Transparent,
                Location = new Point(175, 84),
                Size = new Size(570, 24)
            };

            // Badges row
            var flowBadges = new FlowLayoutPanel
            {
                Location = new Point(175, 116),
                Size = new Size(500, 34),
                BackColor = Color.Transparent,
                WrapContents = false
            };

            _badgeImdb = new RatingBadge
            {
                RatingText = $"IMDb {(_media.ImdbRating.HasValue ? _media.ImdbRating.Value.ToString("0.0") : "NR")} ★",
                IsGold = true,
                Size = new Size(100, 26),
                Margin = new Padding(0, 0, 10, 0)
            };

            _badgeUserAvg = new RatingBadge
            {
                RatingText = $"RateMe {(_media.AverageUserRating > 0 ? _media.AverageUserRating.ToString("0.0") : "9.2")} ★",
                IsGold = false,
                Size = new Size(115, 26),
                Margin = new Padding(0, 0, 10, 0)
            };

            flowBadges.Controls.Add(_badgeImdb);
            flowBadges.Controls.Add(_badgeUserAvg);

            _btnWatchlist = new PillButton
            {
                Text = "+ Add to Watchlist",
                Style = PillButtonStyle.WhiteOutlined,
                Location = new Point(175, 165),
                Size = new Size(175, 38)
            };
            _btnWatchlist.Click += OnWatchlistClick;

            pnlHeaderBand.Controls.Add(_lblBreadcrumb);
            pnlHeaderBand.Controls.Add(_picPoster);
            pnlHeaderBand.Controls.Add(_lblTitle);
            pnlHeaderBand.Controls.Add(_lblMetadata);
            pnlHeaderBand.Controls.Add(flowBadges);
            pnlHeaderBand.Controls.Add(_btnWatchlist);

            // 2. Plot & Synopsis Card
            var cardPlot = new RoundedCard
            {
                Location = new Point(24, 266),
                Size = new Size(735, 125),
                BorderRadius = 12,
                DrawShadow = true
            };

            var lblPlotHeading = new Label
            {
                Text = "SYNOPSIS & CREDITS",
                Font = Theme.SmallBold,
                ForeColor = Theme.StarbucksGreen,
                BackColor = Color.Transparent,
                Location = new Point(16, 12),
                AutoSize = true
            };

            _lblPlot = new Label
            {
                Text = _media.Plot ?? "Loading full synopsis...",
                Font = Theme.Body,
                ForeColor = Theme.TextPrimary,
                BackColor = Color.Transparent,
                Location = new Point(16, 36),
                Size = new Size(700, 75),
                AutoEllipsis = true
            };

            cardPlot.Controls.Add(lblPlotHeading);
            cardPlot.Controls.Add(_lblPlot);

            // 3. Interactive Rating Card
            var cardRating = new RoundedCard
            {
                Location = new Point(24, 405),
                Size = new Size(735, 195),
                BorderRadius = 12,
                DrawShadow = true
            };

            var lblRateTitle = new Label
            {
                Text = "RATE THIS TITLE",
                Font = Theme.SmallBold,
                ForeColor = Theme.StarbucksGreen,
                BackColor = Color.Transparent,
                Location = new Point(16, 14),
                AutoSize = true
            };

            _lblScoreFeedback = new Label
            {
                Text = "Click a star to rate (1 - 10)",
                Font = Theme.BodyBold,
                ForeColor = Theme.Gold,
                BackColor = Color.Transparent,
                Location = new Point(360, 14),
                Size = new Size(350, 22),
                TextAlign = ContentAlignment.TopRight
            };

            _starRating = new StarRatingControl
            {
                MaxStars = 10,
                StarSize = 24,
                Value = 0,
                Location = new Point(16, 40),
                Size = new Size(320, 32)
            };
            _starRating.ValueChanged += (s, e) =>
            {
                _lblScoreFeedback.Text = _starRating.GetRatingLabel();
            };

            _txtReview = new ModernTextBox
            {
                PlaceholderText = "Write your review or thoughts on this title (optional)...",
                Multiline = true,
                Location = new Point(16, 80),
                Size = new Size(530, 95)
            };

            _btnSubmitRating = new PillButton
            {
                Text = "Submit Rating ★",
                Style = PillButtonStyle.PrimaryFilled,
                Location = new Point(565, 105),
                Size = new Size(150, 42)
            };
            _btnSubmitRating.Click += OnSubmitRatingClick;

            cardRating.Controls.Add(lblRateTitle);
            cardRating.Controls.Add(_lblScoreFeedback);
            cardRating.Controls.Add(_starRating);
            cardRating.Controls.Add(_txtReview);
            cardRating.Controls.Add(_btnSubmitRating);

            // 4. Community Reviews Section
            var lblReviewsHeader = new Label
            {
                Text = "COMMUNITY REVIEWS & RATINGS",
                Font = Theme.SmallBold,
                ForeColor = Theme.StarbucksGreen,
                Location = new Point(28, 615),
                AutoSize = true
            };

            _pnlReviewsList = new Panel
            {
                Location = new Point(24, 645),
                Size = new Size(735, 300),
                AutoSize = true
            };

            scrollHost.Controls.Add(pnlHeaderBand);
            scrollHost.Controls.Add(cardPlot);
            scrollHost.Controls.Add(cardRating);
            scrollHost.Controls.Add(lblReviewsHeader);
            scrollHost.Controls.Add(_pnlReviewsList);

            Controls.Add(scrollHost);
        }

        private async void LoadFullDetailsAsync()
        {
            if (!string.IsNullOrEmpty(_media.PosterUrl))
            {
                var img = await ImageCache.GetImageAsync(_media.PosterUrl, _media.Title, 130, 185);
                if (img != null && !_picPoster.IsDisposed)
                {
                    _picPoster.Image = img;
                }
            }

            if (string.IsNullOrEmpty(_media.Director) || string.IsNullOrEmpty(_media.Plot) || _media.Plot.Length < 30)
            {
                var detailed = await _omdb.GetDetailByIdAsync(_media.ImdbId);
                if (detailed != null)
                {
                    _media.Plot = detailed.Plot;
                    _media.Director = detailed.Director;
                    _media.Actors = detailed.Actors;
                    _media.Runtime = detailed.Runtime;
                    _media.Genre = detailed.Genre;
                    _media.ImdbRating = detailed.ImdbRating;
                    if (!string.IsNullOrEmpty(detailed.PosterUrl))
                    {
                        _media.PosterUrl = detailed.PosterUrl;
                    }

                    _lblPlot.Text = $"{_media.Plot}\n\nDirector: {_media.Director}  |  Cast: {_media.Actors}";
                    _lblMetadata.Text = $"{_media.Year}  •  {_media.Runtime}  •  {_media.Genre}";
                    _badgeImdb.RatingText = $"IMDb {(_media.ImdbRating.HasValue ? _media.ImdbRating.Value.ToString("0.0") : "NR")} ★";

                    _db.GetOrCreateMediaItem(_media);

                    if (!string.IsNullOrEmpty(_media.PosterUrl))
                    {
                        var img = await ImageCache.GetImageAsync(_media.PosterUrl, _media.Title, 130, 185);
                        if (img != null && !_picPoster.IsDisposed)
                        {
                            _picPoster.Image = img;
                        }
                    }
                }
            }
            else
            {
                _lblPlot.Text = $"{_media.Plot}\n\nDirector: {_media.Director}  |  Cast: {_media.Actors}";
            }
        }

        private void LoadExistingUserRating()
        {
            if (!_auth.IsLoggedIn) return;

            var existing = _db.GetUserRatingForMedia(_auth.CurrentUser.UserId, _media.MediaId);
            if (existing != null)
            {
                _starRating.Value = existing.Score;
                _txtReview.Text = existing.ReviewText;
                _lblScoreFeedback.Text = $"Your Rating: {existing.Score:0.0} ★";
                _btnSubmitRating.Text = "Update Rating ★";
            }
        }

        private void LoadReviews()
        {
            _pnlReviewsList.Controls.Clear();
            var reviews = _db.GetMediaRatings(_media.MediaId);

            int y = 0;
            if (reviews.Count == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "No community reviews yet. Be the first to rate and share your thoughts!",
                    Font = Theme.Body,
                    ForeColor = Theme.TextSecondary,
                    Location = new Point(10, 10),
                    AutoSize = true
                };
                _pnlReviewsList.Controls.Add(emptyLabel);
                return;
            }

            foreach (var r in reviews)
            {
                var rCard = new RoundedCard
                {
                    Location = new Point(0, y),
                    Size = new Size(735, 85),
                    BorderRadius = 10,
                    DrawShadow = false
                };

                var lblAuthor = new Label
                {
                    Text = r.UserDisplayName ?? r.Username ?? "Verified Reviewer",
                    Font = Theme.BodyBold,
                    ForeColor = Theme.StarbucksGreen,
                    Location = new Point(14, 12),
                    AutoSize = true
                };

                var badgeScore = new RatingBadge
                {
                    RatingText = $"{r.Score:0.0} ★",
                    IsGold = true,
                    Location = new Point(180, 10),
                    Size = new Size(58, 22)
                };

                var lblDate = new Label
                {
                    Text = r.CreatedAt.ToString("MMM dd, yyyy"),
                    Font = Theme.Small,
                    ForeColor = Theme.TextSecondary,
                    Location = new Point(610, 14),
                    Size = new Size(110, 20),
                    TextAlign = ContentAlignment.TopRight
                };

                var lblReview = new Label
                {
                    Text = string.IsNullOrWhiteSpace(r.ReviewText) ? "Rated without a written review." : r.ReviewText,
                    Font = Theme.Body,
                    ForeColor = Theme.TextPrimary,
                    Location = new Point(14, 38),
                    Size = new Size(700, 38),
                    AutoEllipsis = true
                };

                rCard.Controls.Add(lblAuthor);
                rCard.Controls.Add(badgeScore);
                rCard.Controls.Add(lblDate);
                rCard.Controls.Add(lblReview);

                _pnlReviewsList.Controls.Add(rCard);
                y += 95;
            }
        }

        private void UpdateWatchlistButton()
        {
            if (!_auth.IsLoggedIn)
            {
                _btnWatchlist.Text = "+ Add to Watchlist";
                _btnWatchlist.Style = PillButtonStyle.WhiteOutlined;
                return;
            }

            bool inList = _db.IsInWatchlist(_auth.CurrentUser.UserId, _media.MediaId);
            if (inList)
            {
                _btnWatchlist.Text = "✓ In Watchlist";
                _btnWatchlist.Style = PillButtonStyle.WhiteFilled;
            }
            else
            {
                _btnWatchlist.Text = "+ Add to Watchlist";
                _btnWatchlist.Style = PillButtonStyle.WhiteOutlined;
            }
        }

        private void OnWatchlistClick(object sender, EventArgs e)
        {
            if (!_auth.IsLoggedIn)
            {
                using (var authDlg = new AuthDialog(_auth))
                {
                    if (authDlg.ShowDialog(this) == DialogResult.OK)
                    {
                        UpdateWatchlistButton();
                        LoadExistingUserRating();
                    }
                    else
                    {
                        return;
                    }
                }
            }

            bool inList = _db.IsInWatchlist(_auth.CurrentUser.UserId, _media.MediaId);
            if (inList)
            {
                _db.RemoveFromWatchlist(_auth.CurrentUser.UserId, _media.MediaId);
                _showToast?.Invoke("Removed from your Watchlist");
            }
            else
            {
                _db.AddToWatchlist(_auth.CurrentUser.UserId, _media.MediaId, "PlanToWatch");
                _showToast?.Invoke("Added to your Watchlist ★");
            }
            UpdateWatchlistButton();
        }

        private void OnSubmitRatingClick(object sender, EventArgs e)
        {
            if (!_auth.IsLoggedIn)
            {
                using (var authDlg = new AuthDialog(_auth))
                {
                    if (authDlg.ShowDialog(this) == DialogResult.OK)
                    {
                        UpdateWatchlistButton();
                        LoadExistingUserRating();
                    }
                    else
                    {
                        return;
                    }
                }
            }

            if (_starRating.Value <= 0)
            {
                MessageBox.Show("Please click on the stars to choose your rating (1 to 10).", "RateMe", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _db.GetOrCreateMediaItem(_media);
            _db.SaveRating(_auth.CurrentUser.UserId, _media.MediaId, _starRating.Value, _txtReview.Text.Trim());
            _showToast?.Invoke($"Rated {_media.Title} {_starRating.Value:0.0} ★!");

            _btnSubmitRating.Text = "Update Rating ★";
            LoadReviews();
        }
    }
}
