using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Screen.Access
{
    public class StorageManager
    {
        public async Task AzureAccess()
        {
            // Connection string of your Azure Storage account
            string connectionString = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";

            // Name of the container where the files are stored
            string containerName = "screen";

            // Initialize the BlobServiceClient
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // Get a reference to the container
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // List all blobs in the container
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                Console.WriteLine($"File Name: {blobItem.Name}");

                // Get a reference to the blob
                BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);

                // Download the blob's content
                BlobDownloadInfo downloadInfo = await blobClient.DownloadAsync();

                // Read the content of the blob
                using (StreamReader reader = new StreamReader(downloadInfo.Content))
                {
                    string content = await reader.ReadToEndAsync();
                    Console.WriteLine($"File Content: {content}");
                }

                Console.WriteLine();
            }
        }
    }
}