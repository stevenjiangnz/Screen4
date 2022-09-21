using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Screen.Utils;

namespace Screen.Indicator
{
    public class ATR
    {
        public static Result Calculate(double[] high, double[] low, double[] close,
            int period, double?[] outATR)
        {
            Result result = new Result();
            result.Status = ResultStatus.Success;

            int len = high.Length;
            double?[] hl = new double?[len];
            double?[] h_pc = new double?[len];
            double?[] l_pc = new double?[len];
            double[] tr = new double[len];

            try
            {
                for (int i = 0; i < len; i++)
                {
                    if (i == 0)
                    {
                        h_pc[i] = 0;
                        l_pc[i] = 0;
                    }
                    else
                    {
                        h_pc[i] = Math.Abs(close[i - 1] - high[i]);
                        l_pc[i] = Math.Abs(close[i - 1] - low[i]);
                    }

                    hl[i] = Math.Abs(high[i] - low[i]);

                    tr[i] = Math.Max(hl[i].Value, h_pc[i].Value);
                    tr[i] = Math.Max(tr[i], l_pc[i].Value);
                }

                for (int i = period -1; i< len; i++) {
                    if( i == period -1) {
                        outATR[i] = tr.Take(period).Average();
                    } else {
                        outATR[i] = Math.Round(((outATR[i-1] * (period -1) + tr[i]) / period).Value, 4); 
                    }

                }
            }
            catch (Exception ex)
            {
                result.Status = ResultStatus.Fail;
                result.Message = ex.ToString();
            }
            return result;
        }
    }
}