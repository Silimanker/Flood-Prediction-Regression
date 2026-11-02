using System;

namespace Backprop
{
    /// <summary>
    /// Input-layer neuron (one per feature/column of the dataset).
    ///
    /// Holds the single feature value currently being presented to the network plus
    /// the weight vector that connects this neuron to every hidden-layer neuron.
    /// </summary>
    /// <remarks>
    /// This class is part of the supplied "backneural" sample library. The public
    /// surface has been kept intact (the activity requires using the given classes);
    /// only documentation and an explicit random seed were added so that
    /// hyper-parameter experiments can be reproduced.
    /// </remarks>
    public class INeuron
    {
        private int idno;
        private double input;
        private double[] weights;      // weights[i] = link from this neuron to hidden neuron i
        private int wsize;

        /// <summary>Shared RNG so that a whole network can be seeded reproducibly.</summary>
        private static System.Random rand;

        /// <summary>Parameterless constructor kept for compatibility with the sample code.</summary>
        public INeuron() : this(0, 64)
        {
        }

        /// <summary>
        /// Creates an input neuron whose outgoing weight vector connects to
        /// <paramref name="size"/> hidden neurons.
        /// </summary>
        /// <param name="idnodata">Zero-based index of this neuron in the input layer.</param>
        /// <param name="size">Number of hidden neurons this neuron feeds.</param>
        public INeuron(int idnodata, int size)
        {
            idno = idnodata;
            input = 0;
            wsize = size;
            weights = new double[wsize];
            this.setRandomWeights(wsize);
        }

        /// <summary>
        /// Seeds the shared random number generator. Call this <b>before</b> building a
        /// network so that weight initialisation (and therefore an entire training run)
        /// can be reproduced exactly.
        /// </summary>
        public static void SeedRandom(int seed)
        {
            rand = new System.Random(seed);
        }

        /// <summary>
        /// Draws a small random weight from U(-0.5, +0.5). With inputs already Min-Max
        /// scaled to [0,1] this keeps the initial hidden pre-activations close to the
        /// flat, high-gradient part of the sigmoid (no saturation at start-up).
        /// </summary>
        private double randomweight()
        {
            if (rand == null)
            {
                rand = new System.Random();
            }
            return rand.Next(-1000, 1000) / 2000.0;
        }

        /// <summary>Fills the outgoing weight vector with fresh random values.</summary>
        public void setRandomWeights(int size)
        {
            for (int x = 0; x < size; x++)
            {
                weights[x] = this.randomweight();
            }
        }

        /// <summary>
        /// Back-propagation update for the input-to-hidden weights.
        ///   w <- w + learningRate * hiddenDelta * inputValue
        /// </summary>
        /// <param name="hidno">Index of the hidden neuron being updated.</param>
        /// <param name="err">Delta of that hidden neuron (already includes the sigmoid derivative).</param>
        /// <param name="lrpin">Learning rate for the input layer.</param>
        public void setWeight(int hidno, double err, double lrpin)
        {
            double errlrpin = err * lrpin;
            weights[hidno] += (errlrpin * input);
        }

        public void setID(int no)
        {
            idno = no;
        }

        /// <summary>Presents one normalised feature value to the network.</summary>
        public void setInput(double data)
        {
            input = data;
        }

        public double getInput()
        {
            return input;
        }

        public double getWeight(int hidno)
        {
            return weights[hidno];
        }

        public int getID()
        {
            return idno;
        }

        /// <summary>Direct weight assignment (used when loading a trained model).</summary>
        public void setWeight(int pos, double dat)
        {
            weights[pos] = dat;
        }
    }
}
