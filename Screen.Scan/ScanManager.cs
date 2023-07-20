using CsvHelper;
using Google.Apis.Drive.v3;
using Microsoft.Extensions.Logging;
using Screen.Access;
using Screen.Entity;
using System.Globalization;

namespace Screen.Scan
{
    public class ScanManager
    {
        private readonly ILogger _log;
        public ScanManager(ILogger log)
        {
            this._log = log;
        }

        public IList<ScanResultEntity> ProcessScan(IList<IndicatorEntity> indicators, int? dateToProcess = null)
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
                var isReverseBear = this.Check_MACD_REVERSE_BEAR(i, macdArray, macdSignalArray, macdHistArray);
                var isCrossBull = this.Check_MACD_CROSS_BULL(i, macdArray, macdSignalArray, macdHistArray);
                var isCrossBear = this.Check_MACD_CROSS_BEAR(i, macdArray, macdSignalArray, macdHistArray);
                var isAdxBull = this.Check_ADX_INTO_BULL(i, adxArray, diPlusArray, diMinusArray);
                var isAdxBear = this.Check_ADX_INTO_BEAR(i, adxArray, diPlusArray, diMinusArray);
                var isAdxCrossBull = this.Check_ADX_CROSS_BULL(i, adxArray, diPlusArray, diMinusArray);
                var isAdxCrossBear = this.Check_ADX_CROSS_BEAR(i, adxArray, diPlusArray, diMinusArray);
                var isAdxTrendBull = this.Check_ADX_TREND_BULL(i, adxArray, diPlusArray, diMinusArray);
                var isAdxTrendBear = this.Check_ADX_TREND_BEAR(i, adxArray, diPlusArray, diMinusArray);

                var scanResult = new ScanResultEntity()
                {
                    Symbol = orderedIndicators[0].Code,
                    TradingDate = periodArray[i],
                    MACD_REVERSE_BULL = isReverseBull,
                    MACD_REVERSE_BEAR = isReverseBear,
                    MACD_CROSS_BULL = isCrossBull,
                    MACD_CROSS_BEAR = isCrossBear,
                    ADX_INTO_BULL = isAdxBull,
                    ADX_INTO_BEAR = isAdxBear,
                    ADX_CROSS_BULL = isAdxCrossBull,
                    ADX_CROSS_BEAR = isAdxCrossBear,
                    ADX_TREND_BULL = isAdxTrendBull,
                    ADX_TREND_BEAR = isAdxTrendBear
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
                this._log.LogError("Error in process Check_MACD_REVERSE_BULL" + ex.ToString());
            }
            return result;
        }


        public bool? Check_MACD_REVERSE_BEAR(int index, double?[] macdArray, double?[] macdSignalArray,
    double?[] macdHistArray)
        {
            bool? result = null;
            double Threshold = 0.02;

            try
            {
                if (macdArray[index] < Threshold || macdSignalArray[index] < Threshold)
                {
                    if (macdHistArray[index + 1] >= 0)
                    {
                        if (macdHistArray[index + 2] <= macdHistArray[index + 1] &&
                            macdHistArray[index + 1] >= macdHistArray[index])
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        if (macdHistArray[index + 3] <= macdHistArray[index + 2] &&
                            macdHistArray[index + 2] <= macdHistArray[index + 1] &&
                            macdHistArray[index + 1] >= macdHistArray[index])
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
                this._log.LogError("Error in process Check_MACD_REVERSE_BEAR" + ex.ToString());
            }
            return result;
        }


        public bool? Check_MACD_CROSS_BULL(int index, double?[] macdArray, double?[] macdSignalArray,
    double?[] macdHistArray)
        {
            bool? result = null;

            try
            {
                if (macdArray[index] > macdSignalArray[index] && macdArray[index + 1] < macdSignalArray[index + 1])
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
                this._log.LogError("Error in process Check_MACD_CROSS_BULL" + ex.ToString());
            }
            return result;
        }


        public bool? Check_MACD_CROSS_BEAR(int index, double?[] macdArray, double?[] macdSignalArray,
double?[] macdHistArray)
        {
            bool? result = null;

            try
            {
                if (macdArray[index] < macdSignalArray[index] && macdArray[index + 1] > macdSignalArray[index + 1])
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
                this._log.LogError("Error in process Check_MACD_CROSS_BULL" + ex.ToString());
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
                    (diMinusArray[index] - adxArray[index]) < 8 &&
                    adxArray[index] > adxArray[index + 1] &&
                    Math.Abs(diMinusArray[index].Value - adxArray[index].Value) <
                    Math.Abs(diMinusArray[index + 1].Value - adxArray[index + 1].Value))
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
                this._log.LogError("Error in process Check_ADX_INTO_BULL" + ex.ToString());
            }
            return result;
        }

        public bool? Check_ADX_INTO_BEAR(int index, double?[] adxArray, double?[] diPlusArray,
double?[] diMinusArray)
        {
            bool? result = null;

            try
            {
                if (diPlusArray[index] < diMinusArray[index] &&
                    diPlusArray[index] > adxArray[index] &&
                    (diPlusArray[index] - adxArray[index]) < 8 &&
                    adxArray[index] > adxArray[index + 1] &&
                    Math.Abs(diPlusArray[index].Value - adxArray[index].Value) <
                    Math.Abs(diPlusArray[index + 1].Value - adxArray[index + 1].Value))
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
                this._log.LogError("Error in process Check_ADX_INTO_BULL" + ex.ToString());
            }
            return result;
        }


        public bool? Check_ADX_CROSS_BULL(int index, double?[] adxArray, double?[] diPlusArray,
    double?[] diMinusArray)
        {
            bool? result = null;

            try
            {
                if (diPlusArray[index] > diMinusArray[index] &&
                    diPlusArray[index + 1] < diMinusArray[index +1])
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
                this._log.LogError("Error in process Check_ADX_INTO_BULL" + ex.ToString());
            }
            return result;
        }


        public bool? Check_ADX_CROSS_BEAR(int index, double?[] adxArray, double?[] diPlusArray,
double?[] diMinusArray)
        {
            bool? result = null;

            try
            {
                if (diPlusArray[index] < diMinusArray[index] &&
                    diPlusArray[index + 1] > diMinusArray[index + 1])
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
                this._log.LogError("Error in process Check_ADX_INTO_BULL" + ex.ToString());
            }
            return result;
        }


        public bool? Check_ADX_TREND_BULL(int index, double?[] adxArray, double?[] diPlusArray,
    double?[] diMinusArray)
        {
            bool? result = null;

            try
            {
                if (diPlusArray[index] > diMinusArray[index] &&
                    adxArray[index] > diMinusArray[index])
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
                this._log.LogError("Error in process Check_ADX_INTO_BULL" + ex.ToString());
            }
            return result;
        }

        public bool? Check_ADX_TREND_BEAR(int index, double?[] adxArray, double?[] diPlusArray,
double?[] diMinusArray)
        {
            bool? result = null;

            try
            {
                if (diPlusArray[index] < diMinusArray[index] &&
                    adxArray[index] > diPlusArray[index])
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
                this._log.LogError("Error in process Check_ADX_INTO_BULL" + ex.ToString());
            }
            return result;
        }


        public async Task SaveScanResultWeekly(DriveService service, List<ScanResultEntity> scanResultList, string rootID)
        {
            this._log.LogInformation(" in Save Scan Result weekly");


            if (scanResultList != null && scanResultList.Count > 0)
            {
                var dateScan = scanResultList[0].TradingDate.ToString();
                var csvString = ConvertToCsv<ScanResultEntity>(scanResultList);

                var folderId = GoogleDriveManager.FindOrCreateFolder(service, rootID, dateScan.Substring(0,6));

                var fileName = $"weekly_scanresult_{dateScan}.csv";

                GoogleDriveManager.UploadCsvStringToDriveFolder(service, folderId, csvString, fileName);

                this._log.LogInformation($"After upload weekly scan result to  {fileName} {folderId}");
            }
        }

        public async Task SaveScanResultDaily(DriveService service, List<ScanResultEntity> scanResultList, string rootID, string direction = "bull")
        {
            this._log.LogInformation(" in Save Scan Result daily");


            if (scanResultList != null && scanResultList.Count > 0)
            {
                var dateScan = scanResultList[0].TradingDate.ToString();
                var csvString = ConvertToCsv<ScanResultEntity>(scanResultList);

                var folderId = GoogleDriveManager.FindOrCreateFolder(service, rootID, dateScan.Substring(0, 6));

                var fileName = $"daily_scanresult_{direction}_{dateScan}.csv";

                GoogleDriveManager.UploadCsvStringToDriveFolder(service, folderId, csvString, fileName);

                this._log.LogInformation($"After upload daily scan result to  {fileName} {folderId}");
            }
        }


        public static string ConvertToCsv<T>(IEnumerable<T> data)
        {
            using (var writer = new StringWriter())
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(data);
                csv.Flush();
                return writer.ToString();
            }
        }

    }
}