using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using CsvHelper;
using Google.Apis.Drive.v3;
using Screen.Access;
using Screen.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.Symbols
{
    public class CurrencyPairSymbolManager
    {
        private readonly ILogger _log;

        public CurrencyPairSymbolManager(ILogger log)
        {
            _log = log;
        }

        public List<CurrencyPairEntity> GetCurrencyPairsFullList(DriveService service, string rootId, string fileName)
        {
            try
            {
                var currencyPairsFolderId = GoogleDriveManager.FindOrCreateFolder(service, rootId, "forex");
                var content = GoogleDriveManager.DownloadTextStringFromDriveFolder(service, currencyPairsFolderId, fileName);
                var currencyPairList = ConvertCSVToList(content);
                return currencyPairList;
            }
            catch (Exception ex)
            {
                _log.LogError($"Error in GetCurrencyPairsFullList: {ex.Message}");
                return new List<CurrencyPairEntity>();
            }
        }

        public List<CurrencyPairEntity> ConvertCSVToList(string csvContent)
        {
            using (var reader = new StringReader(csvContent))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                MissingFieldFound = null
            }))
            {
                csv.Context.RegisterClassMap<CurrencyPairEntityMap>();
                var records = csv.GetRecords<CurrencyPairEntity>().ToList();
                return records;
            }
        }

        public sealed class CurrencyPairEntityMap : ClassMap<CurrencyPairEntity>
        {
            public CurrencyPairEntityMap()
            {
                Map(m => m.Pair).Name("Pair");
                Map(m => m.YahooCode).Name("YahooCode");
            }
        }
    }
}
