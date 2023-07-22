using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using HtmlAgilityPack;
using Screen.Entity;
using Screen.Access;
using Screen.Utils;

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


                var driveService = GoogleDriveManager.GetDriveServic(this._appSettings.ETSettings.GoogleServiceAccountKey);

                var etoroFolder = GoogleDriveManager.FindOrCreateFolder(driveService, this._appSettings.ETSettings.GoogleRootId, "etoro");
                var instrumentFolder = GoogleDriveManager.FindOrCreateFolder(driveService, etoroFolder, "instruments");

                
                var symbolList =  await this.GetMarketInstuments("etf",
                    baseUrl + this._appSettings.ETSettings.ETFSettings.Suffix);

                GoogleDriveManager.UploadTextStringToDriveFolder(driveService, instrumentFolder, symbolList.ToJsonString(true), "etoro_etf_list.json");
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error in GetInstruments");
            }

            return "example instruments" + this._appSettings.ToString();
        }


        public async Task<List<ETSymbolEntity>> GetMarketInstuments(string market, string marketUrl)
        {
            this._logger.LogInformation($"in GetMarketInstuments {market}, {marketUrl}");

            List<ETSymbolEntity> symbolList = new List<ETSymbolEntity>();

            IWebDriver driver = new ChromeDriver();

            try
            {
                // Navigate to the desired web page
                driver.Navigate().GoToUrl(marketUrl);

                // Wait for the page to load (you can use more sophisticated waits if needed)
                Thread.Sleep(2000);

                bool hasNextPage = true;

                while (hasNextPage)
                {
                    var elements = driver.FindElements(By.ClassName("card-avatar-wrap"));

                    foreach (var element in elements)
                    {
                        var symbolElement = element.FindElement(By.ClassName("symbol"));
                        var nameElement = element.FindElement(By.ClassName("name"));

                        symbolList.Add(new ETSymbolEntity(market, symbolElement.Text, nameElement.Text));
                    }

                    var nextPageButton = driver.FindElement(By.CssSelector("[automation-id='discover-market-next-button']"));

                    if (nextPageButton.GetAttribute("class").Contains("disabled"))
                    {
                        // If the "Next" button is disabled, we've reached the end of the pages
                        hasNextPage = false;
                    }
                    else
                    {
                        // Click the "Next" button to go to the next page
                        nextPageButton.Click();

                        // Wait for the next page to load
                        Thread.Sleep(2000);
                    }
                }

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

            return symbolList;
        }

    }
}
