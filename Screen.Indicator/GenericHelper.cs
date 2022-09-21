using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.Indicator
{
public class GenericHelper
    {
        public static double GetAvg(double[] input)
        {
            double avg = 0;

            if (input != null && input.Length > 0)
            {
                int len = input.Length;
                double sum = 0;

                for (int i = 0; i < len; i++)
                {
                    sum += input[i];
                }

                avg = sum / len;
            }

            return avg;
        }

        public static void GetAccumulate(double?[] input, int offset, int x, double?[] accResult)
        {

            for (int i = offset + x; i <input.Length; i++)
            {
                accResult[i] = 0;
                for (int j =0; j< x; j++)
                {
                    if (input[i - j].HasValue)
                        accResult[i] += input[i - j].Value;
                }
            }

        }


        public static double StandardDeviation(double[] valueArray)
        {
            double M = 0.0;
            double S = 0.0;
            int k = 1;

            for (int i = 0; i < valueArray.Length; i++)
            {
                double value = valueArray[i];
                double tmpM = M;
                M += (value - tmpM) / k;
                S += (value - tmpM) * (value - M);
                k++;

            }

            return Math.Sqrt(S / (k - 1));
        }
    }
}
