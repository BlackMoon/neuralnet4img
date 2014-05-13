using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace compressor.Kohonen
{
    class Som
    {
        public int nx;
        public int ny;
        public int dim;
        public int size;
        public int t = 0;

        public Neuron[,] map;

        public Neuron this[int y, int x]
        {
            get { return map[y, x]; }
        }

        public Som(int nx, int ny, int dim)
        {
            this.nx = nx;
            this.ny = ny;
            this.dim = dim;
            this.size = this.nx * this.ny;
            
            this.map = new Neuron[ny, nx];

            for (int j = 0; j < ny; j++) 
            {
                for (int i = 0; i < nx; i++)
                {
                    map[j, i] = new Neuron(dim);
                }
            }
        }

        public int[] FindBmu(double[] v)
        {
            double e;
            double emin = 4000000000;
            int mx = 0, my = 0;

            for (int y = 0; y < ny; y++)
            {
                for (int x = 0; x < nx; x++)
                {
                    e = map[y, x].Deviation(v);
                    if (e < emin) 
                    {
                        emin = e;
                        mx = x;
                        my = y;
                    }
                }
            }
            return new int[2]{mx, my};
        }

        public Neuron FindBmu1(double[] v)
        {
            double e;
            double emin = 4000000000;
            Neuron bmu = null;

            for (int y = 0; y < ny; y++)
            {
                for (int x = 0; x < nx; x++)
                {
                    Neuron n = map[y, x];
                    e = n.Deviation(v);
                    if (e < emin)
                    {
                        emin = e;
                        bmu = n;
                    }
                }
            }
            return bmu;
        }

        public double Learn(double [] v)
        {   
            int [] n = this.FindBmu(v);
            this.t++;

            double sigma = 2 * Math.Sqrt(this.size / this.t);
            int r = (int)Math.Round(sigma * 2);
            double alpha = this.t < 10 ? 1 : 1 / Math.Pow(this.t - 9, 0.2);

            double d, c;

            for (int y = Math.Max(0, n[1] - r); y < Math.Min(this.ny, n[1] + r); y++)
            {
                for (int x = Math.Max(0, n[0] - r); x < Math.Min(this.nx, n[0] + r); x++)
                {
                    d = this.Ndist(n, new int[2]{x, y});
                    c = alpha * Math.Exp(-(d * d) / (sigma * sigma));
                    this.map[y, x].Learn(c, v); 
                }
            }
            
            return this.map[n[1], n[0]].Deviation(v); 
        }

        public double Ndist(int [] n1, int [] n2)
        {
            return Math.Max(Math.Abs(n1[0] - n2[0]), Math.Abs(n1[1] - n2[1])); 
        }
    }
}
