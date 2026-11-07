using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RateMe.UI.Controls
{
    public enum PillButtonStyle
    {
        PrimaryFilled,   // Green Accent #00754A fill, White text
        PrimaryOutlined, // Outlined Green Accent border & text
        BlackFilled,     // Black #000000 fill, White text
        DarkOutlined,    // Outlined matching text color
        WhiteFilled,     // White fill, Green Accent text (for dark headers)
        WhiteOutlined,   // White border & text, transparent/dark background
        Destructive      // Red #C82014 fill, White text
    }

    public class PillButton : Control
    {
        private PillButtonStyle _style = PillButtonStyle.PrimaryFilled;
        private bool _isHovered = false;
        private bool _isPressed = false;
        private int _borderRadius = 50;

        [Category("Appearance")]
        [DefaultValue(PillButtonStyle.PrimaryFilled)]
        public PillButtonStyle Style
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }

        [Category("Appearance")]
        [DefaultValue(50)]
        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = value; Invalidate(); }
        }

        public PillButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Font = Theme.ButtonFont;
            Cursor = Cursors.Hand;
            Size = new Size(130, 36);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            if (mevent.Button == MouseButtons.Left)
            {
                _isPressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            _isPressed = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Find real non-transparent parent background by walking up the hierarchy
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

            if (_isPressed)
            {
                // Inset by 1px on press to simulate scale(0.95) tactile press
                rect.Inflate(-1, -1);
            }

            Color pillBgColor = parentBg;
            Color borderColor = Color.Transparent;
            Color textColor = Color.White;
            bool hasBorder = false;

            switch (_style)
            {
                case PillButtonStyle.PrimaryFilled:
                    pillBgColor = _isPressed ? Theme.StarbucksGreen : (_isHovered ? Theme.GreenUplift : Theme.GreenAccent);
                    textColor = Color.White;
                    break;

                case PillButtonStyle.PrimaryOutlined:
                    pillBgColor = _isHovered 
                        ? Color.FromArgb(Theme.IsDarkMode ? 60 : 35, Theme.GreenAccent) 
                        : parentBg;
                    borderColor = Theme.GreenAccent;
                    textColor = Theme.IsDarkMode ? Theme.GreenLight : Theme.GreenAccent;
                    hasBorder = true;
                    break;

                case PillButtonStyle.BlackFilled:
                    pillBgColor = _isPressed ? Color.FromArgb(50, 50, 50) : (_isHovered ? Color.FromArgb(30, 30, 30) : Theme.PureBlack);
                    textColor = Color.White;
                    break;

                case PillButtonStyle.DarkOutlined:
                    Color normalText = Theme.TextPrimary;
                    pillBgColor = _isHovered ? Color.FromArgb(25, normalText) : parentBg;
                    borderColor = normalText;
                    textColor = normalText;
                    hasBorder = true;
                    break;

                case PillButtonStyle.WhiteFilled:
                    pillBgColor = _isPressed ? Theme.Ceramic : (_isHovered ? Theme.NeutralWarm : Color.White);
                    borderColor = Color.White;
                    hasBorder = true;
                    textColor = Theme.StarbucksGreen;
                    break;

                case PillButtonStyle.WhiteOutlined:
                    pillBgColor = _isHovered ? Color.FromArgb(45, Color.White) : parentBg;
                    borderColor = _isHovered ? Color.White : Color.FromArgb(140, Color.White);
                    textColor = Color.White;
                    hasBorder = true;
                    break;

                case PillButtonStyle.Destructive:
                    pillBgColor = _isPressed ? Color.FromArgb(160, 20, 15) : (_isHovered ? Color.FromArgb(220, 40, 30) : Theme.DestructiveRed);
                    textColor = Color.White;
                    break;
            }

            // Fill rounded pill shape
            int radius = Math.Min(_borderRadius, rect.Height / 2);
            using (GraphicsPath path = Theme.CreateRoundedRectanglePath(rect, radius))
            {
                using (SolidBrush pillBrush = new SolidBrush(pillBgColor))
                {
                    g.FillPath(pillBrush, path);
                }

                if (hasBorder)
                {
                    using (Pen borderPen = new Pen(borderColor, 1.5f))
                    {
                        g.DrawPath(borderPen, path);
                    }
                }
            }

            // Draw crisp text centered
            TextRenderer.DrawText(g, Text, Font, rect, textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        }
    }
}
