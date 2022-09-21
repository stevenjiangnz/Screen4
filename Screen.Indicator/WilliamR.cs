using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.Indicator
{
    public class WilliamR
    {
        public static Result Calculate(double[] inputData, double[] inputHigh, double[] inputLow, int period, double?[] outData)
        {
            Result res = new Result();
            res.Status = ResultStatus.Success;

            int len = inputData.Length;
            try
            {
                for (int i = period - 1; i < len; i++)
                {
                    double[] tmpHigh = new double[period];
                    double[] tmpLow = new double[period];

                    for (int j = 0; j < period; j++)
                    {
                        int bIndex = i - period + 1;

                        tmpHigh[j] = inputHigh[bIndex + j];
                        tmpLow[j] = inputLow[bIndex + j];
                    }

                    outData[i] = Math.Round(GetWR(tmpHigh, tmpLow, inputData[i]), 4);
                }

            }
            catch(Exception ex)
            {
                res.Status = ResultStatus.Fail;
                res.Message = ex.ToString();
            }

            return res;
        }

        private static double GetWR(double[] inHigh, double[] inLow, double close)
        {
            double wr = 0;

            double highest = inHigh.Max();
            double lowest = inLow.Min();

            if (highest != lowest)
            {
                wr = ((highest - close) / (highest - lowest)) * (-100);
            }
            else
            {
                wr = -100;
            }

            return wr;
        }
    }

}