using System;
using compressor.Kohonen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace compressorTest
{
    [TestClass]
    public class NetworkTest
    {
        [TestMethod]
        public void NetworkConstructorTest()
        {
            int NS = 3;
            int NX = 10;
            int NY = 10;

            int DIM = NS * NS;

            Network net = new Network(NX, NY, DIM);
            Assert.AreEqual(net.SomA.Size, NX * NY);
            Assert.AreEqual(net.SomR.Size, NX * NY);
            Assert.AreEqual(net.SomG.Size, NX * NY);
            Assert.AreEqual(net.SomB.Size, NX * NY);
        }
    }
}
