using System;

namespace compressor.Neuro
{
    public class DistanceNeuron : Neuron
    {
        public DistanceNeuron(int input) : base(input)
        {
        }

        public override double Compute(double[] input)
        {
            output = 0.0;

            // compute distance between inputs and weights
            for (int i = 0; i < inputsCount; i++)
            {
                output += Math.Abs(weights[i] - input[i]);
            }
            return output;
        }

    }
}
