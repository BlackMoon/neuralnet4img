using System;

namespace compressor.Neuro
{
    public class Neuron
    {
        /// <summary>
        /// Neuron's inputs count
        /// </summary>
        private int input = 0;

        private int output = 0;

        /// <summary>
        /// Neuron's wieghts
        /// </summary>
        private double[] weights = null;

        public int Input
        {
            get { return input; }
        }

        public int Output
        {
            get { return output; }
        }

        public double this[int index]
        {
            get { return weights[index]; }
            set { weights[index] = value; }
        }

        public Neuron(int input)
        {
            this.input = Math.Max(1, input);
            weights = new double[input];            
        }

        public double Compute(double[] input)
        {
            return 0;
        }
    }
}
