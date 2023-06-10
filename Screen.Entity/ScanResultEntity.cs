using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.Entity
{
    public class ScanResultEntity
    {
        public int TradingDate { get; set; }
        public string Symbol { get; set; }
        public bool? MACD_REVERSE_BULL { get; set; }
        public bool? ADX_INTO_BULL { get; set; }
    }
}
