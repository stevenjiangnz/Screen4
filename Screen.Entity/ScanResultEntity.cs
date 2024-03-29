﻿using System;
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
        public bool? MACD_CROSS_BULL { get; set; }
        public bool? ADX_INTO_BULL { get; set; }
        public bool? ADX_CROSS_BULL { get; set; }
        public bool? ADX_TREND_BULL { get; set; }
        public bool? MACD_REVERSE_BEAR { get; set; }
        public bool? MACD_CROSS_BEAR { get; set; }
        public bool? ADX_INTO_BEAR { get; set; }
        public bool? ADX_CROSS_BEAR { get; set; }
        public bool? ADX_TREND_BEAR { get; set; }
    }
}
