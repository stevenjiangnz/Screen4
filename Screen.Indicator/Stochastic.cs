using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Screen.Utils;

namespace Screen.Indicator
{
    public class Stochastic
    {
        public static Result CalculateSlow(double[] inputClose, double[] inputHigh, double[] inputLow, int period, int slow, double?[] outDataK, double?[] outDataD)
        {
            Result res = new Result();
            res.Status = ResultStatus.Success;

            int len = inputClose.Length;
            double[] tmpK = new double[len];
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
                    tmpK[i] = GetBasicK(tmpHigh, tmpLow, inputClose[i]);
                }

                SMA.Calculate(tmpK, slow, outDataK);

                double[] outDataK2 = new double[len];

                for (int k = 0; k < len; k++)
                {
                    if (outDataK[k].HasValue)
                    {
                        outDataK2[k] = outDataK[k].Value;
                    }
                }

                SMA.Calculate(outDataK2, slow, outDataD);

                for(int j=0; j< period; j++)
                {
                    outDataK[j] = null;
                    outDataD[j] = null;
                }


                for (int i =0; i< len; i++) {
                    if (outDataD[i].HasValue) {
                        outDataD[i] = Math.Round(outDataD[i].Value, 4);
                    }

                    if (outDataK[i].HasValue) {
                        outDataK[i] = Math.Round(outDataK[i].Value, 4);
                    }
                }
            }
            catch (Exception ex)
            {
                res.Status = ResultStatus.Fail;
                res.Message = ex.ToString();
                return res;
            }
            return res;
        }

        private static double GetBasicK(double[] inHigh, double[] inLow, double close)
        {
            double k = 0;

            double highest = inHigh.Max();
            double lowest = inLow.Min();

            if (highest != lowest)
            {
                k = ((close - lowest) / (highest - lowest)) * (100);
            }
            else
            {
                k = 100;
            }

            Console.WriteLine($"high: {highest} low: {lowest} close: {close} k: {k}");

            return k;
        }
    }

}