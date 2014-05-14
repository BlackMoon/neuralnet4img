using System;

namespace compressor.Kohonen
{
    class Network
    {
        private int inputsCount = 0;
        private int layersCount;

        private Layer[] layers;


        public int NX { get; set; }
        public int NY { get; set; }
        public int DIM { get; set; }

        public Som SomA { get; set; }
        public Som SomR { get; set; }
        public Som SomG { get; set; }
        public Som SomB { get; set; }

        public Network(int nx, int ny, int dim)
        {
            NX = nx;
            NY = ny;
            DIM = dim;

            SomA = new Som(NX, NY, DIM);
            SomR = new Som(NX, NY, DIM);
            SomG = new Som(NX, NY, DIM);
            SomB = new Som(NX, NY, DIM);
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
