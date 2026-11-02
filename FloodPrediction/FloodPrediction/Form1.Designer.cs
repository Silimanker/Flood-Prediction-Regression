namespace FloodPrediction
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblDatasetInfo = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtDatasetPath = new System.Windows.Forms.TextBox();
            this.lblDataset = new System.Windows.Forms.Label();
            this.grpHyper = new System.Windows.Forms.GroupBox();
            this.lblWeightsHint = new System.Windows.Forms.Label();
            this.lblArchitecture = new System.Windows.Forms.Label();
            this.btnExportPred = new System.Windows.Forms.Button();
            this.btnExportLog = new System.Windows.Forms.Button();
            this.btnLoadWeights = new System.Windows.Forms.Button();
            this.btnSaveWeights = new System.Windows.Forms.Button();
            this.btnAutoTune = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnTrain = new System.Windows.Forms.Button();
            this.txtTuneHidden = new System.Windows.Forms.TextBox();
            this.lblTuneHidden = new System.Windows.Forms.Label();
            this.txtTuneLr = new System.Windows.Forms.TextBox();
            this.lblTuneLr = new System.Windows.Forms.Label();
            this.lblTuneSection = new System.Windows.Forms.Label();
            this.numSeed = new System.Windows.Forms.NumericUpDown();
            this.lblSeed = new System.Windows.Forms.Label();
            this.numSplit = new System.Windows.Forms.NumericUpDown();
            this.lblSplit = new System.Windows.Forms.Label();
            this.numLogEvery = new System.Windows.Forms.NumericUpDown();
            this.lblLogEvery = new System.Windows.Forms.Label();
            this.numEpochs = new System.Windows.Forms.NumericUpDown();
            this.lblEpochs = new System.Windows.Forms.Label();
            this.numHidden = new System.Windows.Forms.NumericUpDown();
            this.lblHidden = new System.Windows.Forms.Label();
            this.txtOutLR = new System.Windows.Forms.TextBox();
            this.lblOutLR = new System.Windows.Forms.Label();
            this.txtInLR = new System.Windows.Forms.TextBox();
            this.lblInLR = new System.Windows.Forms.Label();
            this.grpLoss = new System.Windows.Forms.GroupBox();
            this.lossChart = new FloodPrediction.Controls.LossCurveChart();
            this.grpScatter = new System.Windows.Forms.GroupBox();
            this.scatterChart = new FloodPrediction.Controls.PredictionScatterChart();
            this.grpMetrics = new System.Windows.Forms.GroupBox();
            this.lblBaseValue = new System.Windows.Forms.Label();
            this.lblBaseCaption = new System.Windows.Forms.Label();
            this.lblR2Value = new System.Windows.Forms.Label();
            this.lblR2Caption = new System.Windows.Forms.Label();
            this.lblMaeValue = new System.Windows.Forms.Label();
            this.lblMaeCaption = new System.Windows.Forms.Label();
            this.lblRmseValue = new System.Windows.Forms.Label();
            this.lblRmseCaption = new System.Windows.Forms.Label();
            this.lblTestValue = new System.Windows.Forms.Label();
            this.lblTestCaption = new System.Windows.Forms.Label();
            this.lblTrainValue = new System.Windows.Forms.Label();
            this.lblTrainCaption = new System.Windows.Forms.Label();
            this.grpLog = new System.Windows.Forms.GroupBox();
            this.txtLog = new System.Windows.Forms.RichTextBox();
            this.grpSamples = new System.Windows.Forms.GroupBox();
            this.gridSamples = new System.Windows.Forms.DataGridView();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.progress = new System.Windows.Forms.ToolStripProgressBar();
            this.panelTop.SuspendLayout();
            this.grpHyper.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSplit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLogEvery)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEpochs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHidden)).BeginInit();
            this.grpLoss.SuspendLayout();
            this.grpScatter.SuspendLayout();
            this.grpMetrics.SuspendLayout();
            this.grpLog.SuspendLayout();
            this.grpSamples.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridSamples)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.lblDatasetInfo);
            this.panelTop.Controls.Add(this.btnBrowse);
            this.panelTop.Controls.Add(this.txtDatasetPath);
            this.panelTop.Controls.Add(this.lblDataset);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1264, 44);
            this.panelTop.TabIndex = 0;
            // 
            // lblDataset
            // 
            this.lblDataset.AutoSize = true;
            this.lblDataset.Location = new System.Drawing.Point(8, 14);
            this.lblDataset.Name = "lblDataset";
            this.lblDataset.Size = new System.Drawing.Size(50, 15);
            this.lblDataset.TabIndex = 0;
            this.lblDataset.Text = "Dataset:";
            // 
            // txtDatasetPath
            // 
            this.txtDatasetPath.Location = new System.Drawing.Point(68, 11);
            this.txtDatasetPath.Name = "txtDatasetPath";
            this.txtDatasetPath.ReadOnly = true;
            this.txtDatasetPath.Size = new System.Drawing.Size(420, 23);
            this.txtDatasetPath.TabIndex = 1;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(494, 10);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(78, 25);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblDatasetInfo
            // 
            this.lblDatasetInfo.AutoSize = true;
            this.lblDatasetInfo.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.lblDatasetInfo.Location = new System.Drawing.Point(584, 14);
            this.lblDatasetInfo.Name = "lblDatasetInfo";
            this.lblDatasetInfo.Size = new System.Drawing.Size(112, 15);
            this.lblDatasetInfo.TabIndex = 3;
            this.lblDatasetInfo.Text = "No dataset loaded.";
            // 
            // grpHyper
            // 
            this.grpHyper.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.grpHyper.Controls.Add(this.lblWeightsHint);
            this.grpHyper.Controls.Add(this.lblArchitecture);
            this.grpHyper.Controls.Add(this.btnExportPred);
            this.grpHyper.Controls.Add(this.btnExportLog);
            this.grpHyper.Controls.Add(this.btnLoadWeights);
            this.grpHyper.Controls.Add(this.btnSaveWeights);
            this.grpHyper.Controls.Add(this.btnAutoTune);
            this.grpHyper.Controls.Add(this.btnStop);
            this.grpHyper.Controls.Add(this.btnTrain);
            this.grpHyper.Controls.Add(this.txtTuneHidden);
            this.grpHyper.Controls.Add(this.lblTuneHidden);
            this.grpHyper.Controls.Add(this.txtTuneLr);
            this.grpHyper.Controls.Add(this.lblTuneLr);
            this.grpHyper.Controls.Add(this.lblTuneSection);
            this.grpHyper.Controls.Add(this.numSeed);
            this.grpHyper.Controls.Add(this.lblSeed);
            this.grpHyper.Controls.Add(this.numSplit);
            this.grpHyper.Controls.Add(this.lblSplit);
            this.grpHyper.Controls.Add(this.numLogEvery);
            this.grpHyper.Controls.Add(this.lblLogEvery);
            this.grpHyper.Controls.Add(this.numEpochs);
            this.grpHyper.Controls.Add(this.lblEpochs);
            this.grpHyper.Controls.Add(this.numHidden);
            this.grpHyper.Controls.Add(this.lblHidden);
            this.grpHyper.Controls.Add(this.txtOutLR);
            this.grpHyper.Controls.Add(this.lblOutLR);
            this.grpHyper.Controls.Add(this.txtInLR);
            this.grpHyper.Controls.Add(this.lblInLR);
            this.grpHyper.Location = new System.Drawing.Point(8, 52);
            this.grpHyper.Name = "grpHyper";
            this.grpHyper.Size = new System.Drawing.Size(266, 700);
            this.grpHyper.TabIndex = 1;
            this.grpHyper.TabStop = false;
            this.grpHyper.Text = "Hyper-parameters";
            // 
            // lblInLR
            // 
            this.lblInLR.AutoSize = true;
            this.lblInLR.Location = new System.Drawing.Point(12, 25);
            this.lblInLR.Name = "lblInLR";
            this.lblInLR.Size = new System.Drawing.Size(102, 15);
            this.lblInLR.TabIndex = 0;
            this.lblInLR.Text = "Input learning rate";
            // 
            // txtInLR
            // 
            this.txtInLR.Location = new System.Drawing.Point(130, 22);
            this.txtInLR.Name = "txtInLR";
            this.txtInLR.Size = new System.Drawing.Size(120, 23);
            this.txtInLR.TabIndex = 1;
            this.txtInLR.Text = "0.15";
            // 
            // lblOutLR
            // 
            this.lblOutLR.AutoSize = true;
            this.lblOutLR.Location = new System.Drawing.Point(12, 53);
            this.lblOutLR.Name = "lblOutLR";
            this.lblOutLR.Size = new System.Drawing.Size(114, 15);
            this.lblOutLR.TabIndex = 2;
            this.lblOutLR.Text = "Output learning rate";
            // 
            // txtOutLR
            // 
            this.txtOutLR.Location = new System.Drawing.Point(130, 50);
            this.txtOutLR.Name = "txtOutLR";
            this.txtOutLR.Size = new System.Drawing.Size(120, 23);
            this.txtOutLR.TabIndex = 3;
            this.txtOutLR.Text = "0.2";
            // 
            // lblHidden
            // 
            this.lblHidden.AutoSize = true;
            this.lblHidden.Location = new System.Drawing.Point(12, 81);
            this.lblHidden.Name = "lblHidden";
            this.lblHidden.Size = new System.Drawing.Size(94, 15);
            this.lblHidden.TabIndex = 4;
            this.lblHidden.Text = "Hidden neurons";
            // 
            // numHidden
            // 
            this.numHidden.Location = new System.Drawing.Point(130, 78);
            this.numHidden.Maximum = new decimal(new int[] { 1024, 0, 0, 0 });
            this.numHidden.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numHidden.Name = "numHidden";
            this.numHidden.Size = new System.Drawing.Size(120, 23);
            this.numHidden.TabIndex = 5;
            this.numHidden.Value = new decimal(new int[] { 16, 0, 0, 0 });
            this.numHidden.ValueChanged += new System.EventHandler(this.ConfigValueChanged);
            // 
            // lblEpochs
            // 
            this.lblEpochs.AutoSize = true;
            this.lblEpochs.Location = new System.Drawing.Point(12, 109);
            this.lblEpochs.Name = "lblEpochs";
            this.lblEpochs.Size = new System.Drawing.Size(49, 15);
            this.lblEpochs.TabIndex = 6;
            this.lblEpochs.Text = "Epochs";
            // 
            // numEpochs
            // 
            this.numEpochs.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            this.numEpochs.Location = new System.Drawing.Point(130, 106);
            this.numEpochs.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            this.numEpochs.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numEpochs.Name = "numEpochs";
            this.numEpochs.Size = new System.Drawing.Size(120, 23);
            this.numEpochs.TabIndex = 7;
            this.numEpochs.Value = new decimal(new int[] { 300, 0, 0, 0 });
            this.numEpochs.ValueChanged += new System.EventHandler(this.ConfigValueChanged);
            // 
            // lblLogEvery
            // 
            this.lblLogEvery.AutoSize = true;
            this.lblLogEvery.Location = new System.Drawing.Point(12, 137);
            this.lblLogEvery.Name = "lblLogEvery";
            this.lblLogEvery.Size = new System.Drawing.Size(112, 15);
            this.lblLogEvery.TabIndex = 8;
            this.lblLogEvery.Text = "Log every N epochs";
            // 
            // numLogEvery
            // 
            this.numLogEvery.Location = new System.Drawing.Point(130, 134);
            this.numLogEvery.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            this.numLogEvery.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numLogEvery.Name = "numLogEvery";
            this.numLogEvery.Size = new System.Drawing.Size(120, 23);
            this.numLogEvery.TabIndex = 9;
            this.numLogEvery.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // lblSplit
            // 
            this.lblSplit.AutoSize = true;
            this.lblSplit.Location = new System.Drawing.Point(12, 165);
            this.lblSplit.Name = "lblSplit";
            this.lblSplit.Size = new System.Drawing.Size(90, 15);
            this.lblSplit.TabIndex = 10;
            this.lblSplit.Text = "Train split (%)";
            // 
            // numSplit
            // 
            this.numSplit.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            this.numSplit.Location = new System.Drawing.Point(130, 162);
            this.numSplit.Maximum = new decimal(new int[] { 95, 0, 0, 0 });
            this.numSplit.Minimum = new decimal(new int[] { 50, 0, 0, 0 });
            this.numSplit.Name = "numSplit";
            this.numSplit.Size = new System.Drawing.Size(120, 23);
            this.numSplit.TabIndex = 11;
            this.numSplit.Value = new decimal(new int[] { 80, 0, 0, 0 });
            // 
            // lblSeed
            // 
            this.lblSeed.AutoSize = true;
            this.lblSeed.Location = new System.Drawing.Point(12, 193);
            this.lblSeed.Name = "lblSeed";
            this.lblSeed.Size = new System.Drawing.Size(78, 15);
            this.lblSeed.TabIndex = 12;
            this.lblSeed.Text = "Random seed";
            // 
            // numSeed
            // 
            this.numSeed.Location = new System.Drawing.Point(130, 190);
            this.numSeed.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            this.numSeed.Name = "numSeed";
            this.numSeed.Size = new System.Drawing.Size(120, 23);
            this.numSeed.TabIndex = 13;
            this.numSeed.Value = new decimal(new int[] { 42, 0, 0, 0 });
            // 
            // lblTuneSection
            // 
            this.lblTuneSection.AutoSize = true;
            this.lblTuneSection.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTuneSection.ForeColor = System.Drawing.Color.FromArgb(30, 100, 200);
            this.lblTuneSection.Location = new System.Drawing.Point(12, 222);
            this.lblTuneSection.Name = "lblTuneSection";
            this.lblTuneSection.Size = new System.Drawing.Size(150, 15);
            this.lblTuneSection.TabIndex = 14;
            this.lblTuneSection.Text = "Auto-tune grid search";
            // 
            // lblTuneLr
            // 
            this.lblTuneLr.AutoSize = true;
            this.lblTuneLr.Location = new System.Drawing.Point(12, 248);
            this.lblTuneLr.Name = "lblTuneLr";
            this.lblTuneLr.Size = new System.Drawing.Size(98, 15);
            this.lblTuneLr.TabIndex = 15;
            this.lblTuneLr.Text = "Learning rates";
            // 
            // txtTuneLr
            // 
            this.txtTuneLr.Location = new System.Drawing.Point(130, 245);
            this.txtTuneLr.Name = "txtTuneLr";
            this.txtTuneLr.Size = new System.Drawing.Size(120, 23);
            this.txtTuneLr.TabIndex = 16;
            this.txtTuneLr.Text = "0.02,0.05,0.1";
            // 
            // lblTuneHidden
            // 
            this.lblTuneHidden.AutoSize = true;
            this.lblTuneHidden.Location = new System.Drawing.Point(12, 276);
            this.lblTuneHidden.Name = "lblTuneHidden";
            this.lblTuneHidden.Size = new System.Drawing.Size(84, 15);
            this.lblTuneHidden.TabIndex = 17;
            this.lblTuneHidden.Text = "Hidden sizes";
            // 
            // txtTuneHidden
            // 
            this.txtTuneHidden.Location = new System.Drawing.Point(130, 273);
            this.txtTuneHidden.Name = "txtTuneHidden";
            this.txtTuneHidden.Size = new System.Drawing.Size(120, 23);
            this.txtTuneHidden.TabIndex = 18;
            this.txtTuneHidden.Text = "8,16,32";
            // 
            // btnTrain
            // 
            this.btnTrain.BackColor = System.Drawing.Color.FromArgb(30, 100, 200);
            this.btnTrain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTrain.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnTrain.ForeColor = System.Drawing.Color.White;
            this.btnTrain.Location = new System.Drawing.Point(12, 306);
            this.btnTrain.Name = "btnTrain";
            this.btnTrain.Size = new System.Drawing.Size(238, 36);
            this.btnTrain.TabIndex = 19;
            this.btnTrain.Text = "Train";
            this.btnTrain.UseVisualStyleBackColor = false;
            this.btnTrain.Click += new System.EventHandler(this.btnTrain_Click);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(12, 348);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(238, 28);
            this.btnStop.TabIndex = 20;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnAutoTune
            // 
            this.btnAutoTune.Location = new System.Drawing.Point(12, 382);
            this.btnAutoTune.Name = "btnAutoTune";
            this.btnAutoTune.Size = new System.Drawing.Size(238, 28);
            this.btnAutoTune.TabIndex = 21;
            this.btnAutoTune.Text = "Auto-Tune Grid";
            this.btnAutoTune.UseVisualStyleBackColor = true;
            this.btnAutoTune.Click += new System.EventHandler(this.btnAutoTune_Click);
            // 
            // btnSaveWeights
            // 
            this.btnSaveWeights.Location = new System.Drawing.Point(12, 418);
            this.btnSaveWeights.Name = "btnSaveWeights";
            this.btnSaveWeights.Size = new System.Drawing.Size(114, 26);
            this.btnSaveWeights.TabIndex = 22;
            this.btnSaveWeights.Text = "Save Weights";
            this.btnSaveWeights.UseVisualStyleBackColor = true;
            this.btnSaveWeights.Click += new System.EventHandler(this.btnSaveWeights_Click);
            // 
            // btnLoadWeights
            // 
            this.btnLoadWeights.Location = new System.Drawing.Point(136, 418);
            this.btnLoadWeights.Name = "btnLoadWeights";
            this.btnLoadWeights.Size = new System.Drawing.Size(114, 26);
            this.btnLoadWeights.TabIndex = 23;
            this.btnLoadWeights.Text = "Load Weights";
            this.btnLoadWeights.UseVisualStyleBackColor = true;
            this.btnLoadWeights.Click += new System.EventHandler(this.btnLoadWeights_Click);
            // 
            // btnExportLog
            // 
            this.btnExportLog.Location = new System.Drawing.Point(12, 450);
            this.btnExportLog.Name = "btnExportLog";
            this.btnExportLog.Size = new System.Drawing.Size(238, 26);
            this.btnExportLog.TabIndex = 24;
            this.btnExportLog.Text = "Export Training Log (CSV)";
            this.btnExportLog.UseVisualStyleBackColor = true;
            this.btnExportLog.Click += new System.EventHandler(this.btnExportLog_Click);
            // 
            // btnExportPred
            // 
            this.btnExportPred.Location = new System.Drawing.Point(12, 482);
            this.btnExportPred.Name = "btnExportPred";
            this.btnExportPred.Size = new System.Drawing.Size(238, 26);
            this.btnExportPred.TabIndex = 25;
            this.btnExportPred.Text = "Export Predictions (CSV)";
            this.btnExportPred.UseVisualStyleBackColor = true;
            this.btnExportPred.Click += new System.EventHandler(this.btnExportPredictions_Click);
            // 
            // lblArchitecture
            // 
            this.lblArchitecture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblArchitecture.Location = new System.Drawing.Point(12, 518);
            this.lblArchitecture.Name = "lblArchitecture";
            this.lblArchitecture.Padding = new System.Windows.Forms.Padding(4);
            this.lblArchitecture.Size = new System.Drawing.Size(238, 44);
            this.lblArchitecture.TabIndex = 26;
            this.lblArchitecture.Text = "Architecture: (load a dataset)";
            // 
            // lblWeightsHint
            // 
            this.lblWeightsHint.ForeColor = System.Drawing.Color.FromArgb(110, 110, 110);
            this.lblWeightsHint.Location = new System.Drawing.Point(12, 572);
            this.lblWeightsHint.Name = "lblWeightsHint";
            this.lblWeightsHint.Size = new System.Drawing.Size(238, 92);
            this.lblWeightsHint.TabIndex = 27;
            this.lblWeightsHint.Text = "Weights are saved in the original backneural text format, plus a .cfg sidecar tha" +
    "t records the architecture needed to reload them.";
            // 
            // grpLoss
            // 
            this.grpLoss.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpLoss.Controls.Add(this.lossChart);
            this.grpLoss.Location = new System.Drawing.Point(282, 52);
            this.grpLoss.Name = "grpLoss";
            this.grpLoss.Size = new System.Drawing.Size(484, 316);
            this.grpLoss.TabIndex = 2;
            this.grpLoss.TabStop = false;
            this.grpLoss.Text = "Training loss (MSE per epoch)";
            // 
            // lossChart
            // 
            this.lossChart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lossChart.BackColor = System.Drawing.Color.White;
            this.lossChart.Location = new System.Drawing.Point(12, 20);
            this.lossChart.Name = "lossChart";
            this.lossChart.Size = new System.Drawing.Size(460, 286);
            this.lossChart.TabIndex = 0;
            // 
            // grpScatter
            // 
            this.grpScatter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.grpScatter.Controls.Add(this.scatterChart);
            this.grpScatter.Location = new System.Drawing.Point(774, 52);
            this.grpScatter.Name = "grpScatter";
            this.grpScatter.Size = new System.Drawing.Size(482, 316);
            this.grpScatter.TabIndex = 3;
            this.grpScatter.TabStop = false;
            this.grpScatter.Text = "Predicted vs actual (held-out test set)";
            // 
            // scatterChart
            // 
            this.scatterChart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scatterChart.BackColor = System.Drawing.Color.White;
            this.scatterChart.Location = new System.Drawing.Point(12, 20);
            this.scatterChart.Name = "scatterChart";
            this.scatterChart.Size = new System.Drawing.Size(458, 286);
            this.scatterChart.TabIndex = 0;
            // 
            // grpMetrics
            // 
            this.grpMetrics.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpMetrics.Controls.Add(this.lblBaseValue);
            this.grpMetrics.Controls.Add(this.lblBaseCaption);
            this.grpMetrics.Controls.Add(this.lblR2Value);
            this.grpMetrics.Controls.Add(this.lblR2Caption);
            this.grpMetrics.Controls.Add(this.lblMaeValue);
            this.grpMetrics.Controls.Add(this.lblMaeCaption);
            this.grpMetrics.Controls.Add(this.lblRmseValue);
            this.grpMetrics.Controls.Add(this.lblRmseCaption);
            this.grpMetrics.Controls.Add(this.lblTestValue);
            this.grpMetrics.Controls.Add(this.lblTestCaption);
            this.grpMetrics.Controls.Add(this.lblTrainValue);
            this.grpMetrics.Controls.Add(this.lblTrainCaption);
            this.grpMetrics.Location = new System.Drawing.Point(282, 376);
            this.grpMetrics.Name = "grpMetrics";
            this.grpMetrics.Size = new System.Drawing.Size(974, 74);
            this.grpMetrics.TabIndex = 4;
            this.grpMetrics.TabStop = false;
            this.grpMetrics.Text = "Evaluation (held-out test set)";
            // 
            // lblTrainCaption
            // 
            this.lblTrainCaption.AutoSize = true;
            this.lblTrainCaption.ForeColor = System.Drawing.Color.FromArgb(110, 110, 110);
            this.lblTrainCaption.Location = new System.Drawing.Point(12, 21);
            this.lblTrainCaption.Name = "lblTrainCaption";
            this.lblTrainCaption.Size = new System.Drawing.Size(84, 15);
            this.lblTrainCaption.TabIndex = 0;
            this.lblTrainCaption.Text = "Train MSE";
            // 
            // lblTrainValue
            // 
            this.lblTrainValue.AutoSize = true;
            this.lblTrainValue.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTrainValue.Location = new System.Drawing.Point(12, 39);
            this.lblTrainValue.Name = "lblTrainValue";
            this.lblTrainValue.Size = new System.Drawing.Size(25, 19);
            this.lblTrainValue.TabIndex = 1;
            this.lblTrainValue.Text = "--";
            // 
            // lblTestCaption
            // 
            this.lblTestCaption.AutoSize = true;
            this.lblTestCaption.ForeColor = System.Drawing.Color.FromArgb(110, 110, 110);
            this.lblTestCaption.Location = new System.Drawing.Point(170, 21);
            this.lblTestCaption.Name = "lblTestCaption";
            this.lblTestCaption.Size = new System.Drawing.Size(85, 15);
            this.lblTestCaption.TabIndex = 2;
            this.lblTestCaption.Text = "Test MSE";
            // 
            // lblTestValue
            // 
            this.lblTestValue.AutoSize = true;
            this.lblTestValue.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTestValue.Location = new System.Drawing.Point(170, 39);
            this.lblTestValue.Name = "lblTestValue";
            this.lblTestValue.Size = new System.Drawing.Size(25, 19);
            this.lblTestValue.TabIndex = 3;
            this.lblTestValue.Text = "--";
            // 
            // lblRmseCaption
            // 
            this.lblRmseCaption.AutoSize = true;
            this.lblRmseCaption.ForeColor = System.Drawing.Color.FromArgb(110, 110, 110);
            this.lblRmseCaption.Location = new System.Drawing.Point(328, 21);
            this.lblRmseCaption.Name = "lblRmseCaption";
            this.lblRmseCaption.Size = new System.Drawing.Size(44, 15);
            this.lblRmseCaption.TabIndex = 4;
            this.lblRmseCaption.Text = "RMSE";
            // 
            // lblRmseValue
            // 
            this.lblRmseValue.AutoSize = true;
            this.lblRmseValue.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblRmseValue.Location = new System.Drawing.Point(328, 39);
            this.lblRmseValue.Name = "lblRmseValue";
            this.lblRmseValue.Size = new System.Drawing.Size(25, 19);
            this.lblRmseValue.TabIndex = 5;
            this.lblRmseValue.Text = "--";
            // 
            // lblMaeCaption
            // 
            this.lblMaeCaption.AutoSize = true;
            this.lblMaeCaption.ForeColor = System.Drawing.Color.FromArgb(110, 110, 110);
            this.lblMaeCaption.Location = new System.Drawing.Point(486, 21);
            this.lblMaeCaption.Name = "lblMaeCaption";
            this.lblMaeCaption.Size = new System.Drawing.Size(35, 15);
            this.lblMaeCaption.TabIndex = 6;
            this.lblMaeCaption.Text = "MAE";
            // 
            // lblMaeValue
            // 
            this.lblMaeValue.AutoSize = true;
            this.lblMaeValue.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblMaeValue.Location = new System.Drawing.Point(486, 39);
            this.lblMaeValue.Name = "lblMaeValue";
            this.lblMaeValue.Size = new System.Drawing.Size(25, 19);
            this.lblMaeValue.TabIndex = 7;
            this.lblMaeValue.Text = "--";
            // 
            // lblR2Caption
            // 
            this.lblR2Caption.AutoSize = true;
            this.lblR2Caption.ForeColor = System.Drawing.Color.FromArgb(110, 110, 110);
            this.lblR2Caption.Location = new System.Drawing.Point(644, 21);
            this.lblR2Caption.Name = "lblR2Caption";
            this.lblR2Caption.Size = new System.Drawing.Size(27, 15);
            this.lblR2Caption.TabIndex = 8;
            this.lblR2Caption.Text = "R^2";
            // 
            // lblR2Value
            // 
            this.lblR2Value.AutoSize = true;
            this.lblR2Value.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblR2Value.Location = new System.Drawing.Point(644, 39);
            this.lblR2Value.Name = "lblR2Value";
            this.lblR2Value.Size = new System.Drawing.Size(25, 19);
            this.lblR2Value.TabIndex = 9;
            this.lblR2Value.Text = "--";
            // 
            // lblBaseCaption
            // 
            this.lblBaseCaption.AutoSize = true;
            this.lblBaseCaption.ForeColor = System.Drawing.Color.FromArgb(110, 110, 110);
            this.lblBaseCaption.Location = new System.Drawing.Point(802, 21);
            this.lblBaseCaption.Name = "lblBaseCaption";
            this.lblBaseCaption.Size = new System.Drawing.Size(140, 15);
            this.lblBaseCaption.TabIndex = 10;
            this.lblBaseCaption.Text = "Baseline MSE (mean)";
            // 
            // lblBaseValue
            // 
            this.lblBaseValue.AutoSize = true;
            this.lblBaseValue.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblBaseValue.Location = new System.Drawing.Point(802, 39);
            this.lblBaseValue.Name = "lblBaseValue";
            this.lblBaseValue.Size = new System.Drawing.Size(25, 19);
            this.lblBaseValue.TabIndex = 11;
            this.lblBaseValue.Text = "--";
            // 
            // grpLog
            // 
            this.grpLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpLog.Controls.Add(this.txtLog);
            this.grpLog.Location = new System.Drawing.Point(282, 458);
            this.grpLog.Name = "grpLog";
            this.grpLog.Size = new System.Drawing.Size(600, 294);
            this.grpLog.TabIndex = 5;
            this.grpLog.TabStop = false;
            this.grpLog.Text = "Training log (loss every N epochs)";
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.BackColor = System.Drawing.Color.White;
            this.txtLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 8.25F);
            this.txtLog.Location = new System.Drawing.Point(12, 20);
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.Size = new System.Drawing.Size(576, 264);
            this.txtLog.TabIndex = 0;
            this.txtLog.Text = "";
            this.txtLog.WordWrap = false;
            // 
            // grpSamples
            // 
            this.grpSamples.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSamples.Controls.Add(this.gridSamples);
            this.grpSamples.Location = new System.Drawing.Point(890, 458);
            this.grpSamples.Name = "grpSamples";
            this.grpSamples.Size = new System.Drawing.Size(366, 294);
            this.grpSamples.TabIndex = 6;
            this.grpSamples.TabStop = false;
            this.grpSamples.Text = "Sample predictions (test set)";
            // 
            // gridSamples
            // 
            this.gridSamples.AllowUserToAddRows = false;
            this.gridSamples.AllowUserToDeleteRows = false;
            this.gridSamples.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridSamples.BackgroundColor = System.Drawing.Color.White;
            this.gridSamples.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridSamples.Location = new System.Drawing.Point(12, 20);
            this.gridSamples.Name = "gridSamples";
            this.gridSamples.ReadOnly = true;
            this.gridSamples.RowHeadersVisible = false;
            this.gridSamples.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridSamples.Size = new System.Drawing.Size(342, 264);
            this.gridSamples.TabIndex = 0;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus,
            this.progress});
            this.statusStrip.Location = new System.Drawing.Point(0, 766);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1264, 24);
            this.statusStrip.TabIndex = 7;
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(1030, 19);
            this.lblStatus.Spring = true;
            this.lblStatus.Text = "Ready.";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progress
            // 
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(220, 16);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 790);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.grpSamples);
            this.Controls.Add(this.grpLog);
            this.Controls.Add(this.grpMetrics);
            this.Controls.Add(this.grpScatter);
            this.Controls.Add(this.grpLoss);
            this.Controls.Add(this.grpHyper);
            this.Controls.Add(this.panelTop);
            this.MinimumSize = new System.Drawing.Size(1150, 820);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Flood Prediction - 3-Layer Neural Network Regression";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.grpHyper.ResumeLayout(false);
            this.grpHyper.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSplit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numLogEvery)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEpochs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numHidden)).EndInit();
            this.grpLoss.ResumeLayout(false);
            this.grpScatter.ResumeLayout(false);
            this.grpMetrics.ResumeLayout(false);
            this.grpMetrics.PerformLayout();
            this.grpLog.ResumeLayout(false);
            this.grpSamples.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridSamples)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblDataset;
        private System.Windows.Forms.TextBox txtDatasetPath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblDatasetInfo;

        private System.Windows.Forms.GroupBox grpHyper;
        private System.Windows.Forms.Label lblInLR;
        private System.Windows.Forms.TextBox txtInLR;
        private System.Windows.Forms.Label lblOutLR;
        private System.Windows.Forms.TextBox txtOutLR;
        private System.Windows.Forms.Label lblHidden;
        private System.Windows.Forms.NumericUpDown numHidden;
        private System.Windows.Forms.Label lblEpochs;
        private System.Windows.Forms.NumericUpDown numEpochs;
        private System.Windows.Forms.Label lblLogEvery;
        private System.Windows.Forms.NumericUpDown numLogEvery;
        private System.Windows.Forms.Label lblSplit;
        private System.Windows.Forms.NumericUpDown numSplit;
        private System.Windows.Forms.Label lblSeed;
        private System.Windows.Forms.NumericUpDown numSeed;
        private System.Windows.Forms.Label lblTuneSection;
        private System.Windows.Forms.Label lblTuneLr;
        private System.Windows.Forms.TextBox txtTuneLr;
        private System.Windows.Forms.Label lblTuneHidden;
        private System.Windows.Forms.TextBox txtTuneHidden;
        private System.Windows.Forms.Button btnTrain;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnAutoTune;
        private System.Windows.Forms.Button btnSaveWeights;
        private System.Windows.Forms.Button btnLoadWeights;
        private System.Windows.Forms.Button btnExportLog;
        private System.Windows.Forms.Button btnExportPred;
        private System.Windows.Forms.Label lblArchitecture;
        private System.Windows.Forms.Label lblWeightsHint;

        private System.Windows.Forms.GroupBox grpLoss;
        private FloodPrediction.Controls.LossCurveChart lossChart;
        private System.Windows.Forms.GroupBox grpScatter;
        private FloodPrediction.Controls.PredictionScatterChart scatterChart;

        private System.Windows.Forms.GroupBox grpMetrics;
        private System.Windows.Forms.Label lblTrainCaption;
        private System.Windows.Forms.Label lblTrainValue;
        private System.Windows.Forms.Label lblTestCaption;
        private System.Windows.Forms.Label lblTestValue;
        private System.Windows.Forms.Label lblRmseCaption;
        private System.Windows.Forms.Label lblRmseValue;
        private System.Windows.Forms.Label lblMaeCaption;
        private System.Windows.Forms.Label lblMaeValue;
        private System.Windows.Forms.Label lblR2Caption;
        private System.Windows.Forms.Label lblR2Value;
        private System.Windows.Forms.Label lblBaseCaption;
        private System.Windows.Forms.Label lblBaseValue;

        private System.Windows.Forms.GroupBox grpLog;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.GroupBox grpSamples;
        private System.Windows.Forms.DataGridView gridSamples;

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripProgressBar progress;
    }
}
