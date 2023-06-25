using CsvHelper;
using Google.Apis.Drive.v3;
using Microsoft.Extensions.Logging;
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
                var isCrossBull = this.Check_MACD_CROSS_BULL(i, macdArray, macdSignalArray, macdHistArray);
                var isAdxBull = this.Check_ADX_INTO_BULL(i, adxArray, diPlusArray, diMinusArray);
                var isAdxCrossBull = this.Check_ADX_CROSS_BULL(i, adxArray, diPlusArray, diMinusArray);
                var isAdxTrendBull = this.Check_ADX_TREND_BULL(i, adxArray, diPlusArray, diMinusArray);

                var scanResult = new ScanResultEntity()
                {
                    Symbol = orderedIndicators[0].Code,
                    TradingDate = periodArray[i],
                    MACD_REVERSE_BULL = isReverseBull,
                    MACD_CROSS_BULL = isCrossBull,
                    ADX_INTO_BULL = isAdxBull,
                    ADX_CROSS_BULL = isAdxCrossBull,
                    ADX_TREND_BULL = isAdxTrendBull
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

        public async Task SaveScanResultWeekly(DriveService service, List<ScanResultEntity> scanResultList, string rootID)
        {
            this._log.LogInformation(" in Save Scan Result weekly");


            if (scanResultList != null && scanResultList.Count > 0)
            {
                var dateScan = scanResultList[0].TradingDate.ToString();
                var csvString = ConvertToCsv<ScanResultEntity>(scanResultList);

                var folderId = this.FindOrCreateFolder(service, rootID, dateScan.Substring(0,6));

                var fileName = $"weekly_scanresult_{dateScan}.csv";

                UploadCsvStringToDriveFolder(service, folderId, csvString, fileName);

                this._log.LogInformation($"After upload weekly scan result to  {fileName} {folderId}");
            }
        }

        static void UploadCsvStringToDriveFolder(DriveService service, string folderId, string csvData, string fileName)
        {
            // Search for existing file by name and parent folder
            var query = $"name = '{fileName}' and '{folderId}' in parents";
            var listRequest = service.Files.List();
            listRequest.Q = query;
            var existingFiles = listRequest.Execute().Files;

            // Delete existing file if found
            if (existingFiles != null && existingFiles.Count > 0)
            {
                var fileId = existingFiles[0].Id;
                service.Files.Delete(fileId).Execute();
                Console.WriteLine($"Deleted existing file: {fileName}, File ID: {fileId}");
            }

            // Create new file
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,
                Parents = new List<string>() { folderId }
            };

            var byteArray = System.Text.Encoding.UTF8.GetBytes(csvData);
            var stream = new MemoryStream(byteArray);

            FilesResource.CreateMediaUpload request = service.Files.Create(fileMetadata, stream, "text/csv");
            request.Fields = "id";
            request.Upload();

            var file = request.ResponseBody;
            Console.WriteLine($"Uploaded file: {file.Name}, File ID: {file.Id}");
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

        public string FindOrCreateFolder(DriveService service, string parentFolderId, string folderName)
        {
            // List all folders
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.Q = $"mimeType='application/vnd.google-apps.folder' and '{parentFolderId}' in parents";
            listRequest.Fields = "files(id, name)";

            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    if (file.Name == folderName)
                        return file.Id;
                }
            }

            // If the folder doesn't exist, create it
            var folderMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string>
            {
                parentFolderId
            }
            };

            var request = service.Files.Create(folderMetadata);
            request.Fields = "id";
            var folder = request.Execute();

            return folder.Id;
        }
    }
}