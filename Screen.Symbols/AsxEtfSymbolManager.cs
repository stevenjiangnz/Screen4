using CsvHelper;
using CsvHelper.Configuration;
using Google.Apis.Drive.v3;
using Microsoft.Extensions.Logging;
using Screen.Access;
using Screen.Entity;
using System.Globalization;

namespace Screen.Symbols
{
    public class AsxEtfSymbolManager
    {
        private readonly ILogger _log;

        public AsxEtfSymbolManager(ILogger log)
        {
            _log = log;
        }

        public List<AsxEtfSymbolEntity> GetAsxEtfSymbolFullList(DriveService service, string rootId, string fileName)
        {
            try
            {
                var asxetfFolderId = GoogleDriveManager.FindOrCreateFolder(service, rootId, "asx-etf");
                var content = GoogleDriveManager.DownloadTextStringFromDriveFolder(service, asxetfFolderId, fileName);
                var etSymbolList = ConvertCSVToList(content);
                return etSymbolList;
            }
            catch (Exception ex)
            {
                _log.LogError($"Error in GetAsxEtfSymbolFullList: {ex.Message}");
                return new List<AsxEtfSymbolEntity>();
            }
        }

        public List<AsxEtfSymbolEntity> ConvertCSVToList(string csvContent)
        {
            using (var reader = new StringReader(csvContent))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                MissingFieldFound = null
            }))
            {
                csv.Context.RegisterClassMap<AsxEtfSymbolEntityMap>();
                var records = csv.GetRecords<AsxEtfSymbolEntity>().ToList();
                return records;
            }
        }

        public sealed class AsxEtfSymbolEntityMap : ClassMap<AsxEtfSymbolEntity>
        {
            public AsxEtfSymbolEntityMap()
            {
                Map(m => m.Exposure).Name("Exposure");
                Map(m => m.AsxCode).Name("ASX Code");
                Map(m => m.Type).Name("Type");
                Map(m => m.Inav).Name("iNAV");
                Map(m => m.Benchmark).Name("Benchmark");
                Map(m => m.InvestmentStyle).Name("Investment Style");
                Map(m => m.ManagementCostPercentage).Name("Management Cost %").Convert(args =>
                {
                    var percentageString = args.Row.GetField("Management Cost %").Replace("%", "").Trim();
                    return double.TryParse(percentageString, out double result) ? result / 100 : 0; // Converts percentage to decimal
                });
                Map(m => m.OutperformanceFee).Name("Outperf' Fee");
                Map(m => m.AdmissionDate).Name("Admission Date").TypeConverterOption.Format("d/M/yyyy");
                Map(m => m.IsEnabled).Name("IsEnabled").Convert(args => args.Row.GetField("IsEnabled") == "1");
            }
        }
    }
}
