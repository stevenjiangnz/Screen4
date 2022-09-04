using CsvHelper.Configuration.Attributes;

namespace Screen.Entity
{
    public class SymbolEntity
    {
        public SymbolEntity()
        {

        }

        public SymbolEntity(string code, string company, string sector, long marketCap, double weight)
        {
            Code = code;
            Company = company;
            Sector = sector;
            MarketCap = marketCap;
            Weight = weight;
        }

        [Index(0)]
        public string Code { get; set; }
        
        [Index(1)]
        public string Company { get; set; }

        [Index(2)]
        public string Sector { get; set; }

        [Index(3)]
        public long MarketCap { get; set; }

        [Index(4)]

        public double Weight { get; set; }
    }
}