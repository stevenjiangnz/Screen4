using System.IO.Compression;
using System.Text;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using Screen.Entity;
using Screen.Shared;
using Screen.Utils;
using Serilog;
namespace Screen.Ticks
{
    public class TickerManager
    {

        public void ProcessTickers(SharedSettings settings, IList<SymbolEntity> symbolList, int? loadDays = null)
        {
            LoadTickerFromEmail(settings, loadDays);

            ProcessTickersFromDownload(settings, symbolList);
        }

        public void LoadTickerFromEmail(SharedSettings settings, int? loadDays = null)
        {
            Log.Debug($"in LoadTickerFromEmail... basepath: {settings.BasePath}");
            using (var client = new ImapClient())
            {
                int downloadFiles = 0;
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.CheckCertificateRevocation = false;
                Log.Information("About to connect to email...");
                client.Connect("imap.gmail.com", 993, true);
                client.Authenticate(settings.TickerEmailAccount, settings.TickerEmailPWD);

                Log.Information("Successfully connected to email.");

                // The Inbox folder is always available on all IMAP servers...
                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadWrite);

                IList<UniqueId> searchResult;

                if (loadDays != null)
                {
                    searchResult = inbox.Search(SearchQuery.DeliveredAfter(DateTime.Now.AddDays(-1 * loadDays.Value)));
                }
                else
                {
                    searchResult = inbox.Search(SearchQuery.All);
                }

                foreach (var uid in searchResult)
                {
                    try
                    {
                        var message = inbox.GetMessage(uid);

                        Log.Debug($"Uid: {uid.ToString()}, Subject: {message.Subject},  Time: {DateTime.Now.ToLongTimeString()}");


                        if (message.Subject.IndexOf("Daily Historical Data") >= 0)
                        {
                            foreach (MimeEntity attachment in message.Attachments)
                            {

                                var fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;

                                var year = "";
                                if (fileName.IndexOf("_") > 0)
                                {
                                    year = fileName.Substring(fileName.IndexOf("_") + 1, 4);
                                }

                                var tickerDir = Path.Combine(Path.Combine(settings.BasePath, settings.TickerPath), year);
                                Directory.CreateDirectory(tickerDir);

                                fileName = Path.Combine(tickerDir, fileName);

                                if (!File.Exists(fileName))
                                {
                                    Log.Debug($"About to download file name: {fileName}");
                                    using (var stream = File.Create(fileName))
                                    {
                                        if (attachment is MessagePart)
                                        {
                                            var rfc822 = (MessagePart)attachment;
                                            rfc822.Message.WriteTo(stream);
                                        }
                                        else
                                        {
                                            var part = (MimePart)attachment;
                                            part.Content.DecodeTo(stream);
                                        }
                                    }
                                    Log.Information($"Downloaded file name: {fileName}");
                                    downloadFiles++;
                                }
                                else
                                {
                                    Log.Warning($"File {fileName} already exists. skipped.");
                                }

                            }
                        }

                        inbox.AddFlags(uid, MessageFlags.Seen, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Error download email, uid: {uid}.");
                    }
                }

                client.Disconnect(true);

                Log.Information($"Downloaded {downloadFiles} files into local");
            }
        }

        public void ProcessTickersFromDownload(SharedSettings settings, IList<SymbolEntity> symbolList, int? days = null)
        {
            Log.Debug($"in ProcessTickersFromDownload... basepath: {settings.BasePath}, symbols : {symbolList.Count}");

            Dictionary<string, IList<TickerEntity>> symbolTickers = new Dictionary<string, IList<TickerEntity>>();

            // init symbolList
            foreach (SymbolEntity symbol in symbolList)
            {
                symbolTickers.Add(symbol.Code, new List<TickerEntity>());
            }

            var tickerFileList = GetTickerFileList(settings, days);

            foreach (string file in tickerFileList)
            {
                try
                {
                    var tickerString = File.ReadAllText(file);
                    var tickers = getTickerListFromString(tickerString);

                    foreach (TickerEntity ticker in tickers)
                    {
                        if (symbolTickers.ContainsKey(ticker.T))
                        {
                            symbolTickers[ticker.T].Add(ticker);
                        }
                    }

                    Log.Information($"Process ticker for file {file}, ticker {tickers.Count}");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error in loading ticker from file {file}");
                }
            }


            foreach (string code in symbolTickers.Keys)
            {
                SaveProcessedTickets(settings, code, symbolTickers[code]);
            }

            Log.Information($"Processed {tickerFileList.Count} files into to individual code file.");
        }


        public void SaveProcessedTickets(SharedSettings settings, string code, IList<TickerEntity> tickerEntityList)
        {
            try
            {
                var processedFolder = Path.Combine(settings.BasePath, settings.TickerProcessedPath);
                Directory.CreateDirectory(processedFolder);

                string filePath = Path.Combine(processedFolder, code + "_day.txt");

                IList<TickerEntity> mergedTickers = new List<TickerEntity>();
                IList<TickerEntity> existingTickers = new List<TickerEntity>();

                if (File.Exists(filePath))
                {
                    var tickerString = File.ReadAllText(filePath);
                    existingTickers = getTickerListFromString(tickerString);
                }

                var comparer = new TickerComparer();
                mergedTickers = existingTickers.Union(tickerEntityList, comparer).OrderBy(m => m.P).ToList();

                if (mergedTickers.Count > existingTickers.Count)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var ticker in mergedTickers)
                    {
                        sb.AppendLine(ticker.ToString());
                    }

                    File.WriteAllText(filePath, sb.ToString());
                    Log.Information($"Add {mergedTickers.Count - existingTickers.Count} tickers into file {filePath}.");
                } else
                {
                    //Log.Warning($"No new tickers found, nothing to write for code {code}");
                }

            } catch (Exception ex)
            {
                Log.Error(ex, "Error in SaveProcessedTickets");
            }
        }


        public IList<string> GetTickerFileList(SharedSettings settings, int? days)
        {
            IList<string> fileList = new List<string>();
            try
            {
                string tickerFolder = Path.Combine(settings.BasePath, settings.TickerPath);

                string[] files = Directory.GetFiles(tickerFolder, "Metastock_*.txt", SearchOption.AllDirectories);

                foreach (string file in files)
                    fileList.Add(file);

                if (days != null)
                {
                    fileList = fileList.OrderByDescending(fi=>fi.ToString()).Take(days.Value).ToList();
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetTickerFileList");
            }

            Log.Debug($"Load ticker {fileList.Count} files.");
            return fileList;
        }

        public List<TickerEntity> getTickerListFromString(string content)
        {
            List<TickerEntity> tickers = new List<TickerEntity>();

            var tickerLines = StringHelper.SplitToLines(content);

            foreach (string line in tickerLines)
            {
                if (!string.IsNullOrEmpty(line.Trim()))
                {
                    tickers.Add(new TickerEntity(line));
                }
            }

            return tickers;
        }


        public class TickerComparer : IEqualityComparer<TickerEntity>
        {
            public bool Equals(TickerEntity t1, TickerEntity t2)
            {
                if (t1.ToString().ToUpper() == t2.ToString().ToUpper())
                {
                    return true;
                }
                return false;
            }

            public int GetHashCode(TickerEntity o)
            {
                return o.P;
            }
        }
    }
}