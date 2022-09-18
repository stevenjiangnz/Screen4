using System.IO.Compression;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using Screen.Shared;
using Screen.Utils;
using Serilog;
namespace Screen.Ticks
{
    public class TickerManager
    {
        public void LoadTickerFromEmail(SharedSettings settings, int? loadDays = null)
        {
            Log.Debug($"LoadTickerFromEmail {ObjectHelper.ToJsonString(settings)}");
            using (var client = new ImapClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.CheckCertificateRevocation = false;

                client.Connect("imap.gmail.com", 993, true);
                client.Authenticate(settings.TickerEmailAccount, settings.TickerEmailPWD);

                // The Inbox folder is always available on all IMAP servers...
                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadWrite);

                IList<UniqueId> searchResult;

                if (loadDays != null)
                {
                    searchResult = inbox.Search(SearchQuery.DeliveredAfter(DateTime.Now.AddDays(-1 * loadDays.Value)));
                } else
                {
                    searchResult = inbox.Search(SearchQuery.All);
                }

                foreach (var uid in searchResult)
                {
                    var message = inbox.GetMessage(uid);

                    Log.Debug($"Uid: {uid.ToString()}, Subject: {message.Subject},  Time: {DateTime.Now.ToLongTimeString()}" );

    
                    if (message.Subject.IndexOf("Daily Historical Data") >= 0)
                    {
                        foreach (MimeEntity attachment in message.Attachments)
                        {

                            var fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;

                            var year = "";
                            if (fileName.IndexOf("_")>0)
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
                            } else
                            {
                                Log.Warning($"File {fileName} already exists. skipped.");
                            }

                        }
                    }

                    inbox.AddFlags(uid, MessageFlags.Seen, true);
                }

                client.Disconnect(true);

                //if (fileNames.Count > 0)
                //{
                //    S3Service.S3Service service = new S3Service.S3Service();

                //    zipFileName = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".zip";
                //    var zipFilePath = localInboxZipFolder + zipFileName;

                //    ZipFile.CreateFromDirectory(localInboxFolder, zipFilePath);

                //    await service.UploadFileToS3Async(bucketName, "source/" + zipFileName, zipFilePath);
                //}
            }

        }
    }
}