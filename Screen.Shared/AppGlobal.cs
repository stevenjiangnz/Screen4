using System.ComponentModel;
using System.Configuration;
using Screen.Utils;

namespace Screen.Shared
{
    public static class AppGlobal
    {
        public static SharedSettings LoadConfig()
        {
            SharedSettings settings = new SharedSettings();

            var appSettings = ConfigurationManager.AppSettings;

            if (appSettings != null)
            {
                settings.BasePath = PathHelper.FixPathSuffix(appSettings["BasePath"]);
                settings.TickerPath = appSettings["TickerPath"];
                settings.SymbolFullFileName = appSettings["SymbolFullFileName"];
            }

            Console.WriteLine($"setting: {ObjectHelper.ToJsonString(settings)}");

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TickerEmailAccount")) &&
                !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TickerEmailPWD")))
            {
                settings.TickerEmailAccount = Environment.GetEnvironmentVariable("TickerEmailAccount");
                settings.TickerEmailPWD = Environment.GetEnvironmentVariable("TickerEmailPWD");
            }
            else
            {
                throw new Exception("missing Ticker Email Account or PWD");
            }

            return settings;
        }
    }
}