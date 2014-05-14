using System;

namespace compressor.Kohonen
{
    class Network
    {
        private bool trained = false;       

        public bool Trained 
        {
            get
            {
                return trained;
            }
            set
            {
                trained = value;
            }
        }

        public int NX { get; set; }
        public int NY { get; set; }
        public int DIM { get; set; }

        public Som SomA { get; set; }
        public Som SomR { get; set; }
        public Som SomG { get; set; }
        public Som SomB { get; set; }       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nx">ширина сетки</param>
        /// <param name="ny">высота сетки</param>
        /// <param name="dim">размер нейрона в кв.</param>
        public Network(int nx, int ny, int dim)
        {
            NX = nx;
            NY = ny;
            DIM = dim;

            // 4 бита цвета - 4 карты Кохонена
            SomA = new Som(NX, NY, DIM);
            SomR = new Som(NX, NY, DIM);
            SomG = new Som(NX, NY, DIM);
            SomB = new Som(NX, NY, DIM);
        }      
    }
}
