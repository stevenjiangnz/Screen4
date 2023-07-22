using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Screen.Entity;
using Screen.Access;
using Screen.Utils;
using Google.Apis.Drive.v3;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Firefox;

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

                // download etf instruments
                var marketType = "etf";
                var markListName = $"etoro_list_{marketType}.json";
                var marketUrl = baseUrl + this._appSettings.ETSettings.ETFSettings.Suffix;

                //await this.RefreshMarjetInstructments(marketType, marketUrl, driveService, instrumentFolder, markListName);

                // download asx instrumentsn 
                marketType = "asx";
                markListName = $"etoro_list_{marketType}.json";
                marketUrl = baseUrl + this._appSettings.ETSettings.ASXSettings.Suffix;
                
                await this.RefreshMarjetInstructments(marketType, marketUrl, driveService, instrumentFolder, markListName);

            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error in GetInstruments");
            }

            return "example instruments" + this._appSettings.ToString();
        }

        public async Task RefreshMarjetInstructments(string market, string marketUrl, 
            DriveService driveService, string folderId, string fileName)
        {
            try
            {
                this._logger.LogInformation($"In  RefreshMarjetInstructments, market {market}");
                
                var symbolList = await this.GetMarketInstuments(market, marketUrl);

                this._logger.LogInformation($"After retrieve instruments. returned {symbolList.Count} items");

                GoogleDriveManager.UploadTextStringToDriveFolder(driveService, folderId, symbolList.ToJsonString(true), fileName);

                this._logger.LogInformation($"After save instruments into {fileName}");

            } catch (Exception ex)
            {

                this._logger.LogError(ex, $"Error in RefreshMarjetInstructments. market: {market}");
            }
        }

        public async Task<List<ETSymbolEntity>> GetMarketInstuments(string market, string marketUrl)
        {
            this._logger.LogInformation($"in GetMarketInstuments {market}, {marketUrl}");

            List<ETSymbolEntity> symbolList = new List<ETSymbolEntity>();

            IWebDriver driver = new FirefoxDriver();  // change this line
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

            try
            {
                // Navigate to the desired web page
                driver.Navigate().GoToUrl(marketUrl);
                wait.Until(drv => drv.Url.Contains(marketUrl));

                bool hasNextPage = true;

                while (hasNextPage)
                {
                    var elements = driver.FindElements(By.ClassName("card-avatar-wrap"));

                    string firstElementText = "";
                    if (elements.Count > 0)
                    {
                        var firstElement = elements[0].FindElement(By.ClassName("symbol"));
                        firstElementText = firstElement.Text;
                    }

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

                        // Wait for the first symbol of the new page to be loaded and to be different from the first symbol of the old page
                        wait.Until(drv => drv.FindElements(By.ClassName("card-avatar-wrap"))[0].FindElement(By.ClassName("symbol")).Text != firstElementText);
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
