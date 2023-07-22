using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Screen.ETSymbol.Loader
{
    public class ETInstrumentManager
    {

        private readonly ILogger<ETInstrumentManager> _logger;
        private readonly AppSettings _appSettings;

        public ETInstrumentManager(IOptions<AppSettings> appSettings,
            ILogger<ETInstrumentManager> logger) { 
            this._appSettings = appSettings.Value;
            this._logger = logger;
        }

        public async Task<string> GetInstruments(string market)
        {
            this._logger.LogInformation("example instruments" + this._appSettings.ToString());

            return "example instruments" + this._appSettings.ToString();
        }
    }
}
