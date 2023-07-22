using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

    }
}
