using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using compressor.Kohonen;

namespace compressorTest
{
    [TestClass]
    public class SomTest
    {
        [TestMethod]
        public void SomConstructorTest()
        {
            int NS = 3;
            int NX = 10;
            int NY = 10;

            int DIM = NS * NS;
            Som som = new Som(NX, NY, DIM);

            int sz = NX * NY;
            Assert.AreEqual(som.Size, sz);
            
            Neuron [,] map = new Neuron[NY, NX];
            for (int j = 0; j < NY; j++) 
            {
                for (int i = 0; i < NX; i++)
                {
                    Assert.IsNotNull(som[j, i]);                    
                }
            }
        }
    }
}
