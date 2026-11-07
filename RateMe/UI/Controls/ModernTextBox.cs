using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RateMe.UI.Controls
{
    public class ModernTextBox : Control
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        private readonly TextBox _innerTextBox = new TextBox();
        private string _placeholderText = "";
        private bool _isFocused = false;
        private int _borderRadius = 8;
        private bool _isPassword = false;

        [Category("Appearance")]
        public string PlaceholderText
        {
            get => _placeholderText;
            set
            {
                _placeholderText = value ?? "";
                UpdateCueBanner();
                Invalidate();
            }
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
                     ControlStyles.ResizeRedraw, true);

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

            _innerTextBox.HandleCreated += (s, e) => UpdateCueBanner();

            Controls.Add(_innerTextBox);
            AdjustInnerTextBox();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateCueBanner();
        }

        private void UpdateCueBanner()
        {
            if (_innerTextBox.IsHandleCreated && !string.IsNullOrEmpty(_placeholderText))
            {
                SendMessage(_innerTextBox.Handle, EM_SETCUEBANNER, 1, _placeholderText);
            }
        }

        private void AdjustInnerTextBox()
        {
            int padX = 12;
            int padY = _innerTextBox.Multiline ? 10 : (Height - _innerTextBox.PreferredHeight) / 2;
            _innerTextBox.Location = new Point(padX, padY);
            _innerTextBox.Size = new Size(Math.Max(10, Width - (padX * 2)), _innerTextBox.Multiline ? Height - 20 : _innerTextBox.PreferredHeight);
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

            Color parentBg = Theme.PageCanvas;
            Control cur = Parent;
            while (cur != null)
            {
                if (cur.BackColor != Color.Transparent && cur.BackColor != Color.Empty && cur.BackColor.A > 0)
                {
                    parentBg = cur.BackColor;
                    break;
                }
                cur = cur.Parent;
            }

            g.Clear(parentBg);

            Rectangle rect = ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            Color bg = Theme.CardBackground;
            Color border = _isFocused ? Theme.GreenAccent : Color.FromArgb(200, 205, 208);

            using (GraphicsPath path = Theme.CreateRoundedRectanglePath(rect, _borderRadius))
            {
                using (SolidBrush bgBrush = new SolidBrush(bg))
                {
                    g.FillPath(bgBrush, path);
                }
                using (Pen borderPen = new Pen(border, _isFocused ? 1.8f : 1.2f))
                {
                    g.DrawPath(borderPen, path);
                }
            }
        }
    }
}
