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

        /// <summary>
        /// get market, e.g. etf, asx, nyse, nasdaq, hongkong
        /// </summary>
        /// <param name="market"></param>
        public async Task ETRetrieveInstruments(string market)
        {
            this._log.LogInformation("in ETRetrieveInstruments");

            string marketUrl = this.GetETInstrumentUrl(market);

            string content = await this.GetWebPageContent(marketUrl);
        }

        public async Task<string> GetWebPageContent(string url)
        {
            string pageContent = string.Empty;

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    //// Set the User-Agent header to mimic a request from Chrome
                    //httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.9999.999 Safari/537.36");

                    //// Send the request to the webpage and get the response
                    //HttpResponseMessage response = await httpClient.GetAsync(url);

                    //// Ensure the request was successful before proceeding
                    //if (response.IsSuccessStatusCode)
                    //{
                    //    // Read the HTML content as a string
                    //    pageContent = await response.Content.ReadAsStringAsync();
                    //} else {
                    //    this._log.LogError($"Failed to fetch the webpage {url}. Status code: {response.StatusCode}");
                    //}
                }
                catch (HttpRequestException ex)
                {
                    _log.LogError($"Error in retreive page {url} \n" + ex.Message);
                }
            }

            return pageContent;
        }


        public string GetETInstrumentUrl(string market)
        {
            string url = string.Empty;

            string baseUrl = Environment.GetEnvironmentVariable("ET_MARKET_INSTRUMENTS_BASE_URL");

            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new Exception($"Missing configuration of ET_MARKET_INSTRUMENTS_BASE_URL");
            }

            this._log.LogInformation($"Using urls: market ({market}), baseUrl({baseUrl})");

            switch (market.ToLower().Trim())
            {
                case "etf":
                    url = baseUrl.ToLower().Trim() + "etf";
                    break;
                default:
                    throw new Exception($"Unknown input, market: {market}");
                    break;
            }

            return url;
        }
    }
}
