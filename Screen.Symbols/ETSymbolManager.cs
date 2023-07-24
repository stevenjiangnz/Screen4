using CsvHelper;
using CsvHelper.Configuration;
using Google.Apis.Drive.v3;
using Microsoft.Extensions.Logging;
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
    public class ETSymbolManager
    {
        private readonly ILogger _log;
        public ETSymbolManager(ILogger log)
        {
            this._log = log;
        }

        public List<ETSymbolEntity> GetEtSymbolFullList(DriveService service, string rootId, string fileName)
        {
            var etoroId = GoogleDriveManager.FindOrCreateFolder(service, rootId, "etoro");
            var instrumentsId = GoogleDriveManager.FindOrCreateFolder(service, etoroId, "instruments");

            var content = GoogleDriveManager.DownloadTextStringFromDriveFolder(service, instrumentsId, fileName);

            var etSymbolList = ConvertCSVToList(content);

            return etSymbolList;
        }

        public List<ETSymbolEntity> GetEtAsxSymbolList(List<ETSymbolEntity> eTSymbolEntities)
        {
            return eTSymbolEntities.Where(s=>
            s.Symbol.ToLower().IndexOf(".asx") > 0).ToList();
        }

        public List<ETSymbolEntity> GetEtEtfSymbolList(List<ETSymbolEntity> eTSymbolEntities)
        {
            return eTSymbolEntities.Where(s =>
            s.InstrumentType.ToLower() == "etf" ).ToList();
        }

        public List<ETSymbolEntity> GetEtHkSymbolList(List<ETSymbolEntity> eTSymbolEntities)
        {
            return eTSymbolEntities.Where(s =>
            s.Exchange.ToLower() == "hongkong").ToList();
        }

        public List<ETSymbolEntity> GetEtNasdaqSymbolList(List<ETSymbolEntity> eTSymbolEntities)
        {
            return eTSymbolEntities.Where(s =>
            s.Exchange.ToLower() == "nasdaq" && 
            s.InstrumentType.ToLower() == "stocks").ToList();
        }

        public List<ETSymbolEntity> GetEtUkSymbolList(List<ETSymbolEntity> eTSymbolEntities)
        {
            return eTSymbolEntities.Where(s =>
            s.Exchange.ToLower() == "london" &&
            s.InstrumentType.ToLower() == "stocks").ToList();
        }

        public List<ETSymbolEntity> ConvertCSVToList(string csvContent)
        {
            using (var reader = new StringReader(csvContent))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<ETSymbolEntityMap>();
                var records = csv.GetRecords<ETSymbolEntity>();
                return new List<ETSymbolEntity>(records);
            }
        }

        public sealed class ETSymbolEntityMap : ClassMap<ETSymbolEntity>
        {
            public ETSymbolEntityMap()
            {
                Map(m => m.Name).Name("name");
                Map(m => m.Symbol).Name("symbol");
                Map(m => m.InstrumentType).Name("instrument type");
                Map(m => m.Exchange).Name("exchange");
                Map(m => m.Industry).Name("industry");
            }
        }
    }
}
