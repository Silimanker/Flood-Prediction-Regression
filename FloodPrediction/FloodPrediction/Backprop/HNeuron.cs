using System;

namespace Backprop
{
    /// <summary>
    /// Hidden-layer neuron (the middle layer of the 3-layer MLP).
    ///
    /// Stores its activation, its bias, the weights it sends to the output layer and
    /// the error term (delta) computed during back-propagation.
    /// </summary>
    /// <remarks>
    /// Part of the supplied "backneural" sample library. Public surface preserved;
    /// documentation and an explicit random seed were added.
    /// </remarks>
    public class HNeuron
    {
        private double[] weights;      // weights[i] = link from this neuron to output neuron i
        private int idno;
        private int wsize;
        private double hactivation;
        private double bias;
        private double error;

        private static System.Random rand;

        /// <summary>Parameterless constructor kept for compatibility with the sample code.</summary>
        public HNeuron() : this(0, 10)
        {
        }

        /// <summary>
        /// Creates a hidden neuron that feeds <paramref name="size"/> output neurons.
        /// </summary>
        /// <param name="idnodata">Zero-based index of this neuron in the hidden layer.</param>
        /// <param name="size">Number of output neurons this neuron feeds.</param>
        public HNeuron(int idnodata, int size)
        {
            idno = idnodata;
            hactivation = 0.0;
            bias = 0.01;
            wsize = size;
            error = 0.0;
            weights = new double[wsize];
            this.setRandomWeights(wsize);
        }

        /// <summary>Seeds the shared RNG used for weight initialisation.</summary>
        public static void SeedRandom(int seed)
        {
            rand = new System.Random(seed);
        }

        /// <summary>
        /// Computes this neuron's back-propagated error term:
        ///   delta = activation * (1 - activation) * SUM(outputDelta[i] * weight[i])
        /// i.e. the chain rule applied through the sigmoid activation.
        /// </summary>
        /// <param name="der">Delta of every output neuron (pre-update weights).</param>
        public void calculateErr(double[] der)
        {
            double result = 0.0;
            for (int x = 0; x < der.Length; x++)
            {
                result += (der[x] * weights[x]);
            }
            error = (result * hactivation * (1 - hactivation));
        }

        /// <summary>
        /// Back-propagation update for the hidden-to-output weights:
        ///   w <- w + learningRate * outputDelta * hiddenActivation
        /// </summary>
        /// <param name="lrpout">Learning rate for the output layer.</param>
        /// <param name="der">Delta of every output neuron.</param>
        public void setWeight(double lrpout, double[] der)
        {
            double temp = hactivation * lrpout;
            for (int x = 0; x < der.Length; x++)
            {
                weights[x] += (temp * der[x]);
            }
        }

        /// <summary>Direct weight assignment (used when loading a trained model).</summary>
        public void setWeight(int pos, double dat)
        {
            weights[pos] = dat;
        }

        public double getWeight(int outid)
        {
            return weights[outid];
        }

        public void setErr(double x)
        {
            error = x;
        }

        public double getErr()
        {
            return error;
        }

        public void setHactivation(double data)
        {
            this.hactivation = data;
        }

        public double getHactivation()
        {
            return hactivation;
        }

        public void setIdno(int x)
        {
            idno = x;
        }

        public int getIdno()
        {
            return idno;
        }

        public void setBias(double b)
        {
            bias = b;
        }

        public double getBias()
        {
            return bias;
        }

        /// <summary>
        /// Back-propagation update for this neuron's bias:
        ///   bias <- bias + learningRate * delta
        /// (the bias behaves like a weight fed a constant input of 1).
        /// </summary>
        public void changeBias(double lrpin)
        {
            bias += (error * lrpin);
        }

        public void setRandomWeights(int size)
        {
            for (int x = 0; x < size; x++)
            {
                weights[x] = this.randomweight();
            }
        }

        private double randomweight()
        {
            if (rand == null)
            {
                rand = new System.Random();
            }
            return rand.Next(-1000, 1000) / 2000.0;
        }
    }
}
