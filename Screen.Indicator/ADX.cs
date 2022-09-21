using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.Indicator
{
    public class ADX
    {
        public static Result Calculate(double[] high, double[] low, double[] close, 
            double?[] di14plus, double?[] di14minus, double?[] adx, int period = 14)
        {
            Result result = new Result();
            result.Status = ResultStatus.Success;

            int len = high.Length;

            try
            {
                double?[] tr1 = new double?[len];
                double?[] dm1plus = new double?[len];
                double?[] dm1minus = new double?[len];
                for (int i = 1; i < len; i++)
                {
                    double dayDiff = high[i] - low[i];
                    double yesDiff1 = Math.Abs(high[i] - close[i - 1]);
                    double yesDiff2 = Math.Abs(low[i] - close[i - 1]);

                    double yesDiff = yesDiff1 > yesDiff2 ? yesDiff1 : yesDiff2;
                    tr1[i] = dayDiff > yesDiff ? dayDiff : yesDiff;

                    dm1plus[i] = (high[i] - high[i - 1]) > (low[i - 1] - low[i]) ? ((high[i] - high[i - 1]) > 0 ? (high[i] - high[i - 1]) : 0) : 0;
                    dm1minus[i] = (low[i - 1] - low[i]) > (high[i] - high[i - 1]) ? ((low[i - 1] - low[i]) > 0 ? (low[i - 1] - low[i]) : 0) : 0;
                }

                double?[] tr14 = new double?[len];
                double?[] dm14plus = new double?[len];
                double?[] dm14minus = new double?[len];
                double?[] di14diff = new double?[len];
                double?[] di14sum = new double?[len];
                double?[] dx = new double?[len];


                tr14[period] = 0;
                dm14plus[period] = 0;
                dm14minus[period] = 0;
                for (int i = 1; i <= period; i++)
                {
                    tr14[period] += tr1[i].Value;
                    dm14plus[period] += dm1plus[i].Value;
                    dm14minus[period] += dm1minus[i].Value;
                }

                di14plus[period] = 100 * (dm14plus[period].Value / tr14[period].Value);
                if (double.IsNaN(di14plus[period].Value))
                    di14plus[period] = null;

                di14minus[period] = 100 * (dm14minus[period].Value / tr14[period].Value);
                if (double.IsNaN(di14minus[period].Value))
                    di14minus[period] = null;

                di14diff[period] = Math.Abs(di14plus[period].Value - di14minus[period].Value);
                di14sum[period] = di14plus[period].Value + di14minus[period].Value;
                dx[period] = 100 * (di14diff[period].Value / di14sum[period].Value);
                if (double.IsNaN(dx[period].Value))
                    dx[period] = null;

                for (int i = period + 1; i < len; i++)
                {
                    tr14[i] = tr14[i - 1].Value - (tr14[i - 1].Value / period) + tr1[i].Value;
                    dm14plus[i] = dm14plus[i - 1].Value - (dm14plus[i - 1].Value / period) + dm1plus[i].Value;
                    dm14minus[i] = dm14minus[i - 1].Value - (dm14minus[i - 1].Value / period) + dm1minus[i].Value;
                    di14plus[i] = 100 * (dm14plus[i].Value / tr14[i].Value);
                    if (double.IsNaN(di14plus[i].Value))
                        di14plus[i] = null;

                    di14minus[i] = 100 * (dm14minus[i].Value / tr14[i].Value);
                    if (double.IsNaN(di14minus[i].Value))
                        di14minus[i] = null;

                    di14diff[i] = Math.Abs(di14plus[i].Value - di14minus[i].Value);
                    di14sum[i] = di14plus[i].Value + di14minus[i].Value;
                    dx[i] = 100 * (di14diff[i].Value / di14sum[i].Value);
                    if (double.IsNaN(dx[i].Value))
                        dx[i] = null;
                }

                adx[2 * period - 1] = 0;
                for (int i = period; i < 2 * period; i++)
                {
                    adx[2 * period - 1] += dx[i].Value;
                }
                adx[2 * period - 1] = adx[2 * period - 1] / period;

                for (int i = 2 * period; i < len; i++)
                {
                    adx[i] = (adx[i - 1] * (period - 1) + dx[i]) / period;
                }

                for (int i =0; i< len; i++) {
                    if (adx[i].HasValue) {
                        adx[i] = Math.Round(adx[i].Value, 4);
                    }

                    if (di14plus[i].HasValue) {
                        di14plus[i] = Math.Round(di14plus[i].Value, 4);
                    }

                    if (di14minus[i].HasValue) {
                        di14minus[i] = Math.Round(di14minus[i].Value, 4);
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