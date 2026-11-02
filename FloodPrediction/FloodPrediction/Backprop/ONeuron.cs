using System;

namespace Backprop
{
    /// <summary>
    /// Output-layer neuron. For this regression task the network has a single output
    /// neuron whose sigmoid activation is the predicted <b>FloodProbability</b>.
    /// </summary>
    /// <remarks>
    /// Part of the supplied "backneural" sample library. Public surface preserved;
    /// documentation and an explicit random seed were added.
    /// </remarks>
    public class ONeuron
    {
        private double bias;
        private int idno;
        private double OActivation;

        private static System.Random rand;

        /// <summary>Parameterless constructor kept for compatibility with the sample code.</summary>
        public ONeuron()
        {
            bias = this.randomBias();
            idno = 0;
            OActivation = 0.0;
        }

        /// <summary>Creates an output neuron with the given zero-based index.</summary>
        public ONeuron(int idnodata)
        {
            bias = 0.01;
            idno = idnodata;
            OActivation = 0.0;
        }

        /// <summary>Seeds the shared RNG used for bias initialisation.</summary>
        public static void SeedRandom(int seed)
        {
            rand = new System.Random(seed);
        }

        public void setIdno(int data)
        {
            idno = data;
        }

        public int getIdno()
        {
            return idno;
        }

        public void setOActivation(double data)
        {
            OActivation = data;
        }

        /// <summary>The network's current raw output (already in [0,1]).</summary>
        public double getOActivation()
        {
            return OActivation;
        }

        public double getBias()
        {
            return bias;
        }

        public void setBias(double dat)
        {
            bias = dat;
        }

        /// <summary>
        /// Back-propagation update for the output neuron's bias:
        ///   bias <- bias + learningRate * outputDelta
        /// </summary>
        /// <param name="lrpout">Learning rate for the output layer.</param>
        /// <param name="der">Delta of every output neuron.</param>
        public void changeBias(double lrpout, double[] der)
        {
            bias += lrpout * der[idno];
        }

        private double randomBias()
        {
            if (rand == null)
            {
                rand = new System.Random();
            }
            return rand.Next(-1000, 1000) / 2000.0;
        }
    }
}
