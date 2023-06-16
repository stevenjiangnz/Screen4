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

        public async Task<string> AzureAccess(string connStr, string container)
        {
            string symbolContent = string.Empty;
            // Initialize the BlobServiceClient
            BlobServiceClient blobServiceClient = new BlobServiceClient(connStr);

            // Get a reference to the container
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(container);

            // List all blobs in the container
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                this._log.LogInformation($"File Name: {blobItem.Name}");

                // Get a reference to the blob
                BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);

                // Download the blob's content
                BlobDownloadInfo downloadInfo = await blobClient.DownloadAsync();

                // Read the content of the blob
                using (StreamReader reader = new StreamReader(downloadInfo.Content))
                {
                    symbolContent = await reader.ReadToEndAsync();
                    this._log.LogInformation($"File Content: {symbolContent}");
                }


            }

            return symbolContent;
        }
    }
}