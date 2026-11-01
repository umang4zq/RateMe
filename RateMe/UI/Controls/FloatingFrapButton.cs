using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RateMe.UI.Controls
{
    public class FloatingFrapButton : Control
    {
        private bool _isHovered = false;
        private bool _isPressed = false;
        private readonly ToolTip _toolTip = new ToolTip();

        public FloatingFrapButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            Size = new Size(56, 56);
            Cursor = Cursors.Hand;
            _toolTip.SetToolTip(this, "Quick Action / Random Pick (Frap)");
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

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _isPressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isPressed = false;
            Invalidate();
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

            Rectangle circleRect = new Rectangle(4, 4, 48, 48);
            if (_isPressed)
            {
                circleRect.Inflate(-1, -1);
            }

            // Draw shadow stack
            if (!_isPressed)
            {
                using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(_isHovered ? 45 : 25, 0, 0, 0)))
                {
                    g.FillEllipse(shadowBrush, new Rectangle(3, 7, 50, 48));
                }
                using (SolidBrush shadowBrush2 = new SolidBrush(Color.FromArgb(_isHovered ? 60 : 40, 0, 0, 0)))
                {
                    g.FillEllipse(shadowBrush2, new Rectangle(4, 5, 48, 48));
                }
            }

            // Fill Green Accent #00754A circle
            Color fillColor = _isPressed ? Theme.StarbucksGreen : (_isHovered ? Theme.GreenUplift : Theme.GreenAccent);
            using (SolidBrush fillBrush = new SolidBrush(fillColor))
            {
                g.FillEllipse(fillBrush, circleRect);
            }

            // Draw Star Icon inside
            DrawStarIcon(g, circleRect.X + circleRect.Width / 2, circleRect.Y + circleRect.Height / 2, 11);
        }

        private void DrawStarIcon(Graphics g, float cx, float cy, float radius)
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

            using (SolidBrush starBrush = new SolidBrush(Color.White))
            {
                g.FillPolygon(starBrush, points);
            }
        }
    }
}
