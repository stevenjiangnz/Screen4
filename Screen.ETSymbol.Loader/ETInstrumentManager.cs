using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.ETSymbol.Loader
{
    public class ETInstrumentManager
    {
        private readonly MySettings _mySettings;
        private readonly ILogger<ETInstrumentManager> _logger;


        public ETInstrumentManager(IOptions<MySettings> mySettings,
            ILogger<ETInstrumentManager> logger) { 
            this._mySettings = mySettings.Value;
            this._logger = logger;
        }

        public async Task<string> GetInstruments(string market)
        {
            this._logger.LogInformation("example instruments" + this._mySettings.MyKey);

            return "example instruments" + this._mySettings.MyKey;
        }
    }
}
