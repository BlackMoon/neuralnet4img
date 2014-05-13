using System;

namespace compressor.Neuro
{
    public class Range<T> where T: IComparable
    {
        private T min;
        private T max;

        public T Max
        {
            get { return max; }
        }

        public T Min
        {
            get { return min; }
        }

        public Range(T min, T max)
		{
			this.min = min;
			this.max = max;
		}

        public bool IsInside(T x)
        {   
            return (x.CompareTo(min) >= 0 && x.CompareTo(max) <= 0);
        }
    }
}
