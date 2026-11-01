using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RateMe.UI
{
    public static class Theme
    {
        // -------------------------------------------------------------
        // Starbucks Design Tokens (rate-me-design.md)
        // -------------------------------------------------------------
        public static readonly Color StarbucksGreen = Color.FromArgb(0, 98, 65);     // #006241 - Brand heading / primary signal
        public static readonly Color GreenAccent    = Color.FromArgb(0, 117, 74);    // #00754A - Filled CTAs & active elements
        public static readonly Color HouseGreen     = Color.FromArgb(30, 57, 50);    // #1E3932 - Hero band, dark headers
        public static readonly Color GreenUplift    = Color.FromArgb(43, 81, 72);    // #2B5148 - Decorative accents
        public static readonly Color GreenLight     = Color.FromArgb(212, 233, 226); // #D4E9E2 - Soft mint tint

        public static readonly Color Gold           = Color.FromArgb(203, 162, 88);  // #CBA258 - Rating stars, rewards badge
        public static readonly Color GoldLight      = Color.FromArgb(223, 196, 157); // #DFC49D - Soft gold
        public static readonly Color GoldLightest   = Color.FromArgb(250, 246, 238); // #FAF6EE - Warm cream wash

        public static readonly Color NeutralWarm    = Color.FromArgb(242, 240, 235); // #F2F0EB - Primary page canvas (Light)
        public static readonly Color Ceramic        = Color.FromArgb(237, 235, 233); // #EDEBE9 - Surface separators
        public static readonly Color White          = Color.FromArgb(255, 255, 255); // #FFFFFF - Card canvas

        public static readonly Color PureBlack      = Color.FromArgb(0, 0, 0);       // #000000 - High-contrast accents
        public static readonly Color TextBlack      = Color.FromArgb(34, 34, 34);    // rgba(0,0,0,0.87) equivalent
        public static readonly Color TextBlackSoft  = Color.FromArgb(105, 105, 105); // rgba(0,0,0,0.58) equivalent

        public static readonly Color TextWhite      = Color.FromArgb(255, 255, 255); // Primary text on dark surfaces
        public static readonly Color TextWhiteSoft  = Color.FromArgb(178, 255, 255, 255); // rgba(255,255,255,0.70)

        public static readonly Color DestructiveRed = Color.FromArgb(200, 32, 20);   // #C82014 - Errors / delete
        public static readonly Color BorderLight    = Color.FromArgb(224, 222, 217); // Soft card/input border

        // Dark Mode tokens (House Green inspired dark theme)
        public static readonly Color DarkCanvas     = Color.FromArgb(18, 35, 31);    // #12231F - Deep espresso green canvas
        public static readonly Color DarkSurface    = Color.FromArgb(24, 46, 40);    // #182E28 - Navigation & headers
        public static readonly Color DarkCard       = Color.FromArgb(28, 52, 45);    // #1C342D - Card background
        public static readonly Color DarkCardHover  = Color.FromArgb(36, 64, 56);
        public static readonly Color DarkBorder     = Color.FromArgb(45, 78, 68);    // Card border in dark mode

        // -------------------------------------------------------------
        // Current Mode State
        // -------------------------------------------------------------
        public static bool IsDarkMode { get; set; } = false;

        public static event Action ThemeChanged;

        public static void SetDarkMode(bool darkMode)
        {
            if (IsDarkMode != darkMode)
            {
                IsDarkMode = darkMode;
                ThemeChanged?.Invoke();
            }
        }

        // Dynamic Theme Properties based on Light / Dark mode
        public static Color PageCanvas => IsDarkMode ? DarkCanvas : NeutralWarm;
        public static Color Surface => IsDarkMode ? DarkSurface : Ceramic;
        public static Color CardBackground => IsDarkMode ? DarkCard : White;
        public static Color CardBorder => IsDarkMode ? DarkBorder : BorderLight;
        public static Color TextPrimary => IsDarkMode ? TextWhite : TextBlack;
        public static Color TextSecondary => IsDarkMode ? TextWhiteSoft : TextBlackSoft;

        // -------------------------------------------------------------
        // Fonts (Clean geometric sans Segoe UI + editorial Georgia)
        // -------------------------------------------------------------
        public static readonly string PrimaryFontFamily = "Segoe UI";
        public static readonly string SerifFontFamily = "Georgia";

        public static Font HeaderDisplay => new Font(SerifFontFamily, 22f, FontStyle.Bold);
        public static Font HeaderH1 => new Font(PrimaryFontFamily, 16f, FontStyle.Bold);
        public static Font HeaderH2 => new Font(PrimaryFontFamily, 13f, FontStyle.Bold);
        public static Font BodyLarge => new Font(PrimaryFontFamily, 11f, FontStyle.Regular);
        public static Font Body => new Font(PrimaryFontFamily, 9.5f, FontStyle.Regular);
        public static Font BodyBold => new Font(PrimaryFontFamily, 9.5f, FontStyle.Bold);
        public static Font Small => new Font(PrimaryFontFamily, 8.5f, FontStyle.Regular);
        public static Font SmallBold => new Font(PrimaryFontFamily, 8.5f, FontStyle.Bold);
        public static Font ButtonFont => new Font(PrimaryFontFamily, 9.5f, FontStyle.Bold);

        // -------------------------------------------------------------
        // Graphics Drawing Helpers (Anti-aliased Rounded Shapes)
        // -------------------------------------------------------------
        public static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            int diameter = radius * 2;
            if (diameter > bounds.Width) diameter = bounds.Width;
            if (diameter > bounds.Height) diameter = bounds.Height;

            Rectangle arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

            // Top-left
            path.AddArc(arc, 180, 90);

            // Top-right
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom-right
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom-left
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        public static void FillRoundedRectangle(Graphics g, Brush brush, Rectangle bounds, int radius)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = CreateRoundedRectanglePath(bounds, radius))
            {
                g.FillPath(brush, path);
            }
        }

        public static void DrawRoundedRectangle(Graphics g, Pen pen, Rectangle bounds, int radius)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = CreateRoundedRectanglePath(bounds, radius))
            {
                g.DrawPath(pen, path);
            }
        }

        public static void ApplyThemeRecursively(Control root)
        {
            if (root == null) return;

            root.BackColor = PageCanvas;
            root.ForeColor = TextPrimary;

            foreach (Control child in root.Controls)
            {
                ApplyThemeRecursively(child);
            }
        }
    }
}

