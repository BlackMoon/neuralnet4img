
using System;
namespace compressor.Neuro
{
    public class Network
    {
		/// <summary>
		/// Network's inputs count
		/// </summary>
		private int	inputsCount;

		/// <summary>
		/// Network's layers count
		/// </summary>
		private int	layersCount;

        /// <summary>
        /// Network's layers
        /// </summary>
        private Layer[] layers;

        /// <summary>
        /// Network's output vector
        /// </summary>
        private double[] output = null;

        /// <summary>
        /// Network's inputs count
        /// </summary>
        public int InputsCount
        {
            get { return inputsCount; }
        }

        /// <summary>
        /// Network's layers count
        /// </summary>
        public int LayersCount
        {
            get { return layersCount; }
        }

        /// <summary>
        /// Network's output vector
        /// </summary>
        /// 
        /// <remarks>The calculation way of network's output vector is determined by
        /// inherited class.</remarks>
        /// 
        public double[] Output
        {
            get { return output; }
        }

        /// <summary>
        /// Network's layers accessor
        /// </summary>
        /// 
        /// <param name="index">Layer index</param>
        /// 
        /// <remarks>Allows to access network's layer.</remarks>
        /// 
        public Layer this[int index]
        {
            get { return layers[index]; }
        }

        public Network(int inputsCount, int layersCount)
        {
            this.inputsCount = Math.Max(1, inputsCount);
            this.layersCount = Math.Max(1, layersCount);
            // create collection of layers
            layers = new Layer[this.layersCount];
        }


    }
}
