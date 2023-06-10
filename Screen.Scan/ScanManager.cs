using Screen.Entity;

namespace Screen.Scan
{
    public class ScanManager
    {
        public IList<ScanResultEntity> ProcessScan(IList<IndicatorEntity> indicators, int? dateToProcess = null,
            bool recursive = false)
        {
            IList<ScanResultEntity> resultList = new List<ScanResultEntity>();

            List<IndicatorEntity> orderedIndicators = indicators.OrderByDescending(i => i.Period).ToList();

            double?[] macdArray = orderedIndicators.Select(i => i.MACD).ToArray();
            double?[] macdSignalArray = orderedIndicators.Select(i => i.MACD_Signal).ToArray();
            double?[] macdHistArray = orderedIndicators.Select(i => i.MACD_Hist).ToArray();
            double?[] adxArray = orderedIndicators.Select(i => i.ADX).ToArray();
            double?[] diPlusArray = orderedIndicators.Select(i => i.DIPlus).ToArray();
            double?[] diMinusArray = orderedIndicators.Select(i => i.DIMinus).ToArray();

            int[] periodArray = orderedIndicators.Select(i => i.Period).ToArray();

            for (int i = 0; i < periodArray.Length; i++)
            {
                var isReverseBull = this.Check_MACD_REVERSE_BULL(i, macdArray, macdSignalArray, macdHistArray);
               
                var isAdxBull = this.Check_ADX_INTO_BULL(i, adxArray, diPlusArray, diMinusArray);
                var scanResult = new ScanResultEntity()
                {
                    Symbol = orderedIndicators[0].Code,
                    TradingDate = periodArray[i],
                    MACD_REVERSE_BULL = isReverseBull,
                    ADX_INTO_BULL = isAdxBull
                };

                resultList.Add(scanResult);
            }

            return resultList;
        }

        public bool? Check_MACD_REVERSE_BULL(int index, double?[] macdArray, double?[] macdSignalArray,
            double?[] macdHistArray)
        {
            bool? result = null;
            double Threshold = -0.02;

            try
            {
                if (macdArray[index] > Threshold || macdSignalArray[index] > Threshold)
                {
                    if (macdHistArray[index + 1] <= 0)
                    {
                        if (macdHistArray[index + 2] >= macdHistArray[index + 1] &&
                            macdHistArray[index + 1] <= macdHistArray[index])
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        if (macdHistArray[index + 3] >= macdHistArray[index + 2] &&
                            macdHistArray[index + 2] >= macdHistArray[index + 1] &&
                            macdHistArray[index + 1] <= macdHistArray[index])
                        {
                            result = true;
                        }
                    }
                }

                if (!result.HasValue)
                {
                    result = false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in process Check_MACD_REVERSE_BULL" + ex.ToString());
            }
            return result;
        }

        public bool? Check_ADX_INTO_BULL(int index, double?[] adxArray, double?[] diPlusArray,
            double?[] diMinusArray)
        {
            bool? result = null;

            try
            {
                if (diPlusArray[index] > diMinusArray[index] &&
                    diMinusArray[index] > adxArray[index] &&
                    (diMinusArray[index]-adxArray[index]) < 8 &&
                    adxArray[index] > adxArray[index+1] &&
                    Math.Abs(diMinusArray[index].Value - adxArray[index].Value)<
                    Math.Abs(diMinusArray[index +1].Value - adxArray[index+1].Value) )
                {
                    result = true;
                }

                if (!result.HasValue)
                {
                    result = false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in process Check_ADX_INTO_BULL" + ex.ToString());
            }
            return result;

        }

    }
}