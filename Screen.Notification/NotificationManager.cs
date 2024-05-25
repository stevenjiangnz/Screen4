using Microsoft.Extensions.Logging;
using System.Text;

namespace Screen.Notification
{
    public class NotificationManager
    {
        private string _apiKey;
        private string _apiKeySecret;
        private readonly ILogger _log;

        public NotificationManager(string apiKey, string secret, ILogger logger)
        {
            this._apiKey = apiKey;
            this._apiKeySecret = secret;
            this._log = logger;
        }

        public bool ShouldNotifyExtraRecipient()
        {
            var notify = Environment.GetEnvironmentVariable("NOTIFY_EXTRA_RECIPIENT");
            return notify != null && ("true".Equals(notify, StringComparison.OrdinalIgnoreCase) ||
                                      "1".Equals(notify) ||
                                      "yes".Equals(notify, StringComparison.OrdinalIgnoreCase));
        }

        public async Task SendNotificationEmail(string sender, string recipients, string subject, string csvContent)
        {
            string sender_email = sender;
            string body = csvContent.Replace("\r\n", "<br/>").Replace("\n", "<br/>"); // HTML formatted body

            // Convert the original CSV content to Base64 for the attachment
            byte[] csvBytes = Encoding.UTF8.GetBytes(csvContent);
            string csvBase64 = Convert.ToBase64String(csvBytes);

            // Split the recipients into individual emails
            var recipientEmails = recipients.Split(';');
            StringBuilder recipientJson = new StringBuilder();
            foreach (var email in recipientEmails)
            {
                if (!string.IsNullOrWhiteSpace(email))
                {
                    recipientJson.Append($"{{\"Email\": \"{email.Trim()}\"}},");
                }
            }
            if (recipientJson.Length > 0)
                recipientJson.Length--; // Remove the trailing comma

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(this._apiKey + ":" + this._apiKeySecret)));

                string url = "https://api.mailjet.com/v3.1/send";

                string payload = "{" +
                    "\"Messages\": [" +
                        "{" +
                            "\"From\": {" +
                                "\"Email\": \"" + sender_email + "\"" +
                            "}," +
                            "\"To\": [" + recipientJson + "]," +
                            "\"Subject\": \"" + subject + "\"," +
                            "\"HTMLPart\": \"" + body + "\"," +
                            "\"Attachments\": [" +
                                "{" +
                                    "\"ContentType\": \"text/csv\"," +
                                    "\"Filename\": \"data.csv\"," +
                                    "\"Base64Content\": \"" + csvBase64 + "\"" +
                                "}" +
                            "]" +
                        "}" +
                    "]" +
                "}";

                _log.LogInformation("Payload to be sent:\n" + payload);

                StringContent emailContent = new StringContent(payload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, emailContent);

                if (response.IsSuccessStatusCode)
                {
                    _log.LogInformation("Email sent successfully!");
                }
                else
                {
                    _log.LogError("Failed to send email. Status code: " + (int)response.StatusCode);
                    _log.LogError("Response: " + await response.Content.ReadAsStringAsync());

                    throw new Exception("Error in sending email...");
                }
            }
        }
    }
}