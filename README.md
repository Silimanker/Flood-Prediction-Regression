# Flood Prediction Regression — Neural Network in C# (WinForms)

A from-scratch multilayer perceptron (MLP) trained with back-propagation to predict the **FloodProbability** of a region from 20 environmental and socio-political indicators. 

Everything — CSV parsing, Min-Max feature scaling, the neural network, the training engine, custom GDI+ loss and scatter charts, and the automatic grid search — is implemented entirely in C# (.NET Framework 4.7.2). **No external machine-learning libraries are used**; the model is built directly on a 3-layer architecture (`INeuron`, `HNeuron`, `ONeuron`, `NeuralNet` in the `Backprop` namespace).

---

## 1. Dataset

**File:** `FloodPredictionDataSet\flood.csv` — 50,000 rows × 21 columns (20 features + 1 target).

| Attribute | Details |
|---|---|
| **Target column** | `FloodProbability` (continuous float, range **0.285 – 0.725**, mean **0.4997**, std **0.05003**) |
| **Feature types** | Small non-negative integers representing ordinal factor scores (roughly 0–15) |
| **Missing values** | None |
| **Split size** | 40,000 training patterns (80%), 10,000 held-out test patterns (20%) |

**Features (in file order):**

`MonsoonIntensity`, `TopographyDrainage`, `RiverManagement`, `Deforestation`, `Urbanization`,
`ClimateChange`, `DamsQuality`, `Siltation`, `AgriculturalPractices`, `Encroachments`,
`IneffectiveDisasterPreparedness`, `DrainageSystems`, `CoastalVulnerability`, `Landslides`,
`Watersheds`, `DeterioratingInfrastructure`, `PopulationScore`, `WetlandLoss`,
`InadequatePlanning`, `PoliticalFactors`.

Each feature represents an environmental, infrastructural, or socio-political factor contributing to flood risk. The target is the region's overall modelled flood probability.

**Baseline to beat (predict-the-mean test MSE) = `0.002489`.**  
Any functional predictive model must achieve a test error significantly lower than this trivial baseline.

---

## 2. Preprocessing

1. **Parsing:** `Data\FloodDataset.cs` loads the CSV, verifies headers, and parses values with culture-invariant numeric handling, maintaining separation between raw and scaled data.
2. **Train / Test Split:** Seeded Fisher–Yates shuffle splits the data into **80% training (40,000 rows)** and **20% held-out test (10,000 rows)** (default random seed **42**). The split is executed *before* any feature statistics are calculated.
3. **Feature Scaling (Min-Max):** `Data\MinMaxScaler.cs` normalizes every feature column to $[0, 1]$ using:
   $$x' = \frac{x - \min_{\text{train}}}{\max_{\text{train}} - \min_{\text{train}}}$$
   Crucially, minimums and maximums are fitted on the **training split only** and applied to the test split, preventing data leakage.
4. **Target Handling:** The target `FloodProbability` is continuous and naturally bounded within $[0.285, 0.725] \subset [0, 1]$, fitting the range of the sigmoid output node without requiring artificial transformation.

---

## 3. Model and Back-Propagation Architecture

```
20 Input Neurons  ──>  H Hidden Neurons (Sigmoid)  ──>  1 Output Neuron (Sigmoid)
```

### Forward Pass

For each input pattern $x$:

$$h_j = \sigma\left(\sum_{i=1}^{20} w_{ij} x_i + b_j\right), \quad \sigma(z) = \frac{1}{1 + e^{-z}}$$

$$y = \sigma\left(\sum_{j=1}^{H} v_j h_j + c\right)$$

where:
* $w_{ij}$ and $b_j$ are the input-to-hidden weights and biases.
* $v_j$ and $c$ are the hidden-to-output weights and bias.
* $\sigma(z)$ includes a numerical stability guard to prevent overflow for large negative activations.

### Loss Function (Mean Squared Error)

Evaluated across $N$ patterns:

$$\text{MSE} = \frac{1}{N} \sum_{k=1}^N (t_k - y_k)^2$$

### Back-Propagation (Online SGD)

During each epoch, training patterns are presented in shuffled order:

1. **Output error delta:**
   $$\delta_{\text{out}} = y \cdot (1 - y) \cdot (t - y)$$
2. **Hidden layer error deltas (using pre-update output weights):**
   $$\delta_{h,j} = h_j \cdot (1 - h_j) \cdot (v_j \cdot \delta_{\text{out}})$$
3. **Weight and bias updates:**
   $$v_j \leftarrow v_j + \eta_{\text{out}} \cdot \delta_{\text{out}} \cdot h_j, \quad c \leftarrow c + \eta_{\text{out}} \cdot \delta_{\text{out}}$$
   $$w_{ij} \leftarrow w_{ij} + \eta_{\text{in}} \cdot \delta_{h,j} \cdot x_i, \quad b_j \leftarrow b_j + \eta_{\text{in}} \cdot \delta_{h,j}$$

where $\eta_{\text{in}}$ and $\eta_{\text{out}}$ are tunable input and output learning rates.

---

## 4. Graphical User Interface (WinForms)

The application provides a responsive Windows Forms desktop GUI allowing full observation and control over the training process:

| Component | Functionality |
|---|---|
| **Dataset Bar** | Auto-detects `flood.csv`; includes a file browser dialog and shows dataset profile summary |
| **Hyperparameter Controls** | Settable input LR, output LR, hidden node count, epochs, log interval, split %, and seed |
| **Auto-Tune Grid** | Editable lists of learning rates and hidden sizes to execute automatic grid search sweeps |
| **Control Buttons** | `Train`, `Stop`, `Auto-Tune Grid`, `Save Weights`, `Load Weights`, `Export Training Log (CSV)`, `Export Predictions (CSV)` |
| **Training Loss Chart** | Custom GDI+ line plot rendering real-time train and test MSE curves per epoch |
| **Predicted vs Actual Chart** | Custom GDI+ scatter plot of test set predictions hugging the ideal $45^\circ$ reference line |
| **Evaluation Metrics** | Live stat tiles: Train MSE, Test MSE, RMSE, MAE, $R^2$, and Baseline MSE (color-coded) |
| **Training Log Console** | Scrollable timestamped logging console with epoch checkpoints and summary tables |
| **Sample Predictions Table** | Test sample inspection table displaying actual values, predictions, and residual errors |
| **Status Bar** | Real-time progress updates with epoch counters and progress bars |

*Note: All charts are drawn directly using GDI+ in `Controls\LossCurveChart.cs` and `Controls\PredictionScatterChart.cs` without third-party chart dependencies.*

---

## 5. Hyper-Parameter Tuning & Empirical Results

### Grid Search Design
To systematically identify the optimal hyperparameters without manual trial-and-error, the built-in **Auto-Tune Grid** was executed over:
* **Learning Rates:** $\{0.02, 0.05, 0.10\}$
* **Hidden Layer Sizes:** $\{8, 16, 32\}$
* **Budget:** 300 epochs per configuration (40k train / 10k test, seed 42)

Test MSE was sampled at 4 even checkpoints (**75, 150, 225, and 300 epochs**) to simultaneously measure convergence speed and final accuracy.

### Measured Grid Search Results (`results/hyperparameter_tuning_results.csv`)

| Rank | Learning Rate | Hidden Nodes | Test MSE @75 ep | Test MSE @150 ep | Test MSE @225 ep | Test MSE @300 ep | Final Test RMSE | Final Test MAE | Final Test $R^2$ |
|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| 🥇 **1** | **0.10** | **8** | 0.0000024 | 0.0000014 | 0.0000010 | **0.00000076** | **0.000870** | **0.000590** | **0.999696** |
| 2 | 0.10 | 16 | 0.0000028 | 0.0000018 | 0.0000013 | 0.00000099 | 0.000998 | 0.000706 | 0.999600 |
| 3 | 0.10 | 32 | 0.0000022 | 0.0000016 | 0.0000014 | 0.00000106 | 0.001030 | 0.000741 | 0.999574 |
| 4 | 0.05 | 8 | 0.0000039 | 0.0000024 | 0.0000018 | 0.00000138 | 0.001174 | 0.000818 | 0.999446 |
| 5 | 0.05 | 32 | 0.0000027 | 0.0000022 | 0.0000019 | 0.00000164 | 0.001282 | 0.000955 | 0.999340 |
| 6 | 0.05 | 16 | 0.0000038 | 0.0000028 | 0.0000022 | 0.00000179 | 0.001338 | 0.000973 | 0.999281 |
| 7 | 0.02 | 32 | 0.0000031 | 0.0000028 | 0.0000026 | 0.00000243 | 0.001559 | 0.001183 | 0.999023 |
| 8 | 0.02 | 8 | 0.0000061 | 0.0000045 | 0.0000035 | 0.00000286 | 0.001691 | 0.001210 | 0.998851 |
| 9 | 0.02 | 16 | 0.0000048 | 0.0000041 | 0.0000036 | 0.00000315 | 0.001775 | 0.001311 | 0.998734 |

*(Baseline test MSE = `0.002489`. All configurations achieve >99.8% error reduction over baseline.)*

### Independent Headless Validation Sweep (`results/grid_search_harness.csv`)

| Rank | Learning Rate | Hidden Nodes | MSE @50 ep | MSE @100 ep | MSE @200 ep | MSE @300 ep | Final $R^2$ |
|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| 1 | **0.10** | **8** | 0.000003 | 0.000002 | 0.000001 | **0.000001** | **0.9997** |
| 2 | 0.10 | 16 | 0.000003 | 0.000002 | 0.000001 | 0.000001 | 0.9996 |
| 3 | 0.10 | 32 | 0.000003 | 0.000002 | 0.000001 | 0.000001 | 0.9996 |
| 4 | 0.05 | 8 | 0.000005 | 0.000003 | 0.000002 | 0.000001 | 0.9994 |
| 5 | 0.05 | 32 | 0.000003 | 0.000003 | 0.000002 | 0.000002 | 0.9993 |
| 6 | 0.05 | 16 | 0.000004 | 0.000003 | 0.000002 | 0.000002 | 0.9993 |
| 7 | 0.02 | 32 | 0.000003 | 0.000003 | 0.000003 | 0.000002 | 0.9990 |
| 8 | 0.02 | 8 | 0.000007 | 0.000006 | 0.000004 | 0.000003 | 0.9989 |
| 9 | 0.02 | 16 | 0.000005 | 0.000005 | 0.000004 | 0.000003 | 0.9987 |

### Empirical Analysis
* **Dominance of Learning Rate:** Learning rate was the single most impactful factor ($0.10 > 0.05 > 0.02$). At $\eta = 0.02$, descent was too slow to reach the loss floor within 300 epochs. At $\eta = 0.10$, convergence was rapid and stable without oscillation.
* **Network Parsimony (Hidden Size):** 8 hidden nodes ranked first overall. Because the flood probability mapping is smooth, 8 hidden neurons provide ample non-linear expressiveness. Adding 16 or 32 nodes quadrupled weight counts without providing any generalization advantage.
* **Epoch Diminishing Returns:** Over $90\%$ of total error reduction occurs within the first 100–150 epochs. Training past 200 epochs provides incremental refinements at the 6th–7th decimal place of MSE.
* **Optimal Architecture:** **$\text{LR} = 0.10$, $\text{Hidden Nodes} = 8$, $\text{Epochs} = 300$**.

---

## 6. Evaluation Metrics

All metrics are evaluated on the held-out test split of 10,000 patterns (`Training\Metrics.cs`):

| Metric | Formula | Practical Interpretation |
|---|---|---|
| **MSE** | $\frac{1}{N} \sum (t - y)^2$ | Primary regression loss ($0.00000076 \approx 10^{-6}$); penalizes larger deviations |
| **RMSE** | $\sqrt{\text{MSE}}$ | Typical error in target units ($0.000870$, or $<0.09\%$ on a 0–1 probability scale) |
| **MAE** | $\frac{1}{N} \sum \|t - y\|$ | Mean absolute error ($0.000590$, or $<0.06\%$ probability error) |
| **$R^2$** | $1 - \frac{\sum (t - y)^2}{\sum (t - \bar{t})^2}$ | Coefficient of determination (**0.9997**; explains $99.97\%$ of target variance) |
| **Baseline MSE** | $\frac{1}{N} \sum (t - \bar{t}_{\text{train}})^2$ | Trivial "always predict the mean" benchmark ($0.002489$) |

The tuned network beats the baseline by **three orders of magnitude** (MSE dropped from $0.002489$ to $0.00000076$), with $100.0\%$ of test predictions falling within $\pm 0.05$ of the true target.

---

## 7. Build and Run

### Visual Studio
1. Open `FloodPrediction\FloodPrediction.slnx` in Visual Studio (2022 / 2026 Community or higher).
2. Set configuration to `Debug` or `Release` and press **F5** (or click Start).

### Command Line (MSBuild)
```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" `
    "FloodPrediction\FloodPrediction\FloodPrediction.csproj" /p:Configuration=Debug /v:minimal /nologo
```
Executable output: `FloodPrediction\FloodPrediction\bin\Debug\FloodPrediction.exe`.

### Handing In via GitHub
```powershell
cd "C:\Users\Surface\Documents\IS\FloodPredictionRegression"
git remote add origin <your-github-repo-url>
git push -u origin master
```

---

## 8. Project Structure

```
FloodPredictionRegression\
├─ README.md                                  ← Main project documentation
├─ About Dataset.docx                         ← Dataset information
├─ results\                                   ← Evaluation & tuning deliverables
│  ├─ hyperparameter_tuning_results.csv       ← 9-configuration grid comparison table (CSV)
│  ├─ grid_search_harness.csv                 ← Headless test harness results table (CSV)
│  ├─ grid_search_harness.txt                 ← Headless test harness raw console output (TXT)
│  ├─ training_log_1_184936 (.txt / .csv)     ← Run 1: initial 300-epoch baseline training run
│  ├─ training_log_2_194903 (.txt / .csv)     ← Run 2: verification training run
│  └─ training_log_3_203115_autotune (.txt / .csv) ← Run 3: full Auto-Tune sweep & results
├─ FloodPredictionDataSet\                    ← Clean dataset folder
│  └─ flood.csv                               ← 50,000 rows × 20 features + FloodProbability
└─ FloodPrediction\                           ← Visual Studio solution
   └─ FloodPrediction\
      ├─ Program.cs                           entry point
      ├─ Form1.cs                             GUI logic: training, grid search, logging, exports
      ├─ Form1.Designer.cs                    form layout and control definitions
      ├─ Backprop\                            ← 3-layer neural network classes
      │  ├─ INeuron.cs                        input neuron class
      │  ├─ HNeuron.cs                        hidden neuron class
      │  ├─ ONeuron.cs                        output neuron class
      │  └─ NeuralNet.cs                      MLP feed-forward, SGD backprop, and error calculation
      ├─ Data\
      │  ├─ FloodDataset.cs                   CSV loader, matrix management, and seeded train/test split
      │  └─ MinMaxScaler.cs                   leak-free [0,1] normalization fitted on training data
      ├─ Training\
      │  ├─ TrainingEngine.cs                 epoch loop, background workers, and evaluation
      │  ├─ HyperParameterTuner.cs            systematic grid search and checkpoint sampling
      │  └─ Metrics.cs                        regression metrics (MSE, RMSE, MAE, R², baseline)
      └─ Controls\
         ├─ ChartPainter.cs                   GDI+ coordinate and axis drawing helpers
         ├─ LossCurveChart.cs                 custom real-time training & test loss curve
         └─ PredictionScatterChart.cs         custom real-time predicted vs actual scatter plot
```

---

## 9. Reproducibility

* **Deterministic Seeding:** Both dataset shuffling and neural network weight initialization are deterministically seeded (default seed `42`), ensuring exact bit-for-bit reproducibility across runs.
* **Data Isolation:** Normalization statistics ($\min, \max$) are strictly derived from the training set only, preventing any leakage of test set information into model training.
* **Model Serialization:** Trained network weights and their architecture configuration can be saved and reloaded using the GUI's `Save Weights` and `Load Weights` features.
