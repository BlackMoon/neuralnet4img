
using System;
namespace compressor.Neuro
{
    /// <summary>
    /// Base neural layer class
    /// </summary>
    /// 
    /// <remarks>This is a base neural layer class, which represents
    /// collection of neurons.</remarks>
    /// 

    public abstract class Layer
    {
        /// <summary>
        /// Layer's inputs count
        /// </summary>
        protected int inputsCount = 0;

        /// <summary>
        /// Layer's neurons count
        /// </summary>
        protected int neuronsCount = 0;

        /// <summary>
        /// Layer's neurons
        /// </summary>
        protected Neuron[] neurons;

        /// <summary>
        /// Layer's output vector
        /// </summary>
        private double[] output;

        /// <summary>
        /// Layer's inputs count
        /// </summary>
        public int InputsCount
        {
            get { return inputsCount; }
        }

        /// <summary>
        /// Layer's neurons count
        /// </summary>
        public int NeuronsCount
        {
            get { return neuronsCount; }
        }

        public double[] Output
        {
            get { return output; }
        }

        public Neuron this[int index]
        {
            get { return neurons[index]; }
        }

        public virtual void Randomize()
        {
            foreach (Neuron neuron in neurons)
                neuron.Randomize();
        }

        protected Layer(int neuronsCount, int inputsCount)
        {
            this.inputsCount = Math.Max(1, inputsCount);
            this.neuronsCount = Math.Max(1, neuronsCount);
            // create collection of neurons
            neurons = new Neuron[this.neuronsCount];
            // allocate output array
            output = new double[this.neuronsCount];
        }

        public virtual double[] Compute(double[] input)
        {
            // compute each neuron
            for (int i = 0; i < neuronsCount; i++)
                output[i] = neurons[i].Compute(input);

            return output;
        }
    }
}
