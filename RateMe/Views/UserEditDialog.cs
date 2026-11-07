using System;
using System.Drawing;
using System.Windows.Forms;
using RateMe.Models;
using RateMe.Services;
using RateMe.UI;
using RateMe.UI.Controls;

namespace RateMe.Views
{
    public class UserEditDialog : Form
    {
        private readonly DatabaseService _db;
        private readonly User _existingUser; // null if creating a new user
        private readonly bool _isNew;

        private Label _lblTitle;
        private Label _lblSubtitle;
        private Label _lblError;

        private ModernTextBox _txtUsername;
        private ModernTextBox _txtEmail;
        private ModernTextBox _txtDisplayName;
        private ModernTextBox _txtBio;
        private ModernTextBox _txtPassword; // Only used when creating new user

        private ComboBox _cboRole;
        private CheckBox _chkIsActive;
        private PillButton _btnSave;
        private PillButton _btnCancel;

        public User CreatedOrUpdatedUser { get; private set; }

        public UserEditDialog(DatabaseService db, User existingUser = null)
        {
            _db = db;
            _existingUser = existingUser;
            _isNew = existingUser == null;

            InitializeComponent();
            PopulateData();
        }

        private void InitializeComponent()
        {
            Text = _isNew ? "Admin - Create New User" : $"Admin - Edit User: {_existingUser.Username}";
            Size = new Size(480, _isNew ? 620 : 560);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Theme.PageCanvas;

            var card = new RoundedCard
            {
                Location = new Point(20, 16),
                Size = new Size(424, _isNew ? 545 : 485),
                BorderRadius = 14,
                DrawShadow = true
            };

            _lblTitle = new Label
            {
                Text = _isNew ? "Create User Account" : "Edit User Account",
                Font = Theme.HeaderH1,
                ForeColor = Theme.StarbucksGreen,
                BackColor = Color.Transparent,
                Location = new Point(20, 16),
                AutoSize = true
            };

            _lblSubtitle = new Label
            {
                Text = _isNew 
                    ? "Provision a new user account with role-based access." 
                    : $"Editing account details for @{_existingUser?.Username}",
                Font = Theme.Small,
                ForeColor = Theme.TextSecondary,
                BackColor = Color.Transparent,
                Location = new Point(20, 44),
                AutoSize = true
            };

            _lblError = new Label
            {
                Text = "",
                Font = Theme.SmallBold,
                ForeColor = Theme.DestructiveRed,
                BackColor = Color.Transparent,
                Location = new Point(20, 68),
                Size = new Size(380, 24),
                Visible = false
            };

            int y = 96;

            var lblUser = new Label { Text = "Username:", Font = Theme.SmallBold, ForeColor = Theme.TextPrimary, BackColor = Color.Transparent, Location = new Point(20, y), AutoSize = true };
            _txtUsername = new ModernTextBox { PlaceholderText = "username", Location = new Point(20, y + 20), Size = new Size(380, 36) };
            if (!_isNew)
            {
                _txtUsername.InnerTextBox.ReadOnly = true;
                _txtUsername.InnerTextBox.BackColor = Color.FromArgb(235, 233, 228);
            }
            y += 62;

            var lblEmail = new Label { Text = "Email Address:", Font = Theme.SmallBold, ForeColor = Theme.TextPrimary, BackColor = Color.Transparent, Location = new Point(20, y), AutoSize = true };
            _txtEmail = new ModernTextBox { PlaceholderText = "user@domain.com", Location = new Point(20, y + 20), Size = new Size(380, 36) };
            y += 62;

            var lblDisplayName = new Label { Text = "Display Name:", Font = Theme.SmallBold, ForeColor = Theme.TextPrimary, BackColor = Color.Transparent, Location = new Point(20, y), AutoSize = true };
            _txtDisplayName = new ModernTextBox { PlaceholderText = "Display Name", Location = new Point(20, y + 20), Size = new Size(380, 36) };
            y += 62;

            if (_isNew)
            {
                var lblPwd = new Label { Text = "Initial Password:", Font = Theme.SmallBold, ForeColor = Theme.TextPrimary, BackColor = Color.Transparent, Location = new Point(20, y), AutoSize = true };
                _txtPassword = new ModernTextBox { PlaceholderText = "Password (min 6 chars)", UseSystemPasswordChar = true, Location = new Point(20, y + 20), Size = new Size(380, 36) };
                card.Controls.Add(lblPwd);
                card.Controls.Add(_txtPassword);
                y += 62;
            }

            var lblBio = new Label { Text = "Bio:", Font = Theme.SmallBold, ForeColor = Theme.TextPrimary, BackColor = Color.Transparent, Location = new Point(20, y), AutoSize = true };
            _txtBio = new ModernTextBox { PlaceholderText = "Short bio or notes...", Location = new Point(20, y + 20), Size = new Size(380, 36) };
            y += 62;

            // Role selection
            var lblRole = new Label { Text = "System Role:", Font = Theme.SmallBold, ForeColor = Theme.TextPrimary, BackColor = Color.Transparent, Location = new Point(20, y), AutoSize = true };
            _cboRole = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Theme.Body,
                Location = new Point(20, y + 22),
                Size = new Size(180, 30)
            };
            _cboRole.Items.AddRange(new object[] { "User", "Admin" });
            _cboRole.SelectedIndex = 0;

            _chkIsActive = new CheckBox
            {
                Text = "Account Active",
                Font = Theme.BodyBold,
                ForeColor = Theme.GreenAccent,
                Location = new Point(220, y + 24),
                Size = new Size(160, 24),
                Checked = true
            };
            y += 65;

            // Action buttons
            _btnSave = new PillButton
            {
                Text = _isNew ? "Create User" : "Save Changes",
                Style = PillButtonStyle.PrimaryFilled,
                Location = new Point(20, y),
                Size = new Size(160, 40)
            };
            _btnSave.Click += OnSaveClick;

            _btnCancel = new PillButton
            {
                Text = "Cancel",
                Style = PillButtonStyle.DarkOutlined,
                Location = new Point(190, y),
                Size = new Size(110, 40)
            };
            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            card.Controls.Add(_lblTitle);
            card.Controls.Add(_lblSubtitle);
            card.Controls.Add(_lblError);
            card.Controls.Add(lblUser);
            card.Controls.Add(_txtUsername);
            card.Controls.Add(lblEmail);
            card.Controls.Add(_txtEmail);
            card.Controls.Add(lblDisplayName);
            card.Controls.Add(_txtDisplayName);
            card.Controls.Add(lblBio);
            card.Controls.Add(_txtBio);
            card.Controls.Add(lblRole);
            card.Controls.Add(_cboRole);
            card.Controls.Add(_chkIsActive);
            card.Controls.Add(_btnSave);
            card.Controls.Add(_btnCancel);

            Controls.Add(card);
        }

        private void PopulateData()
        {
            if (!_isNew && _existingUser != null)
            {
                _txtUsername.Text = _existingUser.Username;
                _txtEmail.Text = _existingUser.Email;
                _txtDisplayName.Text = _existingUser.DisplayName;
                _txtBio.Text = _existingUser.Bio ?? "";
                _cboRole.SelectedItem = _existingUser.Role ?? "User";
                _chkIsActive.Checked = _existingUser.IsActive;
            }
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            _lblError.Visible = false;

            string email = _txtEmail.Text.Trim();
            string displayName = _txtDisplayName.Text.Trim();
            string bio = _txtBio.Text.Trim();
            string role = _cboRole.SelectedItem?.ToString() ?? "User";
            bool isActive = _chkIsActive.Checked;

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || !email.Contains("."))
            {
                _lblError.Text = "Please enter a valid email address.";
                _lblError.Visible = true;
                return;
            }

            if (_isNew)
            {
                string username = _txtUsername.Text.Trim();
                string password = _txtPassword.Text;

                if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
                {
                    _lblError.Text = "Username must be at least 3 characters.";
                    _lblError.Visible = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                {
                    _lblError.Text = "Password must be at least 6 characters.";
                    _lblError.Visible = true;
                    return;
                }

                if (_db.GetUserByUsernameOrEmail(username) != null)
                {
                    _lblError.Text = "Username is already taken.";
                    _lblError.Visible = true;
                    return;
                }

                if (_db.GetUserByUsernameOrEmail(email) != null)
                {
                    _lblError.Text = "Email is already registered.";
                    _lblError.Visible = true;
                    return;
                }

                string salt = AuthService.GenerateSalt();
                string hash = AuthService.HashPassword(password, salt);

                var newUser = new User
                {
                    Username = username,
                    Email = email,
                    PasswordHash = hash,
                    Salt = salt,
                    DisplayName = string.IsNullOrEmpty(displayName) ? username : displayName,
                    Bio = bio,
                    Role = role,
                    IsActive = isActive,
                    CreatedAt = DateTime.Now
                };

                if (_db.CreateUser(newUser))
                {
                    CreatedOrUpdatedUser = newUser;
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    _lblError.Text = "Database error creating user.";
                    _lblError.Visible = true;
                }
            }
            else
            {
                _existingUser.Email = email;
                _existingUser.DisplayName = string.IsNullOrEmpty(displayName) ? _existingUser.Username : displayName;
                _existingUser.Bio = bio;
                _existingUser.Role = role;
                _existingUser.IsActive = isActive;

                if (_db.UpdateUserAdmin(_existingUser))
                {
                    CreatedOrUpdatedUser = _existingUser;
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    _lblError.Text = "Database error updating user.";
                    _lblError.Visible = true;
                }
            }
        }
    }
}

