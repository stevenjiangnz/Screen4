using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Screen.Shared;


namespace Screen.Test
{
    public class TestConfigHelper
    {
        public void LoadConfig()
        {
            IConfigurationRoot _configuration;
            string configPath = AppContext.BaseDirectory + "../../../../ScreenProcess";

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(configPath) // Set the base path to the Xunit project's output directory
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true); // Load the appsettings.Development.json file

            _configuration = configurationBuilder.Build();

            SharedSettings settings = _configuration.GetRequiredSection("Settings").Get<SharedSettings>();
        }
    }
}
