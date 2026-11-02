using System;

namespace FloodPrediction.Data
{
    /// <summary>
    /// Per-column Min-Max scaler. Maps every feature linearly onto [0,1] using the
    /// statistics of the training data, which is exactly what the activity's
    /// "Data Preprocessing &amp; Pipeline" criterion asks for.
    /// </summary>
    /// <remarks>
    /// Min-Max (rather than Z-score) is used because the supplied network squashes
    /// everything through a sigmoid: [0,1] inputs keep the hidden pre-activations in
    /// the well-behaved region of the curve instead of driving it into saturation.
    /// </remarks>
    public sealed class MinMaxScaler
    {
        /// <summary>Observed minimum of each feature column.</summary>
        public double[] Minimum { get; private set; }

        /// <summary>Observed maximum of each feature column.</summary>
        public double[] Maximum { get; private set; }

        public MinMaxScaler(double[] minimum, double[] maximum)
        {
            if (minimum == null) throw new ArgumentNullException("minimum");
            if (maximum == null) throw new ArgumentNullException("maximum");
            if (minimum.Length != maximum.Length)
                throw new ArgumentException("minimum and maximum must have the same length.");

            Minimum = minimum;
            Maximum = maximum;
        }

        /// <summary>
        /// Scales one value from column <paramref name="column"/> into [0,1].
        /// A constant column (max == min) collapses to the midpoint 0.5 rather than
        /// dividing by zero.
        /// </summary>
        public double Scale(int column, double value)
        {
            double range = Maximum[column] - Minimum[column];
            if (range <= 0.0)
            {
                return 0.5;
            }

            double scaled = (value - Minimum[column]) / range;

            // Guard against values that fall outside the training range.
            if (scaled < 0.0) scaled = 0.0;
            if (scaled > 1.0) scaled = 1.0;
            return scaled;
        }

        /// <summary>Inverse of <see cref="Scale"/> — maps back to the original units.</summary>
        public double Unscale(int column, double scaledValue)
        {
            double range = Maximum[column] - Minimum[column];
            return Minimum[column] + scaledValue * range;
        }
    }
}
