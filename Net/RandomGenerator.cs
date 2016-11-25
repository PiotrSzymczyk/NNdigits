using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Net
{
    public static class RandomGenerator
    {
        private static Random generator = new Random();

        public static double NextDouble()
        {
            lock (generator)
            {
                return generator.NextDouble();
            }
        }

        public static int Next(int max)
        {
            lock (generator)
            {
                return generator.Next(max);
            }
        }
    }
}
