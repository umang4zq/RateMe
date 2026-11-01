using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RateMe.UI.Controls
{
    public class RatingBadge : Control
    {
        private string _ratingText = "8.8 ★";
        private bool _isGold = true;

        [Category("Appearance")]
        [DefaultValue("8.8 ★")]
        public string RatingText
        {
            get => _ratingText;
            set { _ratingText = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool IsGold
        {
            get => _isGold;
            set { _isGold = value; Invalidate(); }
        }

        public RatingBadge()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Font = Theme.SmallBold;
            Size = new Size(68, 24);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Clear parent background first to eliminate white corner artifacts
            Color parentBg = Parent != null ? Parent.BackColor : Theme.PageCanvas;
            using (SolidBrush parentBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(parentBrush, ClientRectangle);
            }

            Rectangle rect = ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            Color accentColor = _isGold ? Theme.Gold : Theme.GreenAccent;
            Color bgColor = _isGold 
                ? (Theme.IsDarkMode ? Color.FromArgb(50, Theme.Gold) : Theme.GoldLightest) 
                : (Theme.IsDarkMode ? Color.FromArgb(50, Theme.GreenAccent) : Theme.GreenLight);

            int radius = rect.Height / 2;
            using (GraphicsPath path = Theme.CreateRoundedRectanglePath(rect, radius))
            {
                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                {
                    g.FillPath(bgBrush, path);
                }
                using (Pen borderPen = new Pen(accentColor, 1.2f))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            TextRenderer.DrawText(g, _ratingText, Font, rect, accentColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
}
