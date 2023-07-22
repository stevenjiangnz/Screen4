using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Screen.Access
{
    public class GoogleDriveManager
    {
        private readonly ILogger _log;
        public GoogleDriveManager(ILogger log)
        {
            this._log = log;
        }

        public static DriveService GetDriveServic(string serviceAccountKeyJson)
        {
            GoogleCredential credential;

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(serviceAccountKeyJson)))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(DriveService.Scope.Drive);
            }

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });

            return service;
        }


        public static string FindOrCreateFolder(DriveService service, string parentFolderId, string folderName)
        {
            // List all folders
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.Q = $"mimeType='application/vnd.google-apps.folder' and '{parentFolderId}' in parents";
            listRequest.Fields = "files(id, name)";

            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    if (file.Name == folderName)
                        return file.Id;
                }
            }

            // If the folder doesn't exist, create it
            var folderMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string>
            {
                parentFolderId
            }
            };

            var request = service.Files.Create(folderMetadata);
            request.Fields = "id";
            var folder = request.Execute();

            return folder.Id;
        }

        public static void UploadTextStringToDriveFolder(DriveService service, string folderId, string csvData, string fileName)
        {
            // Search for existing file by name and parent folder
            var query = $"name = '{fileName}' and '{folderId}' in parents";
            var listRequest = service.Files.List();
            listRequest.Q = query;
            var existingFiles = listRequest.Execute().Files;

            // Delete existing file if found
            if (existingFiles != null && existingFiles.Count > 0)
            {
                var fileId = existingFiles[0].Id;
                service.Files.Delete(fileId).Execute();
                Console.WriteLine($"Deleted existing file: {fileName}, File ID: {fileId}");
            }

            // Create new file
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,
                Parents = new List<string>() { folderId }
            };

            var byteArray = System.Text.Encoding.UTF8.GetBytes(csvData);
            var stream = new MemoryStream(byteArray);

            FilesResource.CreateMediaUpload request = service.Files.Create(fileMetadata, stream, "text/csv");
            request.Fields = "id";
            request.Upload();

            var file = request.ResponseBody;
            Console.WriteLine($"Uploaded file: {file.Name}, File ID: {file.Id}");
        }


    }
}
