
namespace compressor.Kohonen
{
    class Network
    {
        public Som SomA { get; set; }
        public Som SomR { get; set; }
        public Som SomG { get; set; }
        public Som SomB { get; set; }

        public Network(int nx, int ny, int dim)
        {
            SomA = new Som(nx, ny, dim);
            SomR = new Som(nx, ny, dim);
            SomG = new Som(nx, ny, dim);
            SomB = new Som(nx, ny, dim);
        }
    }
}
