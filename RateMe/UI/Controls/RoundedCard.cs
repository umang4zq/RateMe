using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RateMe.UI.Controls
{
    public class RoundedCard : Panel
    {
        private int _borderRadius = 12;
        private Color _customBackColor = Color.Empty;
        private Color _customBorderColor = Color.Empty;
        private bool _drawBorder = true;
        private bool _drawShadow = true;
        private bool _isHovered = false;
        private bool _hoverElevate = false;

        [Category("Appearance")]
        [DefaultValue(12)]
        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool DrawBorder
        {
            get => _drawBorder;
            set { _drawBorder = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool DrawShadow
        {
            get => _drawShadow;
            set { _drawShadow = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue(false)]
        public bool HoverElevate
        {
            get => _hoverElevate;
            set { _hoverElevate = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color CustomBackColor
        {
            get => _customBackColor;
            set { _customBackColor = value; Invalidate(); }
        }

        public RoundedCard()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Padding = new Padding(16);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (_hoverElevate)
            {
                _isHovered = true;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoverElevate)
            {
                _isHovered = false;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Clear parent background first to eliminate white corner artifacts
            Color parentBg = Parent != null ? Parent.BackColor : Theme.PageCanvas;
            using (SolidBrush parentBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(parentBrush, ClientRectangle);
            }

            Rectangle rect = ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            int shadowMargin = _drawShadow ? 3 : 1;
            Rectangle cardRect = new Rectangle(
                rect.X + shadowMargin,
                rect.Y + (_drawShadow && _isHovered ? 1 : 2),
                rect.Width - (shadowMargin * 2),
                rect.Height - (shadowMargin * 2)
            );

            if (_drawShadow)
            {
                int shadowAlpha1 = _isHovered ? 35 : 18;
                int shadowAlpha2 = _isHovered ? 45 : 25;

                using (SolidBrush shadow1 = new SolidBrush(Color.FromArgb(shadowAlpha1, 0, 0, 0)))
                {
                    Rectangle s1 = new Rectangle(cardRect.X, cardRect.Y + 2, cardRect.Width, cardRect.Height);
                    using (GraphicsPath p1 = Theme.CreateRoundedRectanglePath(s1, _borderRadius))
                    {
                        g.FillPath(shadow1, p1);
                    }
                }

                using (SolidBrush shadow2 = new SolidBrush(Color.FromArgb(shadowAlpha2, 0, 0, 0)))
                {
                    Rectangle s2 = new Rectangle(cardRect.X, cardRect.Y + 4, cardRect.Width, cardRect.Height);
                    using (GraphicsPath p2 = Theme.CreateRoundedRectanglePath(s2, _borderRadius))
                    {
                        g.FillPath(shadow2, p2);
                    }
                }
            }

            Color cardBg = _customBackColor != Color.Empty 
                ? _customBackColor 
                : (_isHovered && Theme.IsDarkMode ? Theme.DarkCardHover : Theme.CardBackground);

            Color borderCol = _customBorderColor != Color.Empty 
                ? _customBorderColor 
                : (_isHovered ? Theme.GreenAccent : Theme.CardBorder);

            using (GraphicsPath path = Theme.CreateRoundedRectanglePath(cardRect, _borderRadius))
            {
                using (SolidBrush bgBrush = new SolidBrush(cardBg))
                {
                    g.FillPath(bgBrush, path);
                }

                if (_drawBorder)
                {
                    using (Pen borderPen = new Pen(borderCol, 1f))
                    {
                        g.DrawPath(borderPen, path);
                    }
                }
            }
        }
    }
}
