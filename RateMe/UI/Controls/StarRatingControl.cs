using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RateMe.UI.Controls
{
    public class StarRatingControl : Control
    {
        private int _maxStars = 10;
        private decimal _value = 0m;
        private int _hoveredStar = -1;
        private int _starSize = 22;
        private int _starGap = 6;
        private bool _isReadOnly = false;

        [Category("Behavior")]
        [DefaultValue(10)]
        public int MaxStars
        {
            get => _maxStars;
            set { _maxStars = Math.Max(1, value); Invalidate(); }
        }

        [Category("Behavior")]
        [DefaultValue(0)]
        public decimal Value
        {
            get => _value;
            set
            {
                _value = Math.Max(0, Math.Min(_maxStars, value));
                Invalidate();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set { _isReadOnly = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue(22)]
        public int StarSize
        {
            get => _starSize;
            set { _starSize = value; Invalidate(); }
        }

        public event EventHandler ValueChanged;

        public StarRatingControl()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Cursor = Cursors.Hand;
            Size = new Size(280, 32);
        }

        public string GetRatingLabel()
        {
            decimal score = _hoveredStar > 0 ? _hoveredStar : _value;
            if (score <= 0) return "Click to rate (1 - 10)";
            if (score <= 2) return $"{score:0.0} - Poor";
            if (score <= 4) return $"{score:0.0} - Fair";
            if (score <= 6) return $"{score:0.0} - Average";
            if (score <= 7) return $"{score:0.0} - Good";
            if (score <= 8.5m) return $"{score:0.0} - Great";
            if (score <= 9.5m) return $"{score:0.0} - Outstanding";
            return $"{score:0.0} - Masterpiece ★";
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isReadOnly) return;

            int newHover = GetStarIndexAt(e.X);
            if (newHover != _hoveredStar)
            {
                _hoveredStar = newHover;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_isReadOnly) return;

            _hoveredStar = -1;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (_isReadOnly || e.Button != MouseButtons.Left) return;

            int star = GetStarIndexAt(e.X);
            if (star > 0)
            {
                Value = star;
            }
        }

        private int GetStarIndexAt(int mouseX)
        {
            int totalStarWidth = _starSize + _starGap;
            int index = (mouseX / totalStarWidth) + 1;
            if (index < 1) return 1;
            if (index > _maxStars) return _maxStars;
            return index;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Clear parent background first
            Color parentBg = Parent != null ? Parent.BackColor : Theme.PageCanvas;
            using (SolidBrush parentBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(parentBrush, ClientRectangle);
            }

            decimal activeStars = _hoveredStar > 0 ? _hoveredStar : _value;

            for (int i = 1; i <= _maxStars; i++)
            {
                int startX = (i - 1) * (_starSize + _starGap);
                int startY = (Height - _starSize) / 2;

                bool isFilled = i <= activeStars;
                Color starColor = isFilled ? Theme.Gold : (Theme.IsDarkMode ? Color.FromArgb(80, 110, 95) : Color.FromArgb(200, 195, 185));

                DrawStar(g, startX + _starSize / 2f, startY + _starSize / 2f, _starSize / 2f - 1, isFilled, starColor);
            }
        }

        private void DrawStar(Graphics g, float cx, float cy, float radius, bool filled, Color color)
        {
            PointF[] points = new PointF[10];
            double angle = -Math.PI / 2.0;
            double step = Math.PI / 5.0;
            float innerRadius = radius * 0.45f;

            for (int i = 0; i < 10; i++)
            {
                float r = (i % 2 == 0) ? radius : innerRadius;
                points[i] = new PointF(
                    (float)(cx + r * Math.Cos(angle)),
                    (float)(cy + r * Math.Sin(angle))
                );
                angle += step;
            }

            if (filled)
            {
                using (SolidBrush brush = new SolidBrush(color))
                {
                    g.FillPolygon(brush, points);
                }
            }
            else
            {
                using (Pen pen = new Pen(color, 1.5f))
                {
                    g.DrawPolygon(pen, points);
                }
            }
        }
    }
}
