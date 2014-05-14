using System;

namespace compressor.Kohonen
{
    /**
     * Самоорганизующаяся карта Кохонена (SOM)
     **/

    public class Som
    {
        private int nx;                         // ширина карты
        private int ny;                         // высота карты
        private int dim;                        // размер нейрона        
        private int t = 0;                      // текущая итерация обучения 

        private Neuron[,] map;

        public int Size { get; set; }           // размер карты

        public Neuron this[int y, int x]
        {
            get { return map[y, x]; }
        }

        public Som(int nx, int ny, int dim)
        {
            this.nx = nx;
            this.ny = ny;
            this.dim = dim;
            
            Size = this.nx * this.ny;
            
            this.map = new Neuron[ny, nx];

            for (int j = 0; j < ny; j++) 
            {
                for (int i = 0; i < nx; i++)
                {
                    map[j, i] = new Neuron(dim);
                }
            }
        }

        /// <summary>
        /// Поиск наиболее близких координат координат
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private int[] FindBmu(double[] v)
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

        /// <summary>
        /// Поиск наболее близкого нейрона
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Neuron GetWinner(double[] v)
        {
            int[] pt = this.FindBmu(v);
            Neuron n = map[pt[1], pt[0]];
            return n;
        }

        /// <summary>
        /// Обучение карты Кохонена
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public double Train(double [] input)
        {
            // ближайшие координаты
            int [] pt = this.FindBmu(input);                             
            this.t++;

            double sigma = 2 * Math.Sqrt(Size / this.t);
            int r = (int)Math.Round(sigma * 2);
            double alpha = this.t < 10 ? 1 : 1 / Math.Pow(this.t - 9, 0.2);

            double d, c;

            for (int y = Math.Max(0, pt[1] - r); y < Math.Min(this.ny, pt[1] + r); y++)
            {
                for (int x = Math.Max(0, pt[0] - r); x < Math.Min(this.nx, pt[0] + r); x++)
                {
                    d = this.Ndist(pt, new int[2]{x, y});
                    c = alpha * Math.Exp(-(d * d) / (sigma * sigma));

                    Neuron n = this.map[y, x];
                    n.Train(c, input); 
                }
            }
            
            return this.map[pt[1], pt[0]].Deviation(input); 
        }

        /// <summary>
        /// расстояние между 2мя точками двумерной карты карты
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="n2"></param>
        /// <returns></returns>
        public double Ndist(int [] n1, int [] n2)
        {
            return Math.Max(Math.Abs(n1[0] - n2[0]), Math.Abs(n1[1] - n2[1])); 
        }
    }
}
