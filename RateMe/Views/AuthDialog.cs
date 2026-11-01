using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RateMe.Services;
using RateMe.UI;
using RateMe.UI.Controls;

namespace RateMe.Views
{
    public class AuthDialog : Form
    {
        private readonly AuthService _auth;
        private bool _isSignUpMode = false;

        private Label _lblTitle;
        private Label _lblSubtitle;
        private ModernTextBox _txtUsername;
        private ModernTextBox _txtEmail;
        private ModernTextBox _txtDisplayName;
        private ModernTextBox _txtPassword;
        private PillButton _btnSubmit;
        private PillButton _btnToggleMode;
        private PillButton _btnDemoLogin;
        private Label _lblError;

        public AuthDialog(AuthService auth, bool startInSignUp = false)
        {
            _auth = auth;
            _isSignUpMode = startInSignUp;

            InitializeComponent();
            UpdateModeUI();
        }

        private void InitializeComponent()
        {
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(420, 560);
            BackColor = Theme.NeutralWarm;
            Text = "RateMe - Sign In";

            var card = new RoundedCard
            {
                Location = new Point(24, 20),
                Size = new Size(356, 475),
                BorderRadius = 16,
                CustomBackColor = Theme.IsDarkMode ? Theme.DarkCard : Theme.White,
                DrawShadow = true
            };

            _lblTitle = new Label
            {
                Text = "Welcome Back",
                Font = Theme.HeaderDisplay,
                ForeColor = Theme.StarbucksGreen,
                Location = new Point(20, 20),
                Size = new Size(316, 36),
                TextAlign = ContentAlignment.MiddleCenter
            };

            _lblSubtitle = new Label
            {
                Text = "Sign in to rate movies, build watchlists, and connect.",
                Font = Theme.Small,
                ForeColor = Theme.TextSecondary,
                Location = new Point(20, 56),
                Size = new Size(316, 28),
                TextAlign = ContentAlignment.MiddleCenter
            };

            _lblError = new Label
            {
                Text = "",
                Font = Theme.SmallBold,
                ForeColor = Theme.DestructiveRed,
                Location = new Point(20, 86),
                Size = new Size(316, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            _txtUsername = new ModernTextBox
            {
                PlaceholderText = "Username or Email",
                Location = new Point(20, 120),
                Size = new Size(316, 38)
            };

            _txtEmail = new ModernTextBox
            {
                PlaceholderText = "Email Address",
                Location = new Point(20, 168),
                Size = new Size(316, 38),
                Visible = false
            };

            _txtDisplayName = new ModernTextBox
            {
                PlaceholderText = "Display Name (e.g. CinemaLover)",
                Location = new Point(20, 216),
                Size = new Size(316, 38),
                Visible = false
            };

            _txtPassword = new ModernTextBox
            {
                PlaceholderText = "Password",
                UseSystemPasswordChar = true,
                Location = new Point(20, 168), // Moves dynamically
                Size = new Size(316, 38)
            };

            _btnSubmit = new PillButton
            {
                Text = "Sign In",
                Style = PillButtonStyle.PrimaryFilled,
                Location = new Point(20, 226),
                Size = new Size(316, 42)
            };
            _btnSubmit.Click += OnSubmitClick;

            _btnDemoLogin = new PillButton
            {
                Text = "Quick Demo Login (fan@rateme.app)",
                Style = PillButtonStyle.PrimaryOutlined,
                Location = new Point(20, 278),
                Size = new Size(316, 38)
            };
            _btnDemoLogin.Click += (s, e) =>
            {
                _txtUsername.Text = "cinema_fan";
                _txtPassword.Text = "User@123";
                OnSubmitClick(s, e);
            };

            _btnToggleMode = new PillButton
            {
                Text = "Need an account? Join now",
                Style = PillButtonStyle.DarkOutlined,
                Location = new Point(20, 326),
                Size = new Size(316, 38)
            };
            _btnToggleMode.Click += (s, e) =>
            {
                _isSignUpMode = !_isSignUpMode;
                UpdateModeUI();
            };

            card.Controls.Add(_lblTitle);
            card.Controls.Add(_lblSubtitle);
            card.Controls.Add(_lblError);
            card.Controls.Add(_txtUsername);
            card.Controls.Add(_txtEmail);
            card.Controls.Add(_txtDisplayName);
            card.Controls.Add(_txtPassword);
            card.Controls.Add(_btnSubmit);
            card.Controls.Add(_btnDemoLogin);
            card.Controls.Add(_btnToggleMode);

            Controls.Add(card);
        }

        private void UpdateModeUI()
        {
            _lblError.Visible = false;

            if (_isSignUpMode)
            {
                Text = "RateMe - Create Account";
                _lblTitle.Text = "Join RateMe";
                _lblSubtitle.Text = "Rate your favorite films & television series.";

                _txtUsername.PlaceholderText = "Choose Username";
                _txtEmail.Visible = true;
                _txtDisplayName.Visible = true;

                _txtPassword.Location = new Point(20, 264);
                _btnSubmit.Location = new Point(20, 318);
                _btnSubmit.Text = "Create Account";
                _btnDemoLogin.Visible = false;
                _btnToggleMode.Location = new Point(20, 368);
                _btnToggleMode.Text = "Already have an account? Sign In";
            }
            else
            {
                Text = "RateMe - Sign In";
                _lblTitle.Text = "Welcome Back";
                _lblSubtitle.Text = "Sign in to rate movies, build watchlists, and connect.";

                _txtUsername.PlaceholderText = "Username or Email";
                _txtEmail.Visible = false;
                _txtDisplayName.Visible = false;

                _txtPassword.Location = new Point(20, 168);
                _btnSubmit.Location = new Point(20, 226);
                _btnSubmit.Text = "Sign In";
                _btnDemoLogin.Visible = true;
                _btnToggleMode.Location = new Point(20, 326);
                _btnToggleMode.Text = "Need an account? Join now";
            }
        }

        private void OnSubmitClick(object sender, EventArgs e)
        {
            _lblError.Visible = false;

            if (_isSignUpMode)
            {
                if (_auth.Register(_txtUsername.Text, _txtEmail.Text, _txtPassword.Text, _txtDisplayName.Text, out string err))
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    _lblError.Text = err;
                    _lblError.Visible = true;
                }
            }
            else
            {
                if (_auth.Login(_txtUsername.Text, _txtPassword.Text, out string err))
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    _lblError.Text = err;
                    _lblError.Visible = true;
                }
            }
        }
    }
}

