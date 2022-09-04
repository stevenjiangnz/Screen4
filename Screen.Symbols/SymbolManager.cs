

using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Screen.Entity;
using Screen.Shared;

namespace Screen.Symbols
{
    public class SymbolManager
    {
        public List<SymbolEntity> LoadFullSymbolList(SharedSettings settings)
        {
            List<SymbolEntity> symbolList = new List<SymbolEntity>();

            if (settings.BasePath != null && settings.SymbolFullFileName != null)
            {
                using (var reader = new StreamReader(Path.Combine(settings.BasePath, settings.SymbolFullFileName)))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Context.RegisterClassMap<SymbolEntityMap>();
                    var records = csv.GetRecords<SymbolEntity>();

                    symbolList = records.ToList();
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