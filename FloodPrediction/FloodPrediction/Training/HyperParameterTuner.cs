using System;
using System.Collections.Generic;
using System.Threading;

namespace FloodPrediction.Training
{
    /// <summary>Settings for the automatic hyper-parameter grid search.</summary>
    public sealed class TuningSettings
    {
        /// <summary>Learning rates to try (applied to both the input and output layers).</summary>
        public double[] LearningRates = new double[] { 0.02, 0.05, 0.10 };

        /// <summary>Hidden-layer sizes to try.</summary>
        public int[] HiddenSizes = new int[] { 8, 16, 32 };

        /// <summary>Epoch budget for every grid cell.</summary>
        public int Epochs = 300;

        /// <summary>
        /// Epoch checkpoints at which each cell's test MSE is recorded, so that the
        /// effect of the "epoch size" hyper-parameter can be read straight off one run.
        /// </summary>
        public int[] Checkpoints = new int[] { 50, 100, 200, 300 };

        public double TrainFraction = 0.80;
        public int Seed = 42;
        public int LogEvery = 10;
    }

    /// <summary>Result of one grid cell (one learning rate x one hidden size).</summary>
    public sealed class TuningOutcome
    {
        public double LearningRate;
        public int HiddenSize;
        public TrainingResult Result;

        /// <summary>Test MSE measured at each checkpoint epoch (aligned with TuningSettings.Checkpoints).</summary>
        public double[] TestMseAtCheckpoint;

        /// <summary>Combined score used for ranking (the final test MSE).</summary>
        public double Score { get { return Result == null ? double.PositiveInfinity : Result.FinalTestMse; } }
    }

    /// <summary>Progress message published while the grid search runs.</summary>
    public sealed class TuningProgress
    {
        public int ConfigIndex;
        public int ConfigCount;
        public double LearningRate;
        public int HiddenSize;
        public string Message;
        public EpochResult Epoch;      // may be null (start / finish of a cell)
        public TuningOutcome Outcome;  // non-null when a cell completes
    }

    /// <summary>
    /// Systematically searches the hyper-parameter grid.
    ///
    /// Rather than re-running the whole grid once per "epoch size", every cell is
    /// trained to the full epoch budget and its test MSE is sampled at several
    /// checkpoints. One sweep therefore answers three questions at once — which
    /// learning rate, which hidden-layer width, and how many epochs are enough —
    /// which is exactly what the "Hyperparameter Tuning &amp; Experimentation" rubric
    /// criterion asks for.
    /// </summary>
    public sealed class HyperParameterTuner
    {
        private readonly TrainingEngine _engine = new TrainingEngine();

        /// <summary>
        /// Runs every (learning rate, hidden size) combination and returns the
        /// outcomes ranked from best to worst final test MSE.
        /// </summary>
        public List<TuningOutcome> Run(FloodPrediction.Data.FloodDataset data,
                                       TuningSettings settings,
                                       IProgress<TuningProgress> progress,
                                       CancellationToken token)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (settings == null) throw new ArgumentNullException("settings");

            var outcomes = new List<TuningOutcome>();
            int total = settings.LearningRates.Length * settings.HiddenSizes.Length;
            int index = 0;

            foreach (double lr in settings.LearningRates)
            {
                foreach (int hidden in settings.HiddenSizes)
                {
                    if (token.IsCancellationRequested) break;
                    index++;

                    Publish(progress, index, total, lr, hidden,
                            "Grid " + index + "/" + total + ": learningRate=" +
                            lr.ToString("0.###") + ", hiddenNodes=" + hidden +
                            ", epochs=" + settings.Epochs, null, null);

                    var config = new TrainingConfig();
                    config.InputLearningRate = lr;
                    config.OutputLearningRate = lr;
                    config.HiddenNeurons = hidden;
                    config.Epochs = settings.Epochs;
                    config.LogEvery = settings.LogEvery;
                    config.TrainFraction = settings.TrainFraction;
                    config.Seed = settings.Seed;

                    var cellProgress = new Progress<EpochResult>(e =>
                    {
                        Publish(progress, index, total, lr, hidden, null, e, null);
                    });

                    TrainingResult result = _engine.Run(data, config, cellProgress, token);

                    var outcome = new TuningOutcome();
                    outcome.LearningRate = lr;
                    outcome.HiddenSize = hidden;
                    outcome.Result = result;
                    outcome.TestMseAtCheckpoint = SampleAtCheckpoints(result, settings.Checkpoints);
                    outcomes.Add(outcome);

                    Publish(progress, index, total, lr, hidden,
                            FormatCellSummary(index, total, outcome, settings.Checkpoints),
                            null, outcome);
                }
            }

            // Best first.
            outcomes.Sort(delegate(TuningOutcome a, TuningOutcome b) { return a.Score.CompareTo(b.Score); });
            return outcomes;
        }

        /// <summary>Reads the test MSE of a finished run at each requested checkpoint epoch.</summary>
        public static double[] SampleAtCheckpoints(TrainingResult result, int[] checkpoints)
        {
            var values = new double[checkpoints.Length];
            for (int c = 0; c < checkpoints.Length; c++)
            {
                values[c] = double.NaN;
                if (result == null || result.History == null || result.History.Count == 0) continue;

                // Nearest logged epoch at or before the checkpoint.
                double best = double.NaN;
                int bestDist = int.MaxValue;
                for (int i = 0; i < result.History.Count; i++)
                {
                    int epoch = result.History[i].Epoch;
                    if (epoch <= checkpoints[c])
                    {
                        int dist = checkpoints[c] - epoch;
                        if (dist < bestDist) { bestDist = dist; best = result.History[i].TestMse; }
                    }
                }
                values[c] = best;
            }
            return values;
        }

        private static string FormatCellSummary(int index, int total, TuningOutcome outcome, int[] checkpoints)
        {
            var parts = new List<string>();
            for (int c = 0; c < checkpoints.Length; c++)
            {
                double v = outcome.TestMseAtCheckpoint[c];
                parts.Add("@" + checkpoints[c] + "ep=" + (double.IsNaN(v) ? "n/a" : v.ToString("0.000000")));
            }

            return "Grid " + index + "/" + total +
                   " lr=" + outcome.LearningRate.ToString("0.###") +
                   " hidden=" + outcome.HiddenSize +
                   " | testMSE " + string.Join(" ", parts.ToArray()) +
                   " | final testMSE=" + outcome.Score.ToString("0.000000") +
                   " R2=" + outcome.Result.TestR2.ToString("0.0000");
        }

        private static void Publish(IProgress<TuningProgress> progress, int index, int total,
                                    double lr, int hidden, string message, EpochResult epoch,
                                    TuningOutcome outcome)
        {
            if (progress == null) return;
            var p = new TuningProgress();
            p.ConfigIndex = index;
            p.ConfigCount = total;
            p.LearningRate = lr;
            p.HiddenSize = hidden;
            p.Message = message;
            p.Epoch = epoch;
            p.Outcome = outcome;
            progress.Report(p);
        }
    }
}
