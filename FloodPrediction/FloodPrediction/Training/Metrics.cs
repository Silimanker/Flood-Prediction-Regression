using System;

namespace FloodPrediction.Training
{
    /// <summary>
    /// Regression evaluation metrics used to judge the model (rubric criterion 4:
    /// "rigorous evaluation metrics suited to the chosen problem, e.g. MSE for regression").
    /// </summary>
    public static class Metrics
    {
        /// <summary>Mean Squared Error — the primary loss for this activity.</summary>
        public static double MeanSquaredError(double[] actual, double[] predicted)
        {
            Validate(actual, predicted);
            double sum = 0.0;
            for (int i = 0; i < actual.Length; i++)
            {
                double d = actual[i] - predicted[i];
                sum += d * d;
            }
            return sum / actual.Length;
        }

        /// <summary>Root Mean Squared Error — same units as FloodProbability.</summary>
        public static double RootMeanSquaredError(double[] actual, double[] predicted)
        {
            return Math.Sqrt(MeanSquaredError(actual, predicted));
        }

        /// <summary>Mean Absolute Error — average absolute miss, robust to outliers.</summary>
        public static double MeanAbsoluteError(double[] actual, double[] predicted)
        {
            Validate(actual, predicted);
            double sum = 0.0;
            for (int i = 0; i < actual.Length; i++)
            {
                sum += Math.Abs(actual[i] - predicted[i]);
            }
            return sum / actual.Length;
        }

        /// <summary>
        /// Coefficient of determination R^2: 1.0 = perfect, 0.0 = no better than
        /// predicting the mean, negative = worse than the mean.
        /// </summary>
        public static double RSquared(double[] actual, double[] predicted)
        {
            Validate(actual, predicted);

            double mean = 0.0;
            for (int i = 0; i < actual.Length; i++) mean += actual[i];
            mean /= actual.Length;

            double ssRes = 0.0, ssTot = 0.0;
            for (int i = 0; i < actual.Length; i++)
            {
                double residual = actual[i] - predicted[i];
                double deviation = actual[i] - mean;
                ssRes += residual * residual;
                ssTot += deviation * deviation;
            }

            if (ssTot <= 0.0) return 0.0;   // constant target: no variance to explain
            return 1.0 - (ssRes / ssTot);
        }

        /// <summary>
        /// The "naive" baseline: MSE obtained by always predicting the mean of the
        /// training targets. The network only counts as useful if it beats this.
        /// </summary>
        public static double BaselineMeanMse(double[] trainTargets, double[] testTargets)
        {
            double mean = 0.0;
            for (int i = 0; i < trainTargets.Length; i++) mean += trainTargets[i];
            if (trainTargets.Length > 0) mean /= trainTargets.Length;

            double sum = 0.0;
            for (int i = 0; i < testTargets.Length; i++)
            {
                double d = testTargets[i] - mean;
                sum += d * d;
            }
            return testTargets.Length == 0 ? 0.0 : sum / testTargets.Length;
        }

        /// <summary>Share of predictions within +/- <paramref name="tolerance"/> of the truth.</summary>
        public static double WithinTolerance(double[] actual, double[] predicted, double tolerance)
        {
            Validate(actual, predicted);
            int hits = 0;
            for (int i = 0; i < actual.Length; i++)
            {
                if (Math.Abs(actual[i] - predicted[i]) <= tolerance) hits++;
            }
            return (double)hits / actual.Length;
        }

        private static void Validate(double[] actual, double[] predicted)
        {
            if (actual == null) throw new ArgumentNullException("actual");
            if (predicted == null) throw new ArgumentNullException("predicted");
            if (actual.Length != predicted.Length)
                throw new ArgumentException("actual and predicted must have the same length.");
            if (actual.Length == 0)
                throw new ArgumentException("Cannot evaluate an empty sample.");
        }
    }
}
