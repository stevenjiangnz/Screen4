using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.Indicator
{
    public class SMA 
    {
        // public override Result Calculate(SMAIn input, SMASetting setting)
        // {
        //     Result res = new Result();

        //     int len = input.Data.Length;

        //     if(len< setting.Period)
        //     {
        //         res.Status = ResultStatus.Fail;
        //         res.Message = "input data error";
        //         return res;
        //     }

        //     try
        //     {
        //         double[] inputData = new double[len];
        //         double?[] outData = new double?[len];

        //         for(int i =0; i< len; i++)
        //         {
        //             inputData[i] = input.Data[i].iClose;
        //         }

        //         res = Calculate(inputData, setting.Period, outData);
        //         for (int i = 0; i < len; i++)
        //         {
        //             input.Data[i].oSMA = outData[i];
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         res.Status = ResultStatus.Fail;
        //         res.Message = ex.ToString();
        //         return res;
        //     }

        //     res.Status = ResultStatus.Success;
        //     return res;
        // }



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

    public class SMAIn
    {
        public SMAItem[] Data { get; set; }
    }


    public class SMASetting {
        public int Period { get; set; }
        // public int Offset { get; set; }
    }

    public class SMAItem
    {
        public int TradingDate { get; set; }
        public double iClose { get; set; }
        public double? oSMA { get; set; }
    }
}