using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RateMe.Services
{
    public static class ImageCache
    {
        private static readonly ConcurrentDictionary<string, Image> _cache = new ConcurrentDictionary<string, Image>();
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<Image> GetImageAsync(string url, string titleFallback = "RateMe", int width = 140, int height = 200)
        {
            if (string.IsNullOrWhiteSpace(url) || url.Equals("N/A", StringComparison.OrdinalIgnoreCase))
            {
                return GetPlaceholder(titleFallback, width, height);
            }

            if (_cache.TryGetValue(url, out Image cachedImage))
            {
                return cachedImage;
            }

            try
            {
                byte[] bytes = await _httpClient.GetByteArrayAsync(url).ConfigureAwait(false);
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    Image original = Image.FromStream(ms);
                    Bitmap clone = new Bitmap(original);
                    _cache[url] = clone;
                    return clone;
                }
            }
            catch
            {
                return GetPlaceholder(titleFallback, width, height);
            }
        }

        public static Image GetPlaceholder(string title, int width = 140, int height = 200)
        {
            string cacheKey = $"_placeholder_{title}_{width}_{height}";
            if (_cache.TryGetValue(cacheKey, out Image cached))
            {
                return cached;
            }

            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // House Green background with slight gradient
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    new Rectangle(0, 0, width, height),
                    UI.Theme.HouseGreen,
                    Color.FromArgb(20, 42, 36),
                    LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, 0, 0, width, height);
                }

                // Gold border
                using (Pen pen = new Pen(UI.Theme.Gold, 2f))
                {
                    g.DrawRectangle(pen, 1, 1, width - 2, height - 2);
                }

                // Decorative Gold Star in center
                float cx = width / 2f;
                float cy = height * 0.40f;
                DrawStar(g, cx, cy, Math.Min(width, height) * 0.18f, UI.Theme.Gold);

                // Title label at bottom
                string displayTitle = title ?? "RateMe";
                Rectangle textRect = new Rectangle(8, (int)(height * 0.65f), width - 16, (int)(height * 0.32f));
                using (Font font = new Font(UI.Theme.PrimaryFontFamily, 9.5f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, displayTitle, font, textRect, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);
                }
            }

            _cache[cacheKey] = bmp;
            return bmp;
        }

        private static void DrawStar(Graphics g, float cx, float cy, float radius, Color color)
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

            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillPolygon(brush, points);
            }
        }
    }
}
