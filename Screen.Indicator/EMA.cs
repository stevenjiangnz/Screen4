using System;

namespace Screen.Indicator
{
    public class EMA 
    {
        public static Result Calculate(double[] inputData, int period, double mfactor, double?[] outData)
        {
            Result result = new Result();
            result.Status = ResultStatus.Success;

            int len = inputData.Length;

            double?[] sma = new double?[len];
            try
            {
                var resultSMA = SMA.Calculate(inputData, period, sma);

                double multiplier = mfactor / (period + 1);

                outData[period - 1] = sma[period - 1].Value;

                for (int i = period; i < len; i++)
                {
                    double emaPre = outData[i - 1].Value;
                    outData[i] = Math.Round(((inputData[i] - emaPre) * multiplier + emaPre), 4);
                }
            }
            catch(Exception ex)
            {
                result.Status = ResultStatus.Fail;
                result.Message = ex.ToString();
            }

            return result;
        }

    }
}