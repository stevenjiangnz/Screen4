using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.Indicator
{
    public class MACD
    {
        public static Result Calculate(double[] inputData, int periodSlow, int periodFast, int periodSignal, double?[] outMACD, double?[] outSignal, double?[] outHist)
        {
            Result result = new Result();
            result.Status = ResultStatus.Success;

            int len = inputData.Length;

            double?[] emaSlow = new double?[len];
            double?[] emaFast = new double?[len];

            EMA.Calculate(inputData, periodSlow, 2.0, emaSlow);
            EMA.Calculate(inputData, periodFast, 2.0, emaFast);

            for (int i = periodSlow - 1; i < len; i++)
            {
                if (emaFast[i].HasValue && emaSlow[i].HasValue)
                {
                    outMACD[i] = emaFast[i].Value - emaSlow[i].Value;
                }
            }

            int tmpSLength = len - periodSlow + 1;
            double[] sInput = new double[tmpSLength];

            for (int j = 0; j < tmpSLength; j++)
            {
                sInput[j] = outMACD[periodSlow - 1 + j].Value;
            }

            double?[] tmpOutS = new double?[tmpSLength];

            EMA.Calculate(sInput, periodSignal, 2.0, tmpOutS);

            for (int k = periodSignal - 1; k < tmpSLength; k++)
            {
                outSignal[k + periodSlow - 1] = tmpOutS[k].Value;
            }

            for (int m = periodSlow + periodSignal - 2; m < len; m++)
            {
                if(outMACD[m].HasValue && outSignal[m].HasValue)
                outHist[m] = outMACD[m].Value - outSignal[m].Value;
            }


            for (int i =0; i< len; i++) {
                if (outHist[i].HasValue) {
                    outHist[i] = Math.Round(outHist[i].Value, 4);
                }

                if (outSignal[i].HasValue) {
                    outSignal[i] = Math.Round(outSignal[i].Value, 4);
                }

                if (outMACD[i].HasValue) {
                    outMACD[i] = Math.Round(outMACD[i].Value, 4);
                }
            }
            return result;
        }
    }
}