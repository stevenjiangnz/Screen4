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
                settings.Symbol300FileName = appSettings["Symbol300FileName"];
                settings.SymbolFullFileName = appSettings["SymbolFullFileName"];

            }

            return settings;
        }
    }
}