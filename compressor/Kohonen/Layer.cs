using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace compressor.Kohonen
{
    class Layer
    {
        private Neuron[] neurons;

        public Neuron this[int index]
        {
            get { return neurons[index]; }
        }
    }
}
