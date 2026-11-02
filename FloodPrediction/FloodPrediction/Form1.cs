using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Backprop;
using FloodPrediction.Data;
using FloodPrediction.Training;

namespace FloodPrediction
{
    /// <summary>
    /// Main window. Loads the CSV dataset, lets the user choose the hyper-parameters,
    /// runs training on a background thread so the UI stays responsive, and streams the
    /// loss curve, metrics and log entries to the screen as they happen.
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>Upper bound on retained log lines before the buffer is trimmed.</summary>
        private const int MaxLogLines = 20000;

        /// <summary>Number format for the exported grid-search CSV (10 decimals: the test MSEs
        /// are around 1e-6, so 6 decimals would flatten most of the table to 0.000001).</summary>
        private const string CsvNumber = "0.0000000000";

        /// <summary>Number format for R^2 in the same CSV.</summary>
        private const string CsvR2 = "0.00000000";

        private FloodDataset _dataset;
        private TrainingResult _result;
        private TrainingConfig _lastConfig;
        private CancellationTokenSource _cts;
        private bool _busy;

        /// <summary>Full text of everything shown in the log window (for CSV export).</summary>
        private readonly List<string> _logLines = new List<string>();

        public Form1()
        {
            InitializeComponent();

            gridSamples.Columns.Add("Actual", "Actual");
            gridSamples.Columns.Add("Predicted", "Predicted");
            gridSamples.Columns.Add("Error", "Error");
            gridSamples.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridSamples.DefaultCellStyle.Format = "0.00000";

            lossChart.Clear();
            scatterChart.Clear();
        }

        // ================================================================== #
        //  Start-up / dataset loading
        // ================================================================== #

        private void Form1_Load(object sender, EventArgs e)
        {
            Log("Flood prediction regression - 3-layer MLP with back-propagation.");
            Log("Baseline to beat (predict-the-mean test MSE) is shown once data is loaded.");

            string path = FindDefaultDataset();
            if (path != null)
            {
                LoadDataset(path);
            }
            else
            {
                txtDatasetPath.Text = "";
                lblDatasetInfo.Text = "No dataset found - press Browse...";
                lblStatus.Text = "Ready. Choose a CSV dataset to continue.";
                Log("Default dataset not found automatically. Use Browse... to select flood.csv.");
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "CSV datasets (*.csv)|*.csv|All files (*.*)|*.*";
                dialog.Title = "Select the flood prediction dataset";

                string initial = FindDefaultDataset();
                if (initial != null)
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(initial);
                    dialog.FileName = Path.GetFileName(initial);
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    LoadDataset(dialog.FileName);
                }
            }
        }

        /// <summary>
        /// Reads the CSV, normalises it and shows the resulting data profile.
        /// Any parsing problems are reported instead of crashing the application.
        /// </summary>
        private void LoadDataset(string path)
        {
            try
            {
                string warning;
                FloodDataset dataset = FloodDataset.Load(path, out warning);

                _dataset = dataset;
                _result = null;
                txtDatasetPath.Text = path;
                txtDatasetPath.SelectionStart = 0;
                txtDatasetPath.SelectionLength = 0;
                txtDatasetPath.ScrollToCaret();

                // Profile of the target column, used for the naive baseline.
                double min = double.PositiveInfinity, max = double.NegativeInfinity, mean = 0;
                foreach (double t in dataset.Targets)
                {
                    if (t < min) min = t;
                    if (t > max) max = t;
                    mean += t;
                }
                mean /= dataset.RowCount;

                lblDatasetInfo.Text = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0:N0} rows x {1} features  |  target {2}: min {3:0.###}, max {4:0.###}, mean {5:0.####}",
                    dataset.RowCount, dataset.FeatureCount, dataset.TargetName, min, max, mean);

                lblStatus.Text = "Dataset loaded.";
                UpdateArchitectureLabel();

                lossChart.Clear();
                scatterChart.Clear();
                gridSamples.Rows.Clear();
                ResetMetricLabels();

                Log("Loaded " + Path.GetFileName(path) + ": " + dataset.RowCount.ToString("N0") +
                    " rows, " + dataset.FeatureCount + " features, target = " + dataset.TargetName + ".");
                Log("Preprocessing: features Min-Max scaled to [0,1] with the statistics fitted " +
                    "on the training split only (no test-set leakage). Target left as-is because " +
                    "it is already a probability in [0,1].");
                if (warning != null) Log("Warning: " + warning);

                int[] trainIndex, testIndex;
                dataset.Split((double)numSplit.Value / 100.0, (int)numSeed.Value,
                              out trainIndex, out testIndex);

                Log(string.Format(CultureInfo.InvariantCulture,
                    "Split: {0:N0} train / {1:N0} test ({2}% held out), seed {3}.",
                    trainIndex.Length, testIndex.Length, 100 - numSplit.Value, numSeed.Value));
                Log(string.Format(CultureInfo.InvariantCulture,
                    "Naive baseline test MSE (always predict the training mean) = {0:0.000000}.",
                    Metrics.BaselineMeanMse(Pick(dataset.Targets, trainIndex),
                                            Pick(dataset.Targets, testIndex))));
            }
            catch (Exception ex)
            {
                _dataset = null;
                lblDatasetInfo.Text = "Failed to load dataset.";
                MessageBox.Show(this, ex.Message, "Dataset error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log("ERROR loading dataset: " + ex.Message);
            }
        }

        // ================================================================== #
        //  Training
        // ================================================================== #

        private async void btnTrain_Click(object sender, EventArgs e)
        {
            if (_busy) return;
            if (_dataset == null)
            {
                MessageBox.Show(this, "Load a dataset first.", "No data",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            TrainingConfig config;
            string error;
            if (!TryReadConfig(out config, out error))
            {
                MessageBox.Show(this, error, "Invalid hyper-parameters",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _lastConfig = config;
            BeginRun("Training...");

            Log(string.Format(CultureInfo.InvariantCulture,
                "Run: lr(in={0:0.###}, out={1:0.###}), hidden neurons={2}, epochs={3}, " +
                "log every {4}, split {5}%, seed={6}.",
                config.InputLearningRate, config.OutputLearningRate, config.HiddenNeurons,
                config.Epochs, config.LogEvery, (int)numSplit.Value, config.Seed));

            // The list the chart reads from is only ever touched on the UI thread.
            var liveHistory = new List<EpochResult>();
            var epochProgress = new Progress<EpochResult>(delegate(EpochResult ep)
            {
                liveHistory.Add(ep);
                lossChart.SetData(liveHistory);

                progress.Value = Math.Min(100, (int)(100.0 * ep.Epoch / Math.Max(1, config.Epochs)));
                lblStatus.Text = string.Format(CultureInfo.InvariantCulture,
                    "Training - epoch {0}/{1}  |  train MSE {2:0.000000}  |  test MSE {3:0.000000}",
                    ep.Epoch, config.Epochs, ep.TrainMse, ep.TestMse);

                Log(string.Format(CultureInfo.InvariantCulture,
                    "epoch {0,5} | train MSE {1:0.000000} | test MSE {2:0.000000} | {3:0.0}s",
                    ep.Epoch, ep.TrainMse, ep.TestMse, ep.ElapsedSeconds));
            });

            _cts = new CancellationTokenSource();
            var engine = new TrainingEngine();

            try
            {
                TrainingResult result = await Task.Run(
                    () => engine.Run(_dataset, config, epochProgress, _cts.Token));

                if (result.Cancelled)
                {
                    Log("Training stopped by user after " + result.History.Count + " epoch(s).");
                    lblStatus.Text = "Stopped.";
                }

                ShowResult(result, "Training complete");
            }
            catch (Exception ex)
            {
                Log("ERROR during training: " + ex.Message);
                MessageBox.Show(this, ex.Message, "Training error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Training failed.";
            }
            finally
            {
                EndRun();
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (_cts != null)
            {
                _cts.Cancel();
                btnStop.Enabled = false;
                lblStatus.Text = "Stopping...";
                Log("Stop requested - finishing the current epoch...");
            }
        }

        // ================================================================== #
        //  Hyper-parameter grid search
        // ================================================================== #

        private async void btnAutoTune_Click(object sender, EventArgs e)
        {
            if (_busy) return;
            if (_dataset == null)
            {
                MessageBox.Show(this, "Load a dataset first.", "No data",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            double[] learningRates = ParseDoubleList(txtTuneLr.Text);
            int[] hiddenSizes = ParseIntList(txtTuneHidden.Text);

            if (learningRates.Length == 0 || hiddenSizes.Length == 0)
            {
                MessageBox.Show(this,
                    "Enter comma-separated values for the grid, e.g.\n" +
                    "  Learning rates: 0.02,0.05,0.1\n" +
                    "  Hidden sizes:   8,16,32",
                    "Invalid grid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var settings = new TuningSettings();
            settings.LearningRates = learningRates;
            settings.HiddenSizes = hiddenSizes;
            settings.Epochs = (int)numEpochs.Value;
            settings.TrainFraction = (double)numSplit.Value / 100.0;
            settings.Seed = (int)numSeed.Value;
            settings.LogEvery = (int)numLogEvery.Value;

            int epochBudget = settings.Epochs;
            var checkpoints = new List<int>();
            foreach (int c in new int[] { Math.Max(1, epochBudget / 4), Math.Max(1, epochBudget / 2),
                                          Math.Max(1, (3 * epochBudget) / 4), epochBudget })
            {
                if (!checkpoints.Contains(c)) checkpoints.Add(c);
            }
            checkpoints.Sort();
            settings.Checkpoints = checkpoints.ToArray();

            int total = learningRates.Length * hiddenSizes.Length;
            BeginRun("Grid search...");

            var cellHistory = new List<EpochResult>();
            var currentCell = "";
            var bestSoFar = new List<TuningOutcome>();

            var tuningProgress = new Progress<TuningProgress>(delegate(TuningProgress p)
            {
                // Start a fresh loss curve for every grid cell.
                string cellKey = p.LearningRate + "/" + p.HiddenSize;
                if (p.Epoch != null && cellKey != currentCell)
                {
                    currentCell = cellKey;
                    cellHistory = new List<EpochResult>();
                    lossChart.SetData(cellHistory);
                }

                if (p.Epoch != null)
                {
                    cellHistory.Add(p.Epoch);
                    lossChart.SetData(cellHistory);
                    progress.Value = Math.Min(100, (int)(100.0 * p.Epoch.Epoch / Math.Max(1, settings.Epochs)));
                    lblStatus.Text = string.Format(CultureInfo.InvariantCulture,
                        "Grid {0}/{1}  |  lr={2:0.###} hidden={3}  |  epoch {4}/{5}  |  test MSE {6:0.000000}",
                        p.ConfigIndex, p.ConfigCount, p.LearningRate, p.HiddenSize,
                        p.Epoch.Epoch, settings.Epochs, p.Epoch.TestMse);
                }

                if (p.Message != null)
                {
                    Log(p.Message);
                }

                if (p.Outcome != null)
                {
                    bestSoFar.Add(p.Outcome);
                }
            });

            _cts = new CancellationTokenSource();
            var tuner = new HyperParameterTuner();

            try
            {
                List<TuningOutcome> outcomes = await Task.Run(
                    () => tuner.Run(_dataset, settings, tuningProgress, _cts.Token));

                if (outcomes.Count == 0)
                {
                    Log("Grid search stopped before any configuration finished.");
                    lblStatus.Text = "Stopped.";
                }
                else
                {
                    ReportTuningResults(outcomes, settings);
                }
            }
            catch (Exception ex)
            {
                Log("ERROR during grid search: " + ex.Message);
                MessageBox.Show(this, ex.Message, "Grid search error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                EndRun();
            }
        }

        /// <summary>Writes the ranked grid-search table to the log and to a CSV file.</summary>
        private void ReportTuningResults(List<TuningOutcome> outcomes, TuningSettings settings)
        {
            Log("----------------------------------------------------------------");
            Log("GRID SEARCH RESULTS (ranked by test MSE, " + outcomes.Count + " configurations)");
            Log("lr        hidden  " +
                string.Join("  ", Array.ConvertAll(settings.Checkpoints,
                    delegate(int c) { return "@" + c + "ep        "; })));

            string csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "hyperparameter_results_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");

            try
            {
                using (var writer = new StreamWriter(csvPath, false,
                    new System.Text.UTF8Encoding(false)))
                {
                    writer.WriteLine("Rank,LearningRate,HiddenNodes,Epochs," +
                        string.Join(",", Array.ConvertAll(settings.Checkpoints,
                            delegate(int c) { return "TestMSE@" + c + "ep"; })) +
                        ",FinalTestMSE,FinalTrainMSE,RMSE,MAE,R2,BaselineTestMSE");

                    for (int i = 0; i < outcomes.Count; i++)
                    {
                        TuningOutcome o = outcomes[i];
                        string cells = string.Join("  ", Array.ConvertAll(o.TestMseAtCheckpoint,
                            delegate(double v) { return double.IsNaN(v) ? "n/a      " : v.ToString("0.000000"); }));

                        Log(string.Format(CultureInfo.InvariantCulture,
                            "{0,-9} {1,-7}  {2}",
                            o.LearningRate.ToString("0.###"), o.HiddenSize, cells));

                        // Build the row as a list of strings so the number of columns can never
                        // drift out of sync with the header (a hand-written "{0}..{11}" format
                        // string had one placeholder too many and threw on the first row).
                        var row = new List<string>();
                        row.Add((i + 1).ToString(CultureInfo.InvariantCulture));
                        row.Add(o.LearningRate.ToString("0.###", CultureInfo.InvariantCulture));
                        row.Add(o.HiddenSize.ToString(CultureInfo.InvariantCulture));
                        row.Add(settings.Epochs.ToString(CultureInfo.InvariantCulture));
                        foreach (double v in o.TestMseAtCheckpoint)
                        {
                            row.Add(double.IsNaN(v) ? "" : v.ToString(CsvNumber, CultureInfo.InvariantCulture));
                        }
                        row.Add(o.Result.FinalTestMse.ToString(CsvNumber, CultureInfo.InvariantCulture));
                        row.Add(o.Result.FinalTrainMse.ToString(CsvNumber, CultureInfo.InvariantCulture));
                        row.Add(o.Result.TestRmse.ToString(CsvNumber, CultureInfo.InvariantCulture));
                        row.Add(o.Result.TestMae.ToString(CsvNumber, CultureInfo.InvariantCulture));
                        row.Add(o.Result.TestR2.ToString(CsvR2, CultureInfo.InvariantCulture));
                        row.Add(o.Result.BaselineTestMse.ToString(CsvNumber, CultureInfo.InvariantCulture));
                        writer.WriteLine(string.Join(",", row));
                    }
                }
                Log("Grid search table exported to: " + csvPath);
            }
            catch (Exception ex)
            {
                Log("Could not export grid search CSV: " + ex.Message);
            }

            TuningOutcome best = outcomes[0];
            Log("BEST CONFIGURATION: learning rate " + best.LearningRate.ToString("0.###") +
                ", hidden nodes " + best.HiddenSize +
                ", " + settings.Epochs + " epochs -> test MSE " +
                best.Score.ToString("0.000000") + " (R^2 " + best.Result.TestR2.ToString("0.0000") + ")");
            Log("The best values have been copied into the hyper-parameter boxes - press Train to confirm.");
            Log("----------------------------------------------------------------");

            // Apply the winner so the user can re-run it directly.
            txtInLR.Text = best.LearningRate.ToString("0.###", CultureInfo.InvariantCulture);
            txtOutLR.Text = best.LearningRate.ToString("0.###", CultureInfo.InvariantCulture);
            numHidden.Value = Math.Max(numHidden.Minimum, Math.Min(numHidden.Maximum, best.HiddenSize));

            // Surface the winner's numbers: metric tiles, scatter plot, sample table and a
            // full summary block in the log (the grid table itself only reports MSE and R^2).
            if (best.Result != null)
            {
                ShowResult(best.Result, string.Format(CultureInfo.InvariantCulture,
                    "Grid winner (lr {0:0.###}, hidden {1})", best.LearningRate, best.HiddenSize));
            }

            lblStatus.Text = "Grid search complete - best test MSE " + best.Score.ToString("0.000000");
        }

        // ================================================================== #
        //  Presenting results
        // ================================================================== #

        /// <summary>Fills the metric tiles, scatter plot and sample-prediction table.</summary>
        private void ShowResult(TrainingResult result, string headline)
        {
            _result = result;

            lblTrainValue.Text = result.FinalTrainMse.ToString("0.000000");
            lblTestValue.Text = result.FinalTestMse.ToString("0.000000");
            lblRmseValue.Text = result.TestRmse.ToString("0.000000");
            lblMaeValue.Text = result.TestMae.ToString("0.000000");
            lblR2Value.Text = result.TestR2.ToString("0.0000");
            lblBaseValue.Text = result.BaselineTestMse.ToString("0.000000");

            // Green = better than the naive baseline, red = worse.
            bool beatsBaseline = result.FinalTestMse < result.BaselineTestMse;
            lblTestValue.ForeColor = beatsBaseline ? Color.ForestGreen : Color.Firebrick;
            lblR2Value.ForeColor = beatsBaseline ? Color.ForestGreen : Color.Firebrick;

            if (result.History.Count > 0)
            {
                lossChart.SetData(result.History);
            }
            scatterChart.SetData(result.TestActual, result.TestPredicted);
            FillSampleGrid(result);

            double improvement = result.BaselineTestMse <= 0
                ? 0
                : (1.0 - result.FinalTestMse / result.BaselineTestMse) * 100.0;

            Log("----------------------------------------------------------------");
            Log(headline + " in " + result.Elapsed.TotalSeconds.ToString("0.0") + "s" +
                (result.History.Count > 0 ? " (" + result.History.Count + " epochs)" : "") + ".");
            Log(string.Format(CultureInfo.InvariantCulture,
                "  train MSE      = {0:0.000000}", result.FinalTrainMse));
            Log(string.Format(CultureInfo.InvariantCulture,
                "  test  MSE      = {0:0.000000}   (baseline {1:0.000000})",
                result.FinalTestMse, result.BaselineTestMse));
            Log(string.Format(CultureInfo.InvariantCulture,
                "  improvement over baseline = {0:0.0}%", improvement));
            Log(string.Format(CultureInfo.InvariantCulture,
                "  RMSE = {0:0.000000} | MAE = {1:0.000000} | R^2 = {2:0.0000}",
                result.TestRmse, result.TestMae, result.TestR2));
            Log(string.Format(CultureInfo.InvariantCulture,
                "  predictions within +/-0.05 of the true value: {0:0.0}%",
                result.WithinTolerance005 * 100.0));
            if (result.BestEpoch > 0)
            {
                Log(string.Format(CultureInfo.InvariantCulture,
                    "  best test MSE {0:0.000000} occurred at epoch {1}",
                    result.BestTestMse, result.BestEpoch));
            }
            Log("----------------------------------------------------------------");

            lblStatus.Text = headline + " - test MSE " + result.FinalTestMse.ToString("0.000000");
        }

        private void FillSampleGrid(TrainingResult result)
        {
            gridSamples.Rows.Clear();
            if (result.TestActual == null || result.TestActual.Length == 0) return;

            int count = result.TestActual.Length;
            int rows = Math.Min(15, count);
            int step = Math.Max(1, count / rows);

            for (int i = 0; i < count && gridSamples.Rows.Count < rows; i += step)
            {
                double actual = result.TestActual[i];
                double predicted = result.TestPredicted[i];
                gridSamples.Rows.Add(actual, predicted, predicted - actual);
            }
        }

        private void ResetMetricLabels()
        {
            foreach (Control c in grpMetrics.Controls)
            {
                if (c is Label && ((Label)c).Font.Bold)
                {
                    c.Text = "--";
                    c.ForeColor = Color.Black;
                }
            }
            _result = null;
        }

        private void UpdateArchitectureLabel()
        {
            if (_dataset == null)
            {
                lblArchitecture.Text = "Architecture: (load a dataset)";
                return;
            }

            int trainCount = (int)Math.Round(_dataset.RowCount * ((double)numSplit.Value / 100.0));
            lblArchitecture.Text = string.Format(
                "Architecture: {0} inputs -> {1} hidden -> 1 output\n" +
                "{2:N0} train / {3:N0} test rows",
                _dataset.FeatureCount, numHidden.Value, trainCount, _dataset.RowCount - trainCount);
        }

        private void ConfigValueChanged(object sender, EventArgs e)
        {
            UpdateArchitectureLabel();
        }

        // ================================================================== #
        //  Weights persistence
        // ================================================================== #

        private void btnSaveWeights_Click(object sender, EventArgs e)
        {
            if (_result == null || _result.Network == null)
            {
                MessageBox.Show(this, "Train a model first.", "Nothing to save",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Weight files (*.weights)|*.weights|All files (*.*)|*.*";
                dialog.FileName = "floodmodel.weights";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    _result.Network.saveWeights(dialog.FileName);

                    // Sidecar records the architecture needed to reload the file.
                    File.WriteAllText(Path.ChangeExtension(dialog.FileName, ".cfg"),
                        string.Format(CultureInfo.InvariantCulture,
                            "input={0}\nhidden={1}\noutput={2}\nlrIn={3}\nlrOut={4}\nseed={5}\ndataset={6}\n",
                            _result.Network.InputCount,
                            _result.Network.HiddenCount,
                            _result.Network.OutputCount,
                            _result.Network.InputLearningRate,
                            _result.Network.OutputLearningRate,
                            _result.Config != null ? _result.Config.Seed : 0,
                            _dataset != null ? _dataset.SourcePath : ""));

                    Log("Weights saved to " + dialog.FileName +
                        " (architecture sidecar: " + Path.GetFileName(Path.ChangeExtension(dialog.FileName, ".cfg")) + ").");
                    lblStatus.Text = "Weights saved.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Save failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnLoadWeights_Click(object sender, EventArgs e)
        {
            if (_dataset == null)
            {
                MessageBox.Show(this, "Load a dataset first so the model can be evaluated.",
                    "No data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Weight files (*.weights)|*.weights|All files (*.*)|*.*";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    int hidden = (int)numHidden.Value;
                    double lrIn = 0.15, lrOut = 0.20;

                    string cfgPath = Path.ChangeExtension(dialog.FileName, ".cfg");
                    if (File.Exists(cfgPath))
                    {
                        foreach (string line in File.ReadAllLines(cfgPath))
                        {
                            string[] pair = line.Split('=');
                            if (pair.Length != 2) continue;
                            string key = pair[0].Trim();
                            string value = pair[1].Trim();
                            if (key == "hidden") int.TryParse(value, out hidden);
                            if (key == "lrIn") double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out lrIn);
                            if (key == "lrOut") double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out lrOut);
                        }
                        Log("Loaded architecture from sidecar: hidden nodes = " + hidden + ".");
                    }
                    else
                    {
                        Log("No .cfg sidecar found - using hidden nodes = " + hidden +
                            " from the GUI (must match the saved model).");
                    }

                    NeuralNet.SeedRandom((int)numSeed.Value);
                    var net = new NeuralNet(_dataset.FeatureCount, hidden, 1);
                    net.InputLearningRate = lrIn;
                    net.OutputLearningRate = lrOut;
                    net.loadWeights(dialog.FileName);

                    txtInLR.Text = lrIn.ToString("0.###", CultureInfo.InvariantCulture);
                    txtOutLR.Text = lrOut.ToString("0.###", CultureInfo.InvariantCulture);
                    numHidden.Value = Math.Max(numHidden.Minimum, Math.Min(numHidden.Maximum, hidden));

                    var config = new TrainingConfig();
                    config.InputLearningRate = lrIn;
                    config.OutputLearningRate = lrOut;
                    config.HiddenNeurons = hidden;
                    config.TrainFraction = (double)numSplit.Value / 100.0;
                    config.Seed = (int)numSeed.Value;

                    TrainingResult result = TrainingEngine.Evaluate(_dataset, net, config);
                    ShowResult(result, "Loaded weights from " + Path.GetFileName(dialog.FileName));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this,
                        "Could not load weights.\n\n" + ex.Message +
                        "\n\nCheck that the hidden-node count matches the saved model.",
                        "Load failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Log("ERROR loading weights: " + ex.Message);
                }
            }
        }

        // ================================================================== #
        //  Exporting
        // ================================================================== #

        private void btnExportLog_Click(object sender, EventArgs e)
        {
            if (_logLines.Count == 0 && (_result == null || _result.History.Count == 0))
            {
                MessageBox.Show(this, "The log is empty.", "Nothing to export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.FileName = "training_log_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    // .txt = the human-readable console log; .csv = one row per epoch, i.e. the
                    // loss history (Epoch,TrainMSE,TestMSE,ElapsedSeconds) the activity asks for.
                    bool wantTextLog =
                        dialog.FilterIndex == 2 ||
                        Path.GetExtension(dialog.FileName).Equals(".txt", StringComparison.OrdinalIgnoreCase);
                    bool hasHistory = _result != null && _result.History != null && _result.History.Count > 0;

                    if (!wantTextLog && hasHistory)
                    {
                        WriteEpochCsv(dialog.FileName);
                        Log("Per-epoch training log exported to " + dialog.FileName +
                            " (" + _result.History.Count + " epochs, columns Epoch/TrainMSE/TestMSE/ElapsedSeconds).");
                    }
                    else
                    {
                        File.WriteAllLines(dialog.FileName, _logLines.ToArray(),
                            new System.Text.UTF8Encoding(false));
                        Log("Log exported to " + dialog.FileName);
                    }
                    lblStatus.Text = "Exported.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Export failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>Writes one row per training epoch: the loss curve as data.</summary>
        private void WriteEpochCsv(string path)
        {
            using (var writer = new StreamWriter(path, false, new System.Text.UTF8Encoding(false)))
            {
                writer.WriteLine("Epoch,TrainMSE,TestMSE,ElapsedSeconds");
                foreach (EpochResult step in _result.History)
                {
                    writer.WriteLine(string.Format(CultureInfo.InvariantCulture,
                        "{0},{1},{2},{3}",
                        step.Epoch,
                        step.TrainMse.ToString(CsvNumber, CultureInfo.InvariantCulture),
                        step.TestMse.ToString(CsvNumber, CultureInfo.InvariantCulture),
                        step.ElapsedSeconds.ToString("0.0", CultureInfo.InvariantCulture)));
                }
            }
        }

        private void btnExportPredictions_Click(object sender, EventArgs e)
        {
            if (_result == null || _result.TestActual == null)
            {
                MessageBox.Show(this, "Train or load a model first.", "Nothing to export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                dialog.FileName = "predictions_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    using (var writer = new StreamWriter(dialog.FileName, false,
                        new System.Text.UTF8Encoding(false)))
                    {
                        writer.WriteLine("RowIndex,Actual,Predicted,Error");
                        for (int i = 0; i < _result.TestActual.Length; i++)
                        {
                            writer.WriteLine(string.Format(CultureInfo.InvariantCulture,
                                "{0},{1:0.00000},{2:0.00000},{3:0.00000}",
                                i, _result.TestActual[i], _result.TestPredicted[i],
                                _result.TestPredicted[i] - _result.TestActual[i]));
                        }
                    }
                    Log("Predictions exported to " + dialog.FileName);
                    lblStatus.Text = "Predictions exported.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Export failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ================================================================== #
        //  Helpers
        // ================================================================== #

        private void BeginRun(string status)
        {
            _busy = true;
            _cts = new CancellationTokenSource();

            btnTrain.Enabled = false;
            btnAutoTune.Enabled = false;
            btnStop.Enabled = true;
            btnBrowse.Enabled = false;
            btnSaveWeights.Enabled = false;
            btnLoadWeights.Enabled = false;

            lossChart.Clear();
            gridSamples.Rows.Clear();
            ResetMetricLabels();

            progress.Value = 0;
            lblStatus.Text = status;
            Log("----------------------------------------------------------------");
        }

        private void EndRun()
        {
            _busy = false;
            btnTrain.Enabled = true;
            btnAutoTune.Enabled = true;
            btnStop.Enabled = false;
            btnBrowse.Enabled = true;
            btnSaveWeights.Enabled = true;
            btnLoadWeights.Enabled = true;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_cts != null) _cts.Cancel();
            base.OnFormClosing(e);
        }

        /// <summary>Reads and validates every hyper-parameter from the form.</summary>
        private bool TryReadConfig(out TrainingConfig config, out string error)
        {
            config = null;
            error = null;

            double lrIn, lrOut;
            if (!TryParseDouble(txtInLR.Text, out lrIn) || lrIn <= 0 || lrIn > 10)
            {
                error = "Input learning rate must be a positive number (e.g. 0.15).";
                return false;
            }
            if (!TryParseDouble(txtOutLR.Text, out lrOut) || lrOut <= 0 || lrOut > 10)
            {
                error = "Output learning rate must be a positive number (e.g. 0.2).";
                return false;
            }

            config = new TrainingConfig();
            config.InputLearningRate = lrIn;
            config.OutputLearningRate = lrOut;
            config.HiddenNeurons = (int)numHidden.Value;
            config.Epochs = (int)numEpochs.Value;
            config.LogEvery = (int)numLogEvery.Value;
            config.TrainFraction = (double)numSplit.Value / 100.0;
            config.Seed = (int)numSeed.Value;
            return true;
        }

        private static bool TryParseDouble(string text, out double value)
        {
            value = 0;
            if (string.IsNullOrEmpty(text)) return false;
            text = text.Trim();

            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return true;
            return double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
        }

        private static double[] ParseDoubleList(string text)
        {
            var values = new List<double>();
            foreach (string part in text.Split(new char[] { ',', ';', ' ' },
                StringSplitOptions.RemoveEmptyEntries))
            {
                double v;
                if (TryParseDouble(part, out v) && v > 0) values.Add(v);
            }
            return values.ToArray();
        }

        private static int[] ParseIntList(string text)
        {
            var values = new List<int>();
            foreach (string part in text.Split(new char[] { ',', ';', ' ' },
                StringSplitOptions.RemoveEmptyEntries))
            {
                int v;
                if (int.TryParse(part.Trim(), out v) && v > 0) values.Add(v);
            }
            return values.ToArray();
        }

        private static double[] Pick(double[] source, int start, int count)
        {
            var result = new double[count];
            Array.Copy(source, start, result, 0, count);
            return result;
        }

        private static double[] Pick(double[] source, int[] indices)
        {
            var result = new double[indices.Length];
            for (int i = 0; i < indices.Length; i++) result[i] = source[indices[i]];
            return result;
        }

        /// <summary>Appends a timestamped line to the log window.</summary>
        private void Log(string message)
        {
            string line = DateTime.Now.ToString("HH:mm:ss") + "  " + message;
            _logLines.Add(line);

            if (_logLines.Count > MaxLogLines)
            {
                _logLines.RemoveRange(0, _logLines.Count - (MaxLogLines / 2));
                txtLog.Text = string.Join(Environment.NewLine, _logLines.ToArray());
            }

            txtLog.AppendText(line + Environment.NewLine);
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }

        /// <summary>
        /// Walks up from the executable looking for flood.csv, searching a couple of
        /// levels down at each ancestor (the dataset lives in FloodPredictionDataSet\).
        /// </summary>
        private static string FindDefaultDataset()
        {
            string directory = AppDomain.CurrentDomain.BaseDirectory;

            for (int depth = 0; depth < 10 && !string.IsNullOrEmpty(directory); depth++)
            {
                string direct = Path.Combine(directory, "flood.csv");
                if (File.Exists(direct)) return direct;

                try
                {
                    foreach (string sub in SafeSubdirectories(directory))
                    {
                        string one = Path.Combine(sub, "flood.csv");
                        if (File.Exists(one)) return one;

                        foreach (string inner in SafeSubdirectories(sub))
                        {
                            string two = Path.Combine(inner, "flood.csv");
                            if (File.Exists(two)) return two;
                        }
                    }
                }
                catch
                {
                    // Ignore directories we are not allowed to read and keep walking up.
                }

                directory = Path.GetDirectoryName(directory);
            }

            return null;
        }

        private static string[] SafeSubdirectories(string path)
        {
            try
            {
                return Directory.GetDirectories(path);
            }
            catch
            {
                return new string[0];
            }
        }
    }
}
