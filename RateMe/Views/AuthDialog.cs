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

        private RoundedCard _card;
        private Label _lblTitle;
        private Label _lblSubtitle;
        private Label _lblError;

        private Label _lblUserField;
        private ModernTextBox _txtUsername;

        private Label _lblEmailField;
        private ModernTextBox _txtEmail;

        private Label _lblDisplayNameField;
        private ModernTextBox _txtDisplayName;

        private Label _lblPasswordField;
        private ModernTextBox _txtPassword;

        private PillButton _btnSubmit;
        private PillButton _btnDemoLogin;
        private PillButton _btnToggleMode;

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
            Size = new Size(420, 520);
            BackColor = Theme.NeutralWarm;
            Text = "RateMe - Sign In";

            _card = new RoundedCard
            {
                Location = new Point(24, 18),
                Size = new Size(356, 440),
                BorderRadius = 16,
                CustomBackColor = Theme.IsDarkMode ? Theme.DarkCard : Theme.White,
                DrawShadow = true
            };

            _lblTitle = new Label
            {
                Text = "Welcome Back",
                Font = Theme.HeaderDisplay,
                ForeColor = Theme.StarbucksGreen,
                BackColor = Color.Transparent,
                Location = new Point(14, 18),
                Size = new Size(328, 36),
                TextAlign = ContentAlignment.MiddleCenter
            };

            _lblSubtitle = new Label
            {
                Text = "Sign in to rate movies, build watchlists, and connect.",
                Font = Theme.Small,
                ForeColor = Theme.TextSecondary,
                BackColor = Color.Transparent,
                Location = new Point(14, 52),
                Size = new Size(328, 24),
                TextAlign = ContentAlignment.MiddleCenter
            };

            _lblError = new Label
            {
                Text = "",
                Font = Theme.SmallBold,
                ForeColor = Theme.DestructiveRed,
                BackColor = Color.Transparent,
                Location = new Point(14, 76),
                Size = new Size(328, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            // 1. Username Field
            _lblUserField = new Label
            {
                Text = "USERNAME OR EMAIL",
                Font = Theme.Micro,
                ForeColor = Theme.StarbucksGreen,
                BackColor = Color.Transparent,
                Location = new Point(20, 102),
                Size = new Size(316, 16)
            };

            _txtUsername = new ModernTextBox
            {
                PlaceholderText = "e.g. cinema_fan or user@domain.com",
                Location = new Point(20, 120),
                Size = new Size(316, 36)
            };

            // 2. Email Field (Sign Up only)
            _lblEmailField = new Label
            {
                Text = "EMAIL ADDRESS",
                Font = Theme.Micro,
                ForeColor = Theme.StarbucksGreen,
                BackColor = Color.Transparent,
                Location = new Point(20, 162),
                Size = new Size(316, 16),
                Visible = false
            };

            _txtEmail = new ModernTextBox
            {
                PlaceholderText = "name@example.com",
                Location = new Point(20, 180),
                Size = new Size(316, 36),
                Visible = false
            };

            // 3. Display Name Field (Sign Up only)
            _lblDisplayNameField = new Label
            {
                Text = "DISPLAY NAME",
                Font = Theme.Micro,
                ForeColor = Theme.StarbucksGreen,
                BackColor = Color.Transparent,
                Location = new Point(20, 222),
                Size = new Size(316, 16),
                Visible = false
            };

            _txtDisplayName = new ModernTextBox
            {
                PlaceholderText = "e.g. Aria Sterling",
                Location = new Point(20, 240),
                Size = new Size(316, 36),
                Visible = false
            };

            // 4. Password Field
            _lblPasswordField = new Label
            {
                Text = "PASSWORD",
                Font = Theme.Micro,
                ForeColor = Theme.StarbucksGreen,
                BackColor = Color.Transparent,
                Location = new Point(20, 164), // Dynamic
                Size = new Size(316, 16)
            };

            _txtPassword = new ModernTextBox
            {
                PlaceholderText = "Enter your password",
                UseSystemPasswordChar = true,
                Location = new Point(20, 182), // Dynamic
                Size = new Size(316, 36)
            };

            // Buttons
            _btnSubmit = new PillButton
            {
                Text = "Sign In",
                Style = PillButtonStyle.PrimaryFilled,
                Location = new Point(20, 236),
                Size = new Size(316, 40)
            };
            _btnSubmit.Click += OnSubmitClick;

            _btnDemoLogin = new PillButton
            {
                Text = "Quick Demo Login (fan@rateme.app)",
                Style = PillButtonStyle.PrimaryOutlined,
                Location = new Point(20, 286),
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
                Location = new Point(20, 332),
                Size = new Size(316, 38)
            };
            _btnToggleMode.Click += (s, e) =>
            {
                _isSignUpMode = !_isSignUpMode;
                UpdateModeUI();
            };

            _card.Controls.Add(_lblTitle);
            _card.Controls.Add(_lblSubtitle);
            _card.Controls.Add(_lblError);

            _card.Controls.Add(_lblUserField);
            _card.Controls.Add(_txtUsername);

            _card.Controls.Add(_lblEmailField);
            _card.Controls.Add(_txtEmail);

            _card.Controls.Add(_lblDisplayNameField);
            _card.Controls.Add(_txtDisplayName);

            _card.Controls.Add(_lblPasswordField);
            _card.Controls.Add(_txtPassword);

            _card.Controls.Add(_btnSubmit);
            _card.Controls.Add(_btnDemoLogin);
            _card.Controls.Add(_btnToggleMode);

            Controls.Add(_card);
        }

        private void UpdateModeUI()
        {
            _lblError.Visible = false;

            if (_isSignUpMode)
            {
                Text = "RateMe - Create Account";
                _lblTitle.Text = "Join RateMe";
                _lblSubtitle.Text = "Rate your favorite films & television series.";

                _lblUserField.Text = "CHOOSE USERNAME";
                _txtUsername.PlaceholderText = "e.g. cinema_lover";

                _lblEmailField.Visible = true;
                _txtEmail.Visible = true;

                _lblDisplayNameField.Visible = true;
                _txtDisplayName.Visible = true;

                _lblPasswordField.Text = "PASSWORD (MIN. 6 CHARACTERS)";
                _lblPasswordField.Location = new Point(20, 282);
                _txtPassword.Location = new Point(20, 300);
                _txtPassword.PlaceholderText = "Choose a strong password";

                _btnSubmit.Location = new Point(20, 354);
                _btnSubmit.Text = "Create Account";

                _btnDemoLogin.Visible = false;

                _btnToggleMode.Location = new Point(20, 404);
                _btnToggleMode.Text = "Already have an account? Sign In";

                _card.Size = new Size(356, 465);
                ClientSize = new Size(404, 505);
            }
            else
            {
                Text = "RateMe - Sign In";
                _lblTitle.Text = "Welcome Back";
                _lblSubtitle.Text = "Sign in to rate movies, build watchlists, and connect.";

                _lblUserField.Text = "USERNAME OR EMAIL";
                _txtUsername.PlaceholderText = "e.g. cinema_fan or user@domain.com";

                _lblEmailField.Visible = false;
                _txtEmail.Visible = false;

                _lblDisplayNameField.Visible = false;
                _txtDisplayName.Visible = false;

                _lblPasswordField.Text = "PASSWORD";
                _lblPasswordField.Location = new Point(20, 164);
                _txtPassword.Location = new Point(20, 182);
                _txtPassword.PlaceholderText = "Enter your password";

                _btnSubmit.Location = new Point(20, 236);
                _btnSubmit.Text = "Sign In";

                _btnDemoLogin.Visible = true;
                _btnDemoLogin.Location = new Point(20, 286);

                _btnToggleMode.Location = new Point(20, 332);
                _btnToggleMode.Text = "Need an account? Join now";

                _card.Size = new Size(356, 395);
                ClientSize = new Size(404, 435);
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
