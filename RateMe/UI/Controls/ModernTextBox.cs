using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RateMe.UI.Controls
{
    public class ModernTextBox : Control
    {
        private readonly TextBox _innerTextBox = new TextBox();
        private string _placeholderText = "";
        private bool _isFocused = false;
        private int _borderRadius = 8;
        private bool _isPassword = false;

        [Category("Appearance")]
        public string PlaceholderText
        {
            get => _placeholderText;
            set { _placeholderText = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue(8)]
        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = value; Invalidate(); }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        public bool UseSystemPasswordChar
        {
            get => _isPassword;
            set
            {
                _isPassword = value;
                _innerTextBox.UseSystemPasswordChar = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue(false)]
        public bool Multiline
        {
            get => _innerTextBox.Multiline;
            set
            {
                _innerTextBox.Multiline = value;
                AdjustInnerTextBox();
            }
        }

        public override string Text
        {
            get => _innerTextBox.Text;
            set => _innerTextBox.Text = value;
        }

        public TextBox InnerTextBox => _innerTextBox;

        public event EventHandler TextChangedCustom;

        public ModernTextBox()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.Transparent;
            Font = Theme.Body;
            Size = new Size(240, 38);

            _innerTextBox.BorderStyle = BorderStyle.None;
            _innerTextBox.Font = Font;
            _innerTextBox.BackColor = Theme.CardBackground;
            _innerTextBox.ForeColor = Theme.TextPrimary;
            _innerTextBox.GotFocus += (s, e) => { _isFocused = true; Invalidate(); };
            _innerTextBox.LostFocus += (s, e) => { _isFocused = false; Invalidate(); };
            _innerTextBox.TextChanged += (s, e) =>
            {
                Invalidate();
                TextChangedCustom?.Invoke(this, EventArgs.Empty);
            };

            Controls.Add(_innerTextBox);
            AdjustInnerTextBox();
        }

        private void AdjustInnerTextBox()
        {
            int padX = 14;
            int padY = _innerTextBox.Multiline ? 10 : (Height - _innerTextBox.PreferredHeight) / 2;
            _innerTextBox.Location = new Point(padX, padY);
            _innerTextBox.Size = new Size(Width - (padX * 2), _innerTextBox.Multiline ? Height - 20 : _innerTextBox.PreferredHeight);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AdjustInnerTextBox();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            _innerTextBox.Font = Font;
            AdjustInnerTextBox();
        }

        public void UpdateThemeColors()
        {
            _innerTextBox.BackColor = Theme.CardBackground;
            _innerTextBox.ForeColor = Theme.TextPrimary;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle rect = ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            Color bg = Theme.CardBackground;
            Color border = _isFocused ? Theme.GreenAccent : Theme.CardBorder;

            using (GraphicsPath path = Theme.CreateRoundedRectanglePath(rect, _borderRadius))
            {
                using (SolidBrush bgBrush = new SolidBrush(bg))
                {
                    g.FillPath(bgBrush, path);
                }
                using (Pen borderPen = new Pen(border, _isFocused ? 1.6f : 1f))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Draw placeholder text if empty and not focused
            if (string.IsNullOrEmpty(_innerTextBox.Text) && !string.IsNullOrEmpty(_placeholderText) && !_isFocused)
            {
                Rectangle phRect = new Rectangle(14, (Height - Font.Height) / 2, Width - 28, Font.Height);
                TextRenderer.DrawText(g, _placeholderText, Font, phRect, Theme.TextSecondary, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }
        }
    }
}

