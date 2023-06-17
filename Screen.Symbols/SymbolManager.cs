

using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Screen.Entity;
using Screen.Shared;

namespace Screen.Symbols
{
    public class SymbolManager
    {
        private SharedSettings _settings;
        private ILogger _log;

        public SymbolManager(SharedSettings settings)
        {
            _settings = settings;
        }

        public SymbolManager(ILogger log)
        {
            this._log = log;
        }


        public List<SymbolEntity> GetSymbolsFromAzureStorage(int? takeCount = null)
        {
            List<SymbolEntity> symbolList = new List<SymbolEntity>();
            return symbolList;
        }


        public List<SymbolEntity> LoadFullSymbolList(int? takeCount)
        {
            List<SymbolEntity> symbolList = new List<SymbolEntity>();

            if (_settings.BasePath != null && _settings.SymbolFullFileName != null)
            {
                using (var reader = new StreamReader(Path.Combine(_settings.BasePath, _settings.SymbolFullFileName)))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Context.RegisterClassMap<SymbolEntityMap>();
                    var records = csv.GetRecords<SymbolEntity>();

                    if (takeCount.HasValue)
                    {
                        symbolList = records.OrderByDescending(s => s.MarketCap).Take(takeCount.Value).ToList();
                    }
                    else
                    {
                        symbolList = records.OrderByDescending(s => s.MarketCap).ToList();
                    }
                }
            }

            return symbolList;
        }


        public class SymbolEntityMap : ClassMap<SymbolEntity>
        {
            public SymbolEntityMap()
            {
                Map(m => m.Code);
                Map(m => m.Company);
                Map(m => m.Sector);
                Map(m => m.MarketCap).Convert(
                    (row) => long.Parse(row.Row.GetField("MarketCap").Replace(",", ""))
                );
                Map(m => m.Weight);

            }
        }


    }
}