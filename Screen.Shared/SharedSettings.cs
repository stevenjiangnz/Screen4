using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.Shared
{
    public class SharedSettings
    {
        public string? BasePath { get; set; }
        public string? TickerPath { get; set; }
        public string? SymbolFullFileName { get; set; }
        public string? TickerEmailAccount { get; set; }
        public string TickerEmailPWD { get; set; }
    }
}
