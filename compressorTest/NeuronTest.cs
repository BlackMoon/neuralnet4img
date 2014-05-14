using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using compressor.Kohonen;

namespace compressorTest
{
    [TestClass]
    public class NeuronTest
    {
        [TestMethod]
        public void TestDeviation()
        {
            int S = 3;
            int DIM = S * S;

            double[] v = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Neuron n = new Neuron(DIM);

            double expected = 285; // = v! factorial
            Assert.AreEqual(n.Deviation(v), expected);
        }

        [TestMethod]
        public void TestTrain()
        {
            int S = 3;
            int DIM = S * S;

            double[] v = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            
            Neuron n = new Neuron(DIM);

            double[] expected = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            CollectionAssert.AreEqual(n.Train(1, v), expected);

            double[] expected1 = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            for (int i = 0; i < DIM; i++)
            {
                Assert.AreEqual(n[i], expected1[i]);
            }
        }

    }
}
