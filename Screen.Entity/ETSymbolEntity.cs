using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.Entity
{
    public class ETSymbolEntity
    {
        public ETSymbolEntity()
        {

        }

        public ETSymbolEntity(string market,  string symbol, string fullname)
        {
            Market = market;
            Symbol = symbol;
            Fullname = fullname;
        }

        [Index(0)]
        public string Market { get; set; }

        [Index(1)]
        public string Symbol { get; set; }

        [Index(2)]
        public string Fullname { get; set; }
    }
}
