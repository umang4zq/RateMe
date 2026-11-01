using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RateMe.UI.Controls
{
    public class ToastNotification : Control
    {
        private string _message = "";
        private readonly Timer _animTimer = new Timer();
        private readonly Timer _lifeTimer = new Timer();
        private float _targetY = 20f;
        private float _currentY = -60f;
        private bool _isDismissing = false;
        private bool _isSuccess = true;

        public ToastNotification()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.ResizeRedraw, true);

            Size = new Size(340, 48);
            BackColor = Color.Transparent;
            Visible = false;

            _animTimer.Interval = 16;
            _animTimer.Tick += OnAnimTick;

            _lifeTimer.Interval = 2500;
            _lifeTimer.Tick += (s, e) =>
            {
                _lifeTimer.Stop();
                Dismiss();
            };
        }

        public void ShowToast(Form parent, string message, bool isSuccess = true)
        {
            _message = message;
            _isSuccess = isSuccess;
            _isDismissing = false;

            if (Parent != parent)
            {
                parent.Controls.Add(this);
                BringToFront();
            }

            Location = new Point((parent.ClientSize.Width - Width) / 2, -Height);
            _currentY = -Height;
            _targetY = 24f;
            Visible = true;
            BringToFront();

            _animTimer.Start();
            _lifeTimer.Stop();
        }

        public void Dismiss()
        {
            _isDismissing = true;
            _targetY = -Height - 10;
            _animTimer.Start();
        }

        private void OnAnimTick(object sender, EventArgs e)
        {
            float diff = _targetY - _currentY;
            if (Math.Abs(diff) < 0.8f)
            {
                _currentY = _targetY;
                _animTimer.Stop();

                if (_isDismissing)
                {
                    Visible = false;
                }
                else
                {
                    _lifeTimer.Start();
                }
            }
            else
            {
                _currentY += diff * 0.25f;
            }

            if (Parent != null)
            {
                Location = new Point((Parent.ClientSize.Width - Width) / 2, (int)_currentY);
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            // Draw soft shadow
            using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
            {
                Rectangle sRect = new Rectangle(rect.X + 2, rect.Y + 3, rect.Width - 4, rect.Height - 3);
                using (GraphicsPath spath = Theme.CreateRoundedRectanglePath(sRect, 24))
                {
                    g.FillPath(shadowBrush, spath);
                }
            }

            // Fill Toast Pill
            Color bg = _isSuccess ? Theme.HouseGreen : Theme.DestructiveRed;
            using (GraphicsPath path = Theme.CreateRoundedRectanglePath(new Rectangle(rect.X, rect.Y, rect.Width, rect.Height - 2), 24))
            {
                using (SolidBrush brush = new SolidBrush(bg))
                {
                    g.FillPath(brush, path);
                }

                using (Pen borderPen = new Pen(_isSuccess ? Theme.Gold : Color.White, 1.2f))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Icon prefix
            string prefix = _isSuccess ? "★ " : "⚠ ";
            string fullText = prefix + _message;

            Rectangle textRect = new Rectangle(16, 0, Width - 32, Height - 2);
            TextRenderer.DrawText(g, fullText, Theme.BodyBold, textRect, Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animTimer.Stop();
                _animTimer.Dispose();
                _lifeTimer.Stop();
                _lifeTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
