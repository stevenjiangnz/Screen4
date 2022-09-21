using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.Indicator
{
    public class RSI
    {
        public static Result Calculate(double[] close, int period, double?[] outRSI)
        {
            Result res = new Result();
            res.Status = ResultStatus.Success;

            int len = close.Length;
            double?[] avgGain = new double?[len];
            double?[] avgLoss = new double?[len];

            double[] change = new double[len];
            double?[] rs = new double?[len];

            try
            {
                // calculate change between yesterday and today
                for (int j = 1; j < len; j++)
                {
                    change[j] = close[j] - close[j - 1];
                }

                // Calculate the first day
                double[] gain = new double[period];
                double[] loss = new double[period];

                for (int j = 1; j <= period; j++)
                {
                    gain[j-1] = (change[j] > 0) ? change[j] : 0;
                    loss[j-1] = (change[j] < 0) ? change[j] * (-1) : 0;
                }

                // Calculate the first rsi
                avgGain[period] = GenericHelper.GetAvg(gain);
                avgLoss[period] = GenericHelper.GetAvg(loss);

                if (avgGain[period].HasValue && avgLoss[period].HasValue)
                {
                    if (avgLoss[period].Value == 0)
                    {
                        outRSI[period] = 100;
                    }
                    else {
                        rs[period] = avgGain[period].Value / avgLoss[period].Value;
                        outRSI[period] = 100 - 100.0 / (1 + rs[period].Value);
                    }
                }

                // Calculate the rest RSIs
                for (int i = period +1; i < len; i++)
                {
                    var currentGain = change[i] > 0 ? change[i] : 0;
                    var currentLoss = change[i] < 0 ? change[i] * (-1) : 0;

                    avgGain[i] = (currentGain + avgGain[i - 1] * (period -1)) / period;
                    avgLoss[i] = (currentLoss + avgLoss[i - 1] * (period - 1)) / period;

                    if (avgGain[i].HasValue && avgLoss[i].HasValue)
                    {
                        if (avgLoss[i].Value == 0)
                        {
                            outRSI[i] = 100;
                        }
                        else {
                            rs[i] = avgGain[i].Value / avgLoss[i].Value;
                            outRSI[i] = Math.Round(100 - 100.0 / (1 + rs[i].Value), 4);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                res.Status = ResultStatus.Fail;
                res.Message = ex.ToString();
            }

            return res;
        }
    }
}