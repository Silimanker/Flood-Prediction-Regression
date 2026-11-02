using System;
using System.IO;

namespace Backprop
{
    /// <summary>
    /// A 3-layer Multi-Layer Perceptron (Input -&gt; Hidden -&gt; Output) trained with
    /// online (per-sample) gradient descent and back-propagation.
    ///
    /// This is the engine supplied as sample code for the activity, extended with the
    /// pieces the activity requires but the sample lacked:
    ///   * tunable input/output learning rates (the sample hard-coded them as const),
    ///   * a mean-squared-error routine (required for regression + the training log),
    ///   * an explicit random seed so hyper-parameter experiments are reproducible,
    ///   * numerically safe sigmoid evaluation.
    ///
    /// The forward pass and the weight-update equations are unchanged from the sample.
    /// </summary>
    /// <remarks>
    /// Data-flow for one training pattern:
    ///   1. run()            - forward pass: input -&gt; hidden activations -&gt; output activation
    ///   2. calculateEC()    - errorComponent  = target - output
    ///   3. calculateDER()   - errorDerivative = output * (1 - output) * errorComponent
    ///   4. learn()          - hidden deltas, then every weight/bias is nudged downhill
    /// </remarks>
    public class NeuralNet
    {
        private INeuron[] ineuron;
        private HNeuron[] hneuron;
        private ONeuron[] oneuron;

        // Learning rates. In the original sample these were `const`, which made the
        // "tune the learning rate" objective of the activity impossible to satisfy.
        private double lrpout = 0.2;    // learning rate for the output layer
        private double lrpin = 0.15;    // learning rate for the input layer

        private double[] errorComponent;
        private double[] errorDerivative;
        private double[] desiredout;

        /// <summary>Default network: 3072 inputs, 64 hidden, 10 outputs (sample code default).</summary>
        public NeuralNet()
            : this(3072, 64, 10)
        {
        }

        /// <summary>
        /// Builds a network with the requested architecture.
        /// </summary>
        /// <param name="input">Number of input neurons (one per normalised feature).</param>
        /// <param name="hidden">Number of neurons in the single hidden layer.</param>
        /// <param name="output">Number of output neurons (1 for this regression task).</param>
        public NeuralNet(int input, int hidden, int output)
        {
            ineuron = new INeuron[input];
            hneuron = new HNeuron[hidden];
            oneuron = new ONeuron[output];
            errorComponent = new double[output];
            errorDerivative = new double[output];
            desiredout = new double[output];
            createNeurons(ineuron.Length, hneuron.Length, oneuron.Length);
        }

        // ------------------------------------------------------------------ #
        //  Architecture + tunable hyper-parameters
        // ------------------------------------------------------------------ #

        public int InputCount { get { return ineuron.Length; } }
        public int HiddenCount { get { return hneuron.Length; } }
        public int OutputCount { get { return oneuron.Length; } }

        /// <summary>Learning rate applied to the hidden-to-output weights and biases.</summary>
        public double OutputLearningRate
        {
            get { return lrpout; }
            set { lrpout = value; }
        }

        /// <summary>Learning rate applied to the input-to-hidden weights and biases.</summary>
        public double InputLearningRate
        {
            get { return lrpin; }
            set { lrpin = value; }
        }

        /// <summary>
        /// Seeds the RNG shared by all three neuron classes. Call this before building
        /// a network so weight initialisation is reproducible.
        /// </summary>
        public static void SeedRandom(int seed)
        {
            INeuron.SeedRandom(seed);
            HNeuron.SeedRandom(seed + 1);
            ONeuron.SeedRandom(seed + 2);
        }

        /// <summary>
        /// Seeds the RNG and immediately re-draws every weight and bias, so an existing
        /// network can be reset to a known initial state.
        /// </summary>
        public void seedWeights(int seed)
        {
            SeedRandom(seed);
            createNeurons(ineuron.Length, hneuron.Length, oneuron.Length);
        }

        public void createNeurons(int i, int h, int o)
        {
            for (int x = 0; x < i; x++)
            {
                ineuron[x] = new INeuron(x, h);
            }
            for (int x = 0; x < h; x++)
            {
                hneuron[x] = new HNeuron(x, o);
            }
            for (int x = 0; x < o; x++)
            {
                oneuron[x] = new ONeuron(x);
            }
        }

        // ------------------------------------------------------------------ #
        //  Feeding data in / reading the prediction out
        // ------------------------------------------------------------------ #

        public double getOuputData(int pos)
        {
            return oneuron[pos].getOActivation();
        }

        public void setInputs(int pos, double data)
        {
            ineuron[pos].setInput(data);
        }

        public void setDesiredOutput(int pos, double data)
        {
            desiredout[pos] = data;
        }

        /// <summary>
        /// Sigmoid squashing function. Large positive/negative net inputs are clamped
        /// first so Math.Exp can never overflow or underflow to Infinity/NaN.
        /// </summary>
        public double sigmoid(double dat)
        {
            if (dat > 700.0) dat = 700.0;
            if (dat < -700.0) dat = -700.0;
            return 1.0 / (1.0 + Math.Exp(-dat));
        }

        // ------------------------------------------------------------------ #
        //  Forward pass
        // ------------------------------------------------------------------ #

        /// <summary>Activates the hidden layer from the current inputs.</summary>
        public void calculateHA()
        {
            for (int x = 0; x < hneuron.Length; x++)
            {
                double summation = 0.0;
                for (int y = 0; y < ineuron.Length; y++)
                {
                    summation += ineuron[y].getInput() * ineuron[y].getWeight(x);
                }
                hneuron[x].setHactivation(sigmoid(summation + hneuron[x].getBias()));
            }
        }

        /// <summary>Activates the output layer from the hidden layer.</summary>
        public void calculateOA()
        {
            for (int x = 0; x < oneuron.Length; x++)
            {
                double summation = 0.0;
                for (int y = 0; y < hneuron.Length; y++)
                {
                    summation += hneuron[y].getHactivation() * hneuron[y].getWeight(x);
                }
                oneuron[x].setOActivation(sigmoid(summation + oneuron[x].getBias()));
            }
        }

        /// <summary>Runs a full forward pass without touching any weights.</summary>
        public void run()
        {
            this.calculateHA();
            this.calculateOA();
        }

        /// <summary>
        /// Convenience helper: loads an input vector, runs the forward pass and
        /// returns the output vector. Weights are never modified.
        /// </summary>
        public double[] Forward(double[] inputs)
        {
            int n = Math.Min(inputs.Length, ineuron.Length);
            for (int x = 0; x < n; x++)
            {
                ineuron[x].setInput(inputs[x]);
            }
            run();

            double[] result = new double[oneuron.Length];
            for (int x = 0; x < oneuron.Length; x++)
            {
                result[x] = oneuron[x].getOActivation();
            }
            return result;
        }

        // ------------------------------------------------------------------ #
        //  Loss
        // ------------------------------------------------------------------ #

        /// <summary>
        /// Mean squared error between the desired outputs set via
        /// <see cref="setDesiredOutput"/> and the activations produced by the last
        /// call to <see cref="run"/>. Call run() first.
        /// </summary>
        public double ComputeSquaredError()
        {
            double sum = 0.0;
            for (int x = 0; x < oneuron.Length; x++)
            {
                double diff = desiredout[x] - oneuron[x].getOActivation();
                sum += diff * diff;
            }
            return sum / oneuron.Length;
        }

        // ------------------------------------------------------------------ #
        //  Back-propagation
        // ------------------------------------------------------------------ #

        /// <summary>errorComponent[x] = target[x] - output[x]</summary>
        public void calculateEC()
        {
            for (int x = 0; x < oneuron.Length; x++)
            {
                errorComponent[x] = (desiredout[x]) - (oneuron[x].getOActivation());
            }
        }

        /// <summary>errorDerivative[x] = output[x] * (1 - output[x]) * errorComponent[x]</summary>
        public void calculateDER()
        {
            for (int x = 0; x < oneuron.Length; x++)
            {
                errorDerivative[x] = oneuron[x].getOActivation() * (1 - oneuron[x].getOActivation()) * errorComponent[x];
            }
        }

        /// <summary>
        /// One full training step on the pattern currently held by the network:
        /// forward pass -&gt; error -&gt; deltas -&gt; weight and bias updates.
        ///
        /// Ordering matters and is deliberately preserved from the sample: the hidden
        /// deltas are computed with the *pre-update* hidden-to-output weights, and only
        /// afterwards are those weights changed, so the input-to-hidden update still
        /// sees a consistent error signal.
        /// </summary>
        public void learn()
        {
            this.run();
            this.calculateEC();
            this.calculateDER();

            for (int x = 0; x < hneuron.Length; x++)              // hidden deltas (uses old w)
                hneuron[x].calculateErr(errorDerivative);

            for (int x = 0; x < hneuron.Length; x++)              // update hidden -> output weights
                hneuron[x].setWeight(lrpout, errorDerivative);

            for (int x = 0; x < ineuron.Length; x++)              // update input -> hidden weights
            {
                for (int y = 0; y < hneuron.Length; y++)
                {
                    ineuron[x].setWeight(y, hneuron[y].getErr(), lrpin);
                }
            }

            for (int x = 0; x < oneuron.Length; x++)              // update output biases
                oneuron[x].changeBias(lrpout, errorDerivative);

            for (int x = 0; x < hneuron.Length; x++)              // update hidden biases
                hneuron[x].changeBias(lrpin);
        }

        /// <summary>
        /// Loads one labelled pattern, performs a forward pass and a weight update.
        /// </summary>
        public void TrainPattern(double[] inputs, double[] targets)
        {
            int n = Math.Min(inputs.Length, ineuron.Length);
            for (int x = 0; x < n; x++)
            {
                ineuron[x].setInput(inputs[x]);
            }
            int m = Math.Min(targets.Length, desiredout.Length);
            for (int x = 0; x < m; x++)
            {
                desiredout[x] = targets[x];
            }
            learn();
        }

        // ------------------------------------------------------------------ #
        //  Persistence
        // ------------------------------------------------------------------ #

        /// <summary>
        /// Writes every weight and bias to <paramref name="path"/>, one value per line,
        /// in the exact order expected by <see cref="loadWeights"/>.
        /// </summary>
        public void saveWeights(String path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                for (int x = 0; x < ineuron.Length; x++)
                {
                    for (int y = 0; y < hneuron.Length; y++)
                        sw.WriteLine(ineuron[x].getWeight(y));
                }
                for (int x = 0; x < hneuron.Length; x++)
                {
                    for (int y = 0; y < oneuron.Length; y++)
                    {
                        sw.WriteLine(hneuron[x].getWeight(y));
                    }
                }
                for (int x = 0; x < hneuron.Length; x++)
                {
                    sw.WriteLine(hneuron[x].getBias());
                }
                for (int x = 0; x < oneuron.Length; x++)
                {
                    sw.WriteLine(oneuron[x].getBias());
                }
            }
        }

        /// <summary>
        /// Restores weights previously written by <see cref="saveWeights"/>. The network
        /// must already have been constructed with a matching architecture.
        /// </summary>
        public void loadWeights(String path)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                for (int x = 0; x < ineuron.Length; x++)
                {
                    for (int y = 0; y < hneuron.Length; y++)
                    {
                        ineuron[x].setWeight(y, Convert.ToDouble(sr.ReadLine()));
                    }
                }
                for (int x = 0; x < hneuron.Length; x++)
                {
                    for (int y = 0; y < oneuron.Length; y++)
                    {
                        hneuron[x].setWeight(y, Convert.ToDouble(sr.ReadLine()));
                    }
                }
                for (int x = 0; x < hneuron.Length; x++)
                {
                    hneuron[x].setBias(Convert.ToDouble(sr.ReadLine()));
                }
                for (int x = 0; x < oneuron.Length; x++)
                {
                    oneuron[x].setBias(Convert.ToDouble(sr.ReadLine()));
                }
            }
        }
    }
}
