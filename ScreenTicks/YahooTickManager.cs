using Screen.Entity;
using Screen.Shared;
using Screen.Utils;
using Microsoft.Extensions.Logging;

namespace Screen.Ticks
{
    public class YahooTickManager
    {
        private SharedSettings _settings;
        private readonly ILogger _logger;
        public YahooTickManager(SharedSettings settings, ILogger log)
        {
            this._settings = settings;
            this._logger = log;
        }

        /// <summary>
        /// down load tick
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="interval">possible value '1d' or '1wk'</param>
        /// <returns></returns>
        public async Task<string> DownloadYahooTicks(string symbol, DateTime start, DateTime end, string interval = "1d")
        {
            //string url = "https://query1.finance.yahoo.com/v7/finance/download/AFI.AX?period1=1653690768&period2=1685226768&interval=1d&events=history&includeAdjustedClose=true"; // Replace with the actual URL of the CSV file

            symbol = symbol.IndexOf(".AX") > 0 ? symbol.ToUpper() : symbol.ToUpper().Trim() + ".AX";

            string url = this.getYahooTickUrl(_settings.YahooUrlTemplate, symbol, DateHelper.ToTimeStamp(start), DateHelper.ToTimeStamp(end), interval);
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
            string urlResult = string.Format(template, symbol, start, end, interval);

            return urlResult;
        }

        public List<TickerEntity> ConvertToEntities(string symbol, string tickerString)
        {
            List<TickerEntity> tickerEntities = new List<TickerEntity>();

            string[] lines = tickerString.Split(new[] { '\n' }, StringSplitOptions.None);

            foreach (string line in lines)
            {
                // take out the invalid lines
                if ((!line.Contains("Date,")) && (!line.Contains("null,null")))
                {
                    try
                    {
                        // Process each line here
                        var entity = new TickerEntity(symbol, line);
                        tickerEntities.Add(entity);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in parse: {symbol}\n" + line + "\n" + ex.ToString());
                    }
                }
            }

            return tickerEntities;
        }

        public async Task<List<TickerEntity>> GetEtTickerList(string symbol, DateTime start, DateTime end, string interval = "1d")
        {
            List<TickerEntity> tickerList = new List<TickerEntity>();

            string url = this.getYahooTickUrl(_settings.YahooUrlTemplate, symbol, DateHelper.ToTimeStamp(start), DateHelper.ToTimeStamp(end), interval);
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
                    }

                    tickerList = this.ConvertToEntities(symbol, tickerContent);
                }
                catch (Exception ex)
                {
                    this._logger.LogError($"Error in GetEtTickerList for {symbol}");
                }
            }

            return tickerList;
        }
    }
}
