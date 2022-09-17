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
        public void LoadTickerFromEmail(SharedSettings settings)
        {
            Log.Debug($"LoadTickerFromEmail {ObjectHelper.ToJsonString(settings)}");
            using (var client = new ImapClient())
            {
                client.Connect("imap.gmail.com", 993, true);
                client.Authenticate(settings.TickerEmailAccount, settings.TickerEmailPWD);

                // The Inbox folder is always available on all IMAP servers...
                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadWrite);
                foreach (var uid in inbox.Search(SearchQuery.All))
                {
                    var message = inbox.GetMessage(uid);

                    Console.WriteLine($"Uid: {uid.ToString()}, Subject: {message.Subject},  Time: {DateTime.Now.ToLongTimeString()}" );

                    //if (message.Subject.IndexOf("Daily Historical Data") >= 0)
                    //{
                    //    foreach (MimeEntity attachment in message.Attachments)
                    //    {
                    //        //var fileName = localInboxFolder + attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
                    //        var fileName = "c:\\data" + attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;

                    //        using (var stream = File.Create(fileName))
                    //        {
                    //            if (attachment is MessagePart)
                    //            {
                    //                var rfc822 = (MessagePart)attachment;
                    //                rfc822.Message.WriteTo(stream);
                    //            }
                    //            else
                    //            {
                    //                var part = (MimePart)attachment;
                    //                part.Content.DecodeTo(stream);
                    //            }
                    //        }
                    //        //fileNames.Add(fileName);
                    //    }
                    //}

                    //inbox.AddFlags(uid, MessageFlags.Seen, true);
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