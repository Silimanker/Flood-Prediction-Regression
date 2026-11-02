using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FloodPrediction.Training;

namespace FloodPrediction.Controls
{
    /// <summary>
    /// Live loss curve: plots training-set MSE and held-out test-set MSE against epoch
    /// so it is immediately visible that the loss is decreasing (training-log
    /// deliverable, rendered on screen).
    /// </summary>
    public sealed class LossCurveChart : UserControl
    {
        private static readonly Color TrainColor = Color.FromArgb(30, 100, 200);
        private static readonly Color TestColor = Color.FromArgb(220, 110, 30);

        private List<EpochResult> _history;

        public LossCurveChart()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.UserPaint
                   | ControlStyles.ResizeRedraw, true);

            BackColor = Color.White;
            Padding = new Padding(0);
        }

        /// <summary>Replaces the plotted series and repaints.</summary>
        public void SetData(List<EpochResult> history)
        {
            _history = history;
            Invalidate();
        }

        /// <summary>Clears the chart (e.g. before a new run starts).</summary>
        public void Clear()
        {
            _history = null;
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

            if (_history == null || _history.Count == 0)
            {
                ChartPainter.DrawEmpty(g, ClientRectangle,
                    "No training run yet.\nPress \"Train\" to start.", font);
                return;
            }

            // ---- ranges ------------------------------------------------------
            double xmin = _history[0].Epoch;
            double xmax = _history[_history.Count - 1].Epoch;
            if (xmax <= xmin) xmax = xmin + 1;

            double ymin = double.PositiveInfinity;
            double ymax = double.NegativeInfinity;
            for (int i = 0; i < _history.Count; i++)
            {
                double a = _history[i].TrainMse;
                double b = _history[i].TestMse;
                if (a < ymin) ymin = a;
                if (b < ymin) ymin = b;
                if (a > ymax) ymax = a;
                if (b > ymax) ymax = b;
            }
            if (double.IsInfinity(ymin)) { ymin = 0; ymax = 1; }

            // Pad the vertical range so the curves are not glued to the border.
            double pad = (ymax - ymin) * 0.10;
            if (pad <= 0) pad = System.Math.Abs(ymax) * 0.05 + 1e-9;
            ymin -= pad;
            ymax += pad;
            if (ymin < 0) ymin = 0;

            // ---- axes / grid -------------------------------------------------
            ChartPainter.DrawFrame(g, plot, xmin, xmax, ymin, ymax, 6, 5,
                                   "Epoch", "Mean Squared Error (MSE)", font);

            // ---- series ------------------------------------------------------
            using (var trainPen = new Pen(TrainColor, 1.8f))
            using (var testPen = new Pen(TestColor, 1.8f))
            {
                DrawSeries(g, plot, xmin, xmax, ymin, ymax, _history, true, trainPen);
                DrawSeries(g, plot, xmin, xmax, ymin, ymax, _history, false, testPen);
            }

            ChartPainter.DrawLegend(g, plot, font, "Train MSE", TrainColor, "Test MSE", TestColor);
        }

        private static void DrawSeries(Graphics g, RectangleF plot,
                                       double xmin, double xmax, double ymin, double ymax,
                                       List<EpochResult> history, bool training, Pen pen)
        {
            if (history.Count < 1) return;

            var points = new PointF[history.Count];
            for (int i = 0; i < history.Count; i++)
            {
                EpochResult ep = history[i];
                double value = training ? ep.TrainMse : ep.TestMse;
                points[i] = new PointF(
                    ChartPainter.ToPixelX(ep.Epoch, xmin, xmax, plot),
                    ChartPainter.ToPixelY(value, ymin, ymax, plot));
            }

            if (points.Length == 1)
            {
                g.FillEllipse(Brushes.White, points[0].X - 2, points[0].Y - 2, 4, 4);
                g.DrawEllipse(pen, points[0].X - 2, points[0].Y - 2, 4, 4);
                return;
            }

            g.DrawLines(pen, points);
        }
    }
}
