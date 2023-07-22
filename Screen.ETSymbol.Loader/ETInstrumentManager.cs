using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using HtmlAgilityPack;

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

        public async Task<string> GetInstruments()
        {   
            try
            {
                this._logger.LogInformation("example instruments" + this._appSettings.ToString());
                var baseUrl = this._appSettings.ETSettings.BaseUrl;

                await this.GetMarketInstuments("etf",
                    baseUrl + this._appSettings.ETSettings.ETFSettings.Suffix);

            } catch (Exception ex)
            {
                this._logger.LogError(ex, "Error in GetInstruments");
            }

            return "example instruments" + this._appSettings.ToString();
        }


        public async Task GetMarketInstuments(string market, string marketUrl)
        {
            this._logger.LogInformation($"in GetMarketInstuments {market}, {marketUrl}");

            // Specify the path to the ChromeDriver executable
            var chromeDriverPath = "path/to/chromedriver.exe";

            // Create a new ChromeDriver instance
            var options = new ChromeOptions();
            options.AddArgument("--headless"); // Run Chrome in headless mode (without GUI)
            IWebDriver driver = new ChromeDriver();

            try
            {
                // Navigate to the desired web page
                driver.Navigate().GoToUrl(marketUrl);

                // Wait for the page to load (you can use more sophisticated waits if needed)
                Thread.Sleep(2000);

                int i =0;
                
                var elements = driver.FindElements(By.ClassName("card-avatar-wrap"));

                foreach (var element in elements)
                {
                    var symbolElement = element.FindElement(By.ClassName("symbol"));
                    var nameElement = element.FindElement(By.ClassName("name"));
                    Console.WriteLine("Symbol: " + symbolElement.Text);
                    Console.WriteLine("Name: " + nameElement.Text);
                    i++;
                }

                Console.WriteLine($"found items {i + 1}");

            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            finally
            {
                // Quit the driver and close the browser
                driver.Quit();
            }
        }
    }
}
