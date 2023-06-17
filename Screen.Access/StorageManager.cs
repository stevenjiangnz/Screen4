using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
namespace Screen.Access
{
    public class StorageManager
    {
        private ILogger _log;

        public StorageManager(ILogger log)
        {
            this._log = log;
        }

        public async Task<string> GetSymbolFromAzureStorage(string connStr, string container, string symbolListFileName)
        {
            string symbolContent = string.Empty;
            // Initialize the BlobServiceClient
            BlobServiceClient blobServiceClient = new BlobServiceClient(connStr);

            // Get a reference to the container
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(container);

            this._log.LogInformation($"File Name: {symbolListFileName}");

            // Get a reference to the blob
            BlobClient blobClient = containerClient.GetBlobClient(symbolListFileName);

            // Download the blob's content
            BlobDownloadInfo downloadInfo = await blobClient.DownloadAsync();

            // Read the content of the blob
            using (StreamReader reader = new StreamReader(downloadInfo.Content))
            {
                symbolContent = await reader.ReadToEndAsync();
                this._log.LogInformation($"Returned file content size: {symbolContent.Length}");
            }

            return symbolContent;
        }
    }
}