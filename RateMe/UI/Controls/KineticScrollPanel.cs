using System;
using System.Drawing;
using System.Windows.Forms;

namespace RateMe.UI.Controls
{
    /// <summary>
    /// Smooth kinetic scrollable container inspired by Framer Motion smooth scrolling.
    /// Intercepts mouse wheel events and animates scroll position with exponential easing.
    /// </summary>
    public class KineticScrollPanel : Panel
    {
        private const int WM_MOUSEWHEEL = 0x020A;
        private readonly Timer _scrollTimer = new Timer();
        private float _targetScrollY = 0f;
        private float _currentScrollY = 0f;
        private bool _isAnimating = false;
        private const float EasingFactor = 0.22f; // Framer motion feel: responsive yet soft

        public KineticScrollPanel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);

            AutoScroll = true;
            _scrollTimer.Interval = 16; // ~60 FPS
            _scrollTimer.Tick += OnScrollTimerTick;
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            if (!_isAnimating)
            {
                _currentScrollY = AutoScrollPosition.Y * -1;
                _targetScrollY = _currentScrollY;
            }
        }

        public void SmoothScrollTo(int targetY)
        {
            int maxY = DisplayRectangle.Height - ClientRectangle.Height;
            if (maxY < 0) maxY = 0;

            _targetScrollY = Math.Max(0, Math.Min(maxY, targetY));
            if (!_isAnimating)
            {
                _isAnimating = true;
                _scrollTimer.Start();
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_MOUSEWHEEL)
            {
                int delta = (short)((m.WParam.ToInt64() >> 16) & 0xffff);
                int scrollStep = 90; // Pixels per wheel tick

                int maxY = DisplayRectangle.Height - ClientRectangle.Height;
                if (maxY < 0) maxY = 0;

                // Move target in opposite direction of wheel delta
                _targetScrollY -= (delta / 120f) * scrollStep;
                _targetScrollY = Math.Max(0, Math.Min(maxY, _targetScrollY));

                if (!_isAnimating)
                {
                    _isAnimating = true;
                    _scrollTimer.Start();
                }

                // Mark handled so WinForms doesn't jump abruptly
                m.Result = IntPtr.Zero;
                return;
            }

            base.WndProc(ref m);
        }

        private void OnScrollTimerTick(object sender, EventArgs e)
        {
            float diff = _targetScrollY - _currentScrollY;

            if (Math.Abs(diff) < 0.8f)
            {
                _currentScrollY = _targetScrollY;
                _scrollTimer.Stop();
                _isAnimating = false;
            }
            else
            {
                _currentScrollY += diff * EasingFactor;
            }

            AutoScrollPosition = new Point(0, (int)Math.Round(_currentScrollY));
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _scrollTimer.Stop();
                _scrollTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

