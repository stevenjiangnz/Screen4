

using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Screen.Access;
using Screen.Entity;
using Screen.Shared;
using Google.Apis.Drive.v3;
using Google.Apis.Download;
using System;

namespace Screen.Symbols
{
    public class SymbolManager
    {
        private SharedSettings _settings;
        private ILogger _log;

        public SymbolManager(SharedSettings settings)
        {
            _settings = settings;
        }

        public SymbolManager(ILogger log)
        {
            this._log = log;
        }


        public async Task<List<SymbolEntity>> GetSymbolsFromAzureStorage(string connStr, 
            string container, 
            string symbolListFileName,
            int? takeCount = null)
        {
            List<SymbolEntity> symbolList = new List<SymbolEntity>();
            StorageManager storageManager = new StorageManager(this._log);

            string symbolResultString = await storageManager.GetSymbolFromAzureStorage(connStr, container, symbolListFileName);
            using (var reader = new StringReader(symbolResultString))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Context.RegisterClassMap<SymbolEntityMap>();
                    var records = csv.GetRecords<SymbolEntity>();

                    if (takeCount.HasValue)
                    {
                        symbolList = records.OrderByDescending(s => s.MarketCap).Take(takeCount.Value).ToList();
                    }
                    else
                    {
                        symbolList = records.OrderByDescending(s => s.MarketCap).ToList();
                    }
                }
            }

            return symbolList;
        }

        public async Task<List<SymbolEntity>> GetSymbolsFromGoogleStorage(DriveService service,
            string rootId,
            string symbolListFileName,
            int? takeCount = null)
        {
            List<SymbolEntity> symbolList = new List<SymbolEntity>();
            string symbolResultString = string.Empty;

            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.Q = $"'{rootId}' in parents";
            listRequest.Fields = "nextPageToken, files(id, name, mimeType)";

            // List all files and folders.
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;

            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {   
                    if (file.MimeType != "application/vnd.google-apps.folder" &&
                        file.Name == symbolListFileName)
                    {
                        var request = service.Files.Get(file.Id);
                        var stream = new MemoryStream();
                        request.Download(stream);

                        // You can convert the MemoryStream to a string like this
                        stream.Seek(0, SeekOrigin.Begin);
                        StreamReader reader = new StreamReader(stream);
                        symbolResultString = reader.ReadToEnd();
                    }
                }
            }

            if (symbolResultString == string.Empty)
            {
                throw new Exception("Failed to read symbol string from google drive");
            }

            using (var reader = new StringReader(symbolResultString))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Context.RegisterClassMap<SymbolEntityMap>();
                    var records = csv.GetRecords<SymbolEntity>();

                    if (takeCount.HasValue)
                    {
                        symbolList = records.OrderByDescending(s => s.MarketCap).Take(takeCount.Value).ToList();
                    }
                    else
                    {
                        symbolList = records.OrderByDescending(s => s.MarketCap).ToList();
                    }
                }
            }

            return symbolList;
        }

        public List<SymbolEntity> LoadFullSymbolList(int? takeCount)
        {
            List<SymbolEntity> symbolList = new List<SymbolEntity>();

            if (_settings.BasePath != null && _settings.SymbolFullFileName != null)
            {
                using (var reader = new StreamReader(Path.Combine(_settings.BasePath, _settings.SymbolFullFileName)))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Context.RegisterClassMap<SymbolEntityMap>();
                    var records = csv.GetRecords<SymbolEntity>();

                    if (takeCount.HasValue)
                    {
                        symbolList = records.OrderByDescending(s => s.MarketCap).Take(takeCount.Value).ToList();
                    }
                    else
                    {
                        symbolList = records.OrderByDescending(s => s.MarketCap).ToList();
                    }
                }
            }

            return symbolList;
        }

        public string GetStringFromSymbolList(List<SymbolEntity> symbols)
        {
            string output = string.Empty;

            foreach (var symbol in symbols)
            {
                output = output + symbol.Code + ",";
            }

            if (output.EndsWith(','))
            {
                output = output.TrimEnd(',');
            }

            return output;
        }

        public class SymbolEntityMap : ClassMap<SymbolEntity>
        {
            public SymbolEntityMap()
            {
                Map(m => m.Code);
                Map(m => m.Company);
                Map(m => m.Sector);
                Map(m => m.MarketCap).Convert(
                    (row) => long.Parse(row.Row.GetField("MarketCap").Replace(",", ""))
                );
                Map(m => m.Weight);

            }
        }


    }
}