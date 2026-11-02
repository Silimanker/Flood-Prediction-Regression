using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Backprop;
using FloodPrediction.Data;

namespace FloodPrediction.Training
{
    /// <summary>All hyper-parameters of a single training run.</summary>
    public sealed class TrainingConfig
    {
        /// <summary>Learning rate for the input-to-hidden weights.</summary>
        public double InputLearningRate = 0.15;

        /// <summary>Learning rate for the hidden-to-output weights.</summary>
        public double OutputLearningRate = 0.20;

        /// <summary>Number of neurons in the single hidden layer.</summary>
        public int HiddenNeurons = 16;

        /// <summary>Number of passes over the training data.</summary>
        public int Epochs = 300;

        /// <summary>Report progress to the GUI every N epochs.</summary>
        public int LogEvery = 10;

        /// <summary>Fraction of rows used for training (0.8 = 80/20 split).</summary>
        public double TrainFraction = 0.80;

        /// <summary>Seed controlling weight init, the shuffle and the train/test split.</summary>
        public int Seed = 42;

        public TrainingConfig Clone()
        {
            return (TrainingConfig)MemberwiseClone();
        }
    }

    /// <summary>Per-epoch progress record published to the GUI / training log.</summary>
    public sealed class EpochResult
    {
        public int Epoch;
        public double TrainMse;
        public double TestMse;
        public double ElapsedSeconds;
    }

    /// <summary>Everything a finished run produced.</summary>
    public sealed class TrainingResult
    {
        /// <summary>The trained network (kept so weights can be saved / re-used).</summary>
        public NeuralNet Network;

        public TrainingConfig Config;

        /// <summary>One entry per epoch — the full loss curve.</summary>
        public List<EpochResult> History = new List<EpochResult>();

        public int[] TrainIndex;
        public int[] TestIndex;

        // Final metrics ------------------------------------------------------
        public double FinalTrainMse;
        public double FinalTestMse;
        public double TestRmse;
        public double TestMae;
        public double TestR2;
        public double BaselineTestMse;
        public double WithinTolerance005;

        /// <summary>Epoch with the lowest test MSE (useful when choosing "epoch size").</summary>
        public int BestEpoch;
        public double BestTestMse;

        public double[] TestActual;
        public double[] TestPredicted;

        public TimeSpan Elapsed;
        public bool Cancelled;
    }

    /// <summary>
    /// Drives a full training run: builds a seeded 3-layer MLP from the supplied
    /// "backneural" classes, shuffles the training rows, performs one online
    /// gradient-descent update per row per epoch, and measures MSE on both the
    /// training and held-out test split after every epoch.
    /// </summary>
    public sealed class TrainingEngine
    {
        /// <summary>
        /// Runs the training loop.
        /// </summary>
        /// <param name="data">Normalised dataset.</param>
        /// <param name="config">Hyper-parameters.</param>
        /// <param name="progress">
        /// Receives an <see cref="EpochResult"/> at the configured log interval (plus the
        /// first and last epoch). May be null.
        /// </param>
        /// <param name="token">Cancellation token used by the Stop button.</param>
        public TrainingResult Run(FloodDataset data, TrainingConfig config,
                                  IProgress<EpochResult> progress, CancellationToken token)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (config == null) throw new ArgumentNullException("config");

            var stopwatch = Stopwatch.StartNew();

            // --- deterministic weight initialisation --------------------------
            NeuralNet.SeedRandom(config.Seed);
            var net = new NeuralNet(data.FeatureCount, config.HiddenNeurons, 1);
            net.InputLearningRate = config.InputLearningRate;
            net.OutputLearningRate = config.OutputLearningRate;

            // --- reproducible train/test split --------------------------------
            int[] trainIndex, testIndex;
            data.Split(config.TrainFraction, config.Seed, out trainIndex, out testIndex);

            // Fit Min-Max statistics on the training rows only (no test-set leakage).
            data.FitScalerOn(trainIndex);

            var result = new TrainingResult();
            result.Network = net;
            result.Config = config.Clone();
            result.TrainIndex = trainIndex;
            result.TestIndex = testIndex;

            double[] trainTargets = Pick(data.Targets, trainIndex);
            double[] testTargets = Pick(data.Targets, testIndex);
            result.BaselineTestMse = Metrics.BaselineMeanMse(trainTargets, testTargets);

            double[][] features = data.Features;
            double[] targets = data.Targets;
            int featureCount = data.FeatureCount;

            int[] order = (int[])trainIndex.Clone();
            var shuffleRng = new Random(config.Seed ^ 0x5f3759df);
            bool cancelled = false;

            // --- epoch loop ---------------------------------------------------
            for (int epoch = 1; epoch <= config.Epochs; epoch++)
            {
                if (token.IsCancellationRequested) { cancelled = true; break; }

                // Shuffle every epoch so online SGD does not chase a fixed row order.
                for (int i = order.Length - 1; i > 0; i--)
                {
                    int j = shuffleRng.Next(i + 1);
                    int tmp = order[i]; order[i] = order[j]; order[j] = tmp;
                }

                // One forward+backward update per training row (stochastic gradient descent).
                for (int n = 0; n < order.Length; n++)
                {
                    int row = order[n];
                    double[] x = features[row];

                    for (int k = 0; k < featureCount; k++)
                    {
                        net.setInputs(k, x[k]);
                    }
                    net.setDesiredOutput(0, targets[row]);
                    net.learn();
                }

                // Measure (weights are untouched by run(), so this is a clean evaluation).
                double trainMse = EvaluateMse(data, trainIndex, net);
                double testMse = EvaluateMse(data, testIndex, net);

                var epochResult = new EpochResult();
                epochResult.Epoch = epoch;
                epochResult.TrainMse = trainMse;
                epochResult.TestMse = testMse;
                epochResult.ElapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                result.History.Add(epochResult);

                if (epoch == 1 || epoch == config.Epochs || epoch % config.LogEvery == 0)
                {
                    if (progress != null) progress.Report(epochResult);
                }
            }

            stopwatch.Stop();
            result.Elapsed = stopwatch.Elapsed;
            result.Cancelled = cancelled;

            // --- final evaluation --------------------------------------------
            if (result.History.Count > 0)
            {
                EpochResult last = result.History[result.History.Count - 1];
                result.FinalTrainMse = last.TrainMse;
                result.FinalTestMse = last.TestMse;

                result.BestEpoch = last.Epoch;
                result.BestTestMse = double.PositiveInfinity;
                for (int i = 0; i < result.History.Count; i++)
                {
                    if (result.History[i].TestMse < result.BestTestMse)
                    {
                        result.BestTestMse = result.History[i].TestMse;
                        result.BestEpoch = result.History[i].Epoch;
                    }
                }
            }

            Predict(data, testIndex, net, out result.TestActual, out result.TestPredicted);
            result.TestRmse = Metrics.RootMeanSquaredError(result.TestActual, result.TestPredicted);
            result.TestMae = Metrics.MeanAbsoluteError(result.TestActual, result.TestPredicted);
            result.TestR2 = Metrics.RSquared(result.TestActual, result.TestPredicted);
            result.WithinTolerance005 = Metrics.WithinTolerance(result.TestActual, result.TestPredicted, 0.05);

            return result;
        }

        /// <summary>
        /// Scores an <b>already trained</b> network (e.g. one restored from disk)
        /// without performing a single weight update. Uses the same seeded split as a
        /// training run with an identical <see cref="TrainingConfig"/>, so the numbers
        /// are directly comparable.
        /// </summary>
        public static TrainingResult Evaluate(FloodDataset data, NeuralNet net, TrainingConfig config)
        {
            int[] trainIndex, testIndex;
            data.Split(config.TrainFraction, config.Seed, out trainIndex, out testIndex);

            // Same scaling rule as training: statistics come from the training rows only.
            data.FitScalerOn(trainIndex);

            var result = new TrainingResult();
            result.Network = net;
            result.Config = config.Clone();
            result.TrainIndex = trainIndex;
            result.TestIndex = testIndex;

            result.FinalTrainMse = EvaluateMse(data, trainIndex, net);
            result.FinalTestMse = EvaluateMse(data, testIndex, net);
            result.BaselineTestMse = Metrics.BaselineMeanMse(
                Pick(data.Targets, trainIndex), Pick(data.Targets, testIndex));

            Predict(data, testIndex, net, out result.TestActual, out result.TestPredicted);
            result.TestRmse = Metrics.RootMeanSquaredError(result.TestActual, result.TestPredicted);
            result.TestMae = Metrics.MeanAbsoluteError(result.TestActual, result.TestPredicted);
            result.TestR2 = Metrics.RSquared(result.TestActual, result.TestPredicted);
            result.WithinTolerance005 = Metrics.WithinTolerance(result.TestActual, result.TestPredicted, 0.05);

            result.BestEpoch = 0;
            result.BestTestMse = result.FinalTestMse;
            return result;
        }

        /// <summary>Mean squared error of the current network over the given rows.</summary>
        public static double EvaluateMse(FloodDataset data, int[] indices, NeuralNet net)
        {
            double sum = 0.0;
            for (int n = 0; n < indices.Length; n++)
            {
                int i = indices[n];
                double[] x = data.Features[i];
                for (int k = 0; k < x.Length; k++)
                {
                    net.setInputs(k, x[k]);
                }
                net.run();

                double err = data.Targets[i] - net.getOuputData(0);
                sum += err * err;
            }
            return indices.Length == 0 ? 0.0 : sum / indices.Length;
        }

        /// <summary>Produces predictions for the given rows (forward pass only).</summary>
        public static void Predict(FloodDataset data, int[] indices, NeuralNet net,
                                   out double[] actual, out double[] predicted)
        {
            actual = new double[indices.Length];
            predicted = new double[indices.Length];

            for (int n = 0; n < indices.Length; n++)
            {
                int i = indices[n];
                double[] x = data.Features[i];
                for (int k = 0; k < x.Length; k++)
                {
                    net.setInputs(k, x[k]);
                }
                net.run();

                actual[n] = data.Targets[i];
                predicted[n] = net.getOuputData(0);
            }
        }

        private static double[] Pick(double[] source, int[] index)
        {
            var result = new double[index.Length];
            for (int i = 0; i < index.Length; i++) result[i] = source[index[i]];
            return result;
        }
    }
}
