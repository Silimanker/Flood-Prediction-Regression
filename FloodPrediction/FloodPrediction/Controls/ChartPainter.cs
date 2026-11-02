using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FloodPrediction.Controls
{
    /// <summary>
    /// Small shared painting helper for the hand-drawn charts: margins, value-to-pixel
    /// mapping, "nice" axis ticks and grid/label rendering. No third-party charting
    /// library is used — everything here is plain GDI+.
    /// </summary>
    internal static class ChartPainter
    {
        public const int MarginLeft = 72;
        public const int MarginRight = 18;
        public const int MarginTop = 28;
        public const int MarginBottom = 40;

        /// <summary>The rectangle available for plotting, after margins.</summary>
        public static RectangleF PlotArea(Size clientSize)
        {
            float w = clientSize.Width - MarginLeft - MarginRight;
            float h = clientSize.Height - MarginTop - MarginBottom;
            if (w < 20) w = 20;
            if (h < 20) h = 20;
            return new RectangleF(MarginLeft, MarginTop, w, h);
        }

        /// <summary>Maps a data value onto the horizontal pixel coordinate.</summary>
        public static float ToPixelX(double value, double min, double max, RectangleF plot)
        {
            if (max <= min) return plot.Left;
            return plot.Left + (float)((value - min) / (max - min) * plot.Width);
        }

        /// <summary>Maps a data value onto the vertical pixel coordinate.</summary>
        public static float ToPixelY(double value, double min, double max, RectangleF plot)
        {
            if (max <= min) return plot.Bottom;
            return plot.Bottom - (float)((value - min) / (max - min) * plot.Height);
        }

        /// <summary>
        /// Produces evenly spaced, human-friendly tick positions between
        /// <paramref name="min"/> and <paramref name="max"/>.
        /// </summary>
        public static double[] NiceTicks(double min, double max, int targetCount)
        {
            if (double.IsNaN(min) || double.IsNaN(max) || max <= min)
                return new double[] { min };

            double span = max - min;
            double rough = span / Math.Max(1, targetCount);
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(rough)));
            double normalised = rough / magnitude;

            double step;
            if (normalised <= 1.0) step = 1.0;
            else if (normalised <= 2.0) step = 2.0;
            else if (normalised <= 2.5) step = 2.5;
            else if (normalised <= 5.0) step = 5.0;
            else step = 10.0;
            step *= magnitude;

            double start = Math.Ceiling(min / step) * step;
            var ticks = new List<double>();
            for (double t = start; t <= max + step * 1e-6; t += step)
            {
                ticks.Add(t);
                if (ticks.Count > 50) break;   // safety against pathological ranges
            }
            if (ticks.Count == 0) ticks.Add(min);
            return ticks.ToArray();
        }

        /// <summary>Picks a decimal format that makes a tick step readable.</summary>
        public static string FormatForStep(double step)
        {
            if (step >= 1.0 || step <= 0) return "0";
            int decimals = (int)Math.Ceiling(-Math.Log10(step)) + 1;
            if (decimals < 2) decimals = 2;
            if (decimals > 8) decimals = 8;
            return "0." + new string('0', decimals);
        }

        /// <summary>
        /// Draws the plot border, grid lines, tick labels and axis titles.
        /// </summary>
        public static void DrawFrame(Graphics g, RectangleF plot,
                                     double xmin, double xmax, double ymin, double ymax,
                                     int xDivisions, int yDivisions,
                                     string xTitle, string yTitle, Font font)
        {
            double[] xticks = NiceTicks(xmin, xmax, xDivisions);
            double[] yticks = NiceTicks(ymin, ymax, yDivisions);
            string yFormat = FormatForStep(yticks.Length > 1 ? yticks[1] - yticks[0] : 0);
            string xFormat = FormatForStep(xticks.Length > 1 ? xticks[1] - xticks[0] : 0);

            using (var gridPen = new Pen(Color.FromArgb(232, 232, 232)))
            using (var tickBrush = new SolidBrush(Color.FromArgb(90, 90, 90)))
            using (var borderPen = new Pen(Color.FromArgb(170, 170, 170)))
            {
                // Horizontal grid lines + Y tick labels
                foreach (double t in yticks)
                {
                    float y = ToPixelY(t, ymin, ymax, plot);
                    if (y < plot.Top - 1 || y > plot.Bottom + 1) continue;
                    g.DrawLine(gridPen, plot.Left, y, plot.Right, y);

                    string label = t.ToString(yFormat);
                    var size = g.MeasureString(label, font);
                    g.DrawString(label, font, tickBrush, plot.Left - 8 - size.Width, y - size.Height / 2f);
                }

                // Vertical grid lines + X tick labels
                foreach (double t in xticks)
                {
                    float x = ToPixelX(t, xmin, xmax, plot);
                    if (x < plot.Left - 1 || x > plot.Right + 1) continue;
                    g.DrawLine(gridPen, x, plot.Top, x, plot.Bottom);

                    string label = t.ToString(xFormat);
                    var size = g.MeasureString(label, font);
                    g.DrawString(label, font, tickBrush, x - size.Width / 2f, plot.Bottom + 6);
                }

                g.DrawRectangle(borderPen, plot.X, plot.Y, plot.Width, plot.Height);
            }

            DrawAxisTitle(g, plot, xTitle, yTitle, font);
        }

        private static void DrawAxisTitle(Graphics g, RectangleF plot, string xTitle, string yTitle, Font font)
        {
            using (var brush = new SolidBrush(Color.FromArgb(110, 110, 110)))
            {
                // X title: centred under the plot.
                if (!string.IsNullOrEmpty(xTitle))
                {
                    var size = g.MeasureString(xTitle, font);
                    g.DrawString(xTitle, font, brush,
                        plot.Left + (plot.Width - size.Width) / 2f,
                        plot.Bottom + 22);
                }

                // Y title: rotated 90 degrees, centred to the left of the plot.
                if (!string.IsNullOrEmpty(yTitle))
                {
                    var state = g.Save();
                    g.TranslateTransform(plot.Left - 54, plot.Top + plot.Height / 2f);
                    g.RotateTransform(-90f);
                    var sf = new StringFormat();
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    g.DrawString(yTitle, font, brush,
                        new RectangleF(-plot.Height / 2f, -10, plot.Height, 20), sf);
                    sf.Dispose();
                    g.Restore(state);
                }
            }
        }

        /// <summary>Draws the "nothing to show yet" placeholder.</summary>
        public static void DrawEmpty(Graphics g, Rectangle clip, string message, Font font)
        {
            using (var brush = new SolidBrush(Color.FromArgb(150, 150, 150)))
            {
                var sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                g.DrawString(message, font, brush, clip, sf);
                sf.Dispose();
            }
        }

        /// <summary>Draws a small two-item legend in the top-right corner of the plot.</summary>
        public static void DrawLegend(Graphics g, RectangleF plot, Font font,
                                      string label1, Color color1, string label2, Color color2)
        {
            float x = plot.Right - 12;
            float y = plot.Top + 8;

            using (var textBrush = new SolidBrush(Color.FromArgb(70, 70, 70)))
            {
                // Measure right-to-left so the legend never leaves the plot area.
                float w2 = g.MeasureString(label2, font).Width;
                float w1 = g.MeasureString(label1, font).Width;
                float gap = 16;      // colour swatch + spacing
                float total = w1 + gap + w2 + gap;

                float left = x - total;

                DrawSwatch(g, left, y + 3, color1);
                g.DrawString(label1, font, textBrush, left + 12, y);

                float left2 = left + w1 + gap;
                DrawSwatch(g, left2, y + 3, color2);
                g.DrawString(label2, font, textBrush, left2 + 12, y);
            }
        }

        private static void DrawSwatch(Graphics g, float x, float y, Color color)
        {
            using (var pen = new Pen(color, 2.5f))
            {
                g.DrawLine(pen, x, y + 4, x + 10, y + 4);
            }
        }
    }
}
