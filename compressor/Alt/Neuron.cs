using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace compressor.Alt
{
    class Neuron
    {
        public int dim;
        public double [] w;

        public Neuron(int dim)
        {
            this.dim = dim;
            w = new double[dim];
        }

        public double Deviation(double [] v) 
        {   
            double sum = 0;
            for (int i = 0; i <  v.Length; i++) {
                sum += (v[i] - w[i]) * (v[i] - w[i]);
            }
            return sum;
        }

        public double[] Learn(double c, double[] v)
        {
            double [] res = new double[dim];
    
            for (int i = 0; i < dim; i++) 
            {
                w[i] += Math.Round(c * (v[i] - this.w[i]));
                res[i] = w[i];                
            }
            return res; 
        }
    }
}
