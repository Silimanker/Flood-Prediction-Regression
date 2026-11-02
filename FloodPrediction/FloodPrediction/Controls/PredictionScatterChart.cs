using System;
using System.Drawing;
using System.Windows.Forms;

namespace FloodPrediction.Controls
{
    /// <summary>
    /// Predicted-vs-actual scatter plot for the held-out test set, with a dashed y = x
    /// reference line. Points hugging the diagonal indicate an accurate regressor.
    /// </summary>
    public sealed class PredictionScatterChart : UserControl
    {
        /// <summary>Cap on the number of drawn points so repaints stay instant.</summary>
        private const int MaxDrawnPoints = 4000;

        private static readonly Color PointColor = Color.FromArgb(120, 45, 110, 200);
        private static readonly Color DiagonalColor = Color.FromArgb(160, 140, 140, 140);

        private double[] _actual;
        private double[] _predicted;

        public PredictionScatterChart()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.UserPaint
                   | ControlStyles.ResizeRedraw, true);

            BackColor = Color.White;
        }

        /// <summary>Replaces the plotted points and repaints.</summary>
        public void SetData(double[] actual, double[] predicted)
        {
            _actual = actual;
            _predicted = predicted;
            Invalidate();
        }

        /// <summary>Clears the chart.</summary>
        public void Clear()
        {
            _actual = null;
            _predicted = null;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            Font font = Font;
            RectangleF plot = ChartPainter.PlotArea(ClientSize);

            if (_actual == null || _predicted == null || _actual.Length == 0)
            {
                ChartPainter.DrawEmpty(g, ClientRectangle,
                    "No predictions yet.\nPress \"Train\" to start.", font);
                return;
            }

            // Both axes are FloodProbability, so share one range to keep y = x at 45 degrees.
            double dmin = double.PositiveInfinity, dmax = double.NegativeInfinity;
            for (int i = 0; i < _actual.Length; i++)
            {
                if (_actual[i] < dmin) dmin = _actual[i];
                if (_predicted[i] < dmin) dmin = _predicted[i];
                if (_actual[i] > dmax) dmax = _actual[i];
                if (_predicted[i] > dmax) dmax = _predicted[i];
            }
            if (double.IsInfinity(dmin)) { dmin = 0; dmax = 1; }

            double pad = (dmax - dmin) * 0.05;
            if (pad <= 0) pad = 0.01;
            dmin -= pad;
            dmax += pad;

            ChartPainter.DrawFrame(g, plot, dmin, dmax, dmin, dmax, 5, 5,
                                   "Actual FloodProbability", "Predicted FloodProbability", font);

            // ---- dashed y = x reference --------------------------------------
            using (var diagPen = new Pen(DiagonalColor, 1f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
            {
                g.DrawLine(diagPen,
                    ChartPainter.ToPixelX(dmin, dmin, dmax, plot),
                    ChartPainter.ToPixelY(dmin, dmin, dmax, plot),
                    ChartPainter.ToPixelX(dmax, dmin, dmax, plot),
                    ChartPainter.ToPixelY(dmax, dmin, dmax, plot));
            }

            // ---- points -------------------------------------------------------
            int count = _actual.Length;
            int step = (count + MaxDrawnPoints - 1) / MaxDrawnPoints;
            if (step < 1) step = 1;

            float radius = count > 8000 ? 2f : 3f;

            using (var brush = new SolidBrush(PointColor))
            {
                for (int i = 0; i < count; i += step)
                {
                    float x = ChartPainter.ToPixelX(_actual[i], dmin, dmax, plot);
                    float y = ChartPainter.ToPixelY(_predicted[i], dmin, dmax, plot);
                    g.FillEllipse(brush, x - radius, y - radius, radius * 2f, radius * 2f);
                }
            }

            // ---- diagonal legend note ----------------------------------------
            using (var textBrush = new SolidBrush(Color.FromArgb(120, 120, 120)))
            {
                string note = "dashed = perfect prediction";
                var size = g.MeasureString(note, font);
                g.DrawString(note, font, textBrush, plot.Right - size.Width - 8, plot.Bottom - size.Height - 8);
            }
        }
    }
}
