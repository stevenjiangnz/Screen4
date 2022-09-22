using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.Indicator
{
    public class SMA 
    {
  
        public static Result Calculate(double[] inputData, int period, double?[] outData)
        {
            Result result = new Result();
            result.Status = ResultStatus.Success;

            int len = inputData.Length;

            try
            {
                for (int i = period - 1; i < len; i++)
                {
                    double[] periodData = new double[period];
                    int b = i - (period - 1);

                    for (int k = 0; k < period; k++)
                    {
                        periodData[k] = inputData[b + k];
                    }

                    outData[i] = Math.Round(GenericHelper.GetAvg(periodData), 4);
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