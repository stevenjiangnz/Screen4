using CsvHelper.Configuration;
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
using Microsoft.Extensions.Logging;

namespace Screen.Symbols
{
    public class IbkrSymbolManager
    {
        private readonly ILogger _log;

        public IbkrSymbolManager(ILogger log)
        {
            _log = log;
        }

        public List<IbkrEtfSymbolEntity> GetSymbolList(DriveService service, string rootId, string fileName)
        {
            try
            {
                var ibkretfFolderId = GoogleDriveManager.FindOrCreateFolder(service, rootId, "ibkr");
                var symbolFolderId = GoogleDriveManager.FindOrCreateFolder(service, ibkretfFolderId, "symbols");
                var content = GoogleDriveManager.DownloadTextStringFromDriveFolder(service, symbolFolderId, fileName);
                var symbolList = ConvertCSVToList(content);
                return symbolList;
            }
            catch (Exception ex)
            {
                _log.LogError($"Error in GetSymbolList: {ex.Message}");
                return new List<IbkrEtfSymbolEntity>();
            }
        }

        public List<IbkrEtfSymbolEntity> ConvertCSVToList(string csvContent)
        {
            using (var reader = new StringReader(csvContent))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                MissingFieldFound = null
            }))
            {
                csv.Context.RegisterClassMap<IbkrEtfSymbolEntityMap>();
                var records = csv.GetRecords<IbkrEtfSymbolEntity>().ToList();
                return records;
            }
        }

        public sealed class IbkrEtfSymbolEntityMap : ClassMap<IbkrEtfSymbolEntity>
        {
            public IbkrEtfSymbolEntityMap()
            {
                Map(m => m.Symbol).Name("SYMBOL");
                Map(m => m.Description).Name("DESCRIPTION");
                Map(m => m.IbkrSymbol).Name("IBKR-SYMBOL");
                Map(m => m.Currency).Name("CURRENCY");
                Map(m => m.Product).Name("PRODUCT");
                Map(m => m.Region).Name("REGION");
                Map(m => m.Exchange).Name("EXCHANGE");
            }
        }

    }
}
