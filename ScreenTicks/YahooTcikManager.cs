using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Screen.Shared;
using Screen.Utils;

namespace Screen.Ticks
{
    public class YahooTcikManager
    {
        private SharedSettings _settings;

        public YahooTcikManager(SharedSettings settings)
        {
            this._settings = settings;
        }

        public async Task<string> DownloadTicks(string symbol, DateTime start, DateTime end)
        {
            //string url = "https://query1.finance.yahoo.com/v7/finance/download/AFI.AX?period1=1653690768&period2=1685226768&interval=1d&events=history&includeAdjustedClose=true"; // Replace with the actual URL of the CSV file

            string url = this.getYahooTickUrl(_settings.YahooUrlTemplate, symbol, DateHelper.ToTimeStamp(start), DateHelper.ToTimeStamp(end));
            string tickerContent = string.Empty;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    using (HttpContent content = response.Content)
                    {
                        tickerContent = await content.ReadAsStringAsync();

                        //// Perform operations with the CSV data
                        //// For example, you can save it to a file
                        //string filePath = "path/to/save/file.csv"; // Replace with the desired file path
                        //await System.IO.File.WriteAllTextAsync(filePath, csv);

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }

            return tickerContent;

        }

        public async Task SaveTickers(string symbol, string tickerContent)
        {
            string tickFilePath = Path.Combine(this._settings.YahooFilePath, symbol.ToLower());

            await File.WriteAllTextAsync(tickFilePath, tickerContent);
        }

        public string getYahooTickUrl(string template, string symbol, long start, long end, string interval = "1d")
        {
            string urlResult = string.Format(template, symbol, start, end, symbol);
            
            return urlResult;

        }
    }
}
