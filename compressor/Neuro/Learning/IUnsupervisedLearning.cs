
namespace compressor.Neuro
{
    interface IUnsupervisedLearning
    {
        /// <summary>
        /// Runs learning iteration
        /// </summary>
        /// 
        /// <param name="input">input vector</param>
        /// 
        /// <returns>Returns learning error</returns>
        /// 
        double Run(double[] input);

        /// <summary>
        /// Runs learning epoch
        /// </summary>
        /// 
        /// <param name="input">array of input vectors</param>
        ///
        /// <returns>Returns sum of learning errors</returns>
        /// 
        double RunEpoch(double[][] input);
    }
}
