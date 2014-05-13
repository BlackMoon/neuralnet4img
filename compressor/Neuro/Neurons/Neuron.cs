using System;

namespace compressor.Neuro
{
    public abstract class Neuron
    {
        /// <summary>
        /// Neuron's inputs count
        /// </summary>
        protected int inputsCount = 0;

        protected double output = 0;

        /// <summary>
        /// Neuron's wieghts
        /// </summary>
        protected double[] weights = null;

        /// <summary>
        /// Random number generator
        /// </summary>
        /// 
        /// <remarks>The generator is used for neuron's weights randomization</remarks>
        /// 
        private static Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);

        /// <summary>
        /// Random generator range
        /// </summary>
        /// 
        /// <remarks>Sets the range of random generator. Affects initial values of neuron's weight.
        /// Default value is [0, 1].</remarks>
        /// 
        protected static Range<Double> randRange = new Range<Double>(0.0, 1.0);

        public static Range<Double> RandRange
        {
            get { return randRange; }
            set
            {
                if (value != null)
                {
                    randRange = value;
                }
            }
        }


        public int InputsCount
        {
            get { return inputsCount; }
        }

        public double Output
        {
            get { return output; }
        }

        public double this[int index]
        {
            get { return weights[index]; }
            set { weights[index] = value; }
        }

        protected Neuron(int inputsCount)
        {
            this.inputsCount = Math.Max(1, inputsCount);
            weights = new double[inputsCount];

            Randomize();
        }

        public virtual void Randomize( )
		{
			double d = RandRange.Max - RandRange.Min;
            // randomize weights
            for (int i = 0; i < inputsCount; i++)
                weights[i] = rand.NextDouble() * d + RandRange.Min;
		}

        public abstract double Compute(double[] input);        
    }
}
