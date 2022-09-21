using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.Indicator
{
    public class BollingerBand
    {
        public static Result Calculate(double[] inputData, int period, double factor, double?[] outMiddle, double?[] outHigh, double?[] outLow)
        {
            Result res = new Result();
            res.Status = ResultStatus.Success;

            int len = inputData.Length;

            double[] sd = new double[len];

            try
            {
                SMA.Calculate(inputData, period, outMiddle);

                for (int i = period - 1; i < len; i++)
                {
                    double[] d = new double[period];

                    for (int j = 0; j < period; j++)
                    {
                        int bIndex = i + 1 - period;

                        d[j] = inputData[bIndex + j];
                    }

                    sd[i] = Math.Round(GenericHelper.StandardDeviation(d), 4);

                    outHigh[i] = Math.Round((outMiddle[i] + factor * sd[i]).Value, 4);

                    outLow[i] = Math.Round((outMiddle[i] - factor * sd[i]).Value, 4);
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
    }
}