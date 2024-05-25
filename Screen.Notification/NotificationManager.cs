using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

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
            // Clean and convert the CSV content
            string body = csvContent.Replace("\r\n", "<br/>").Replace("\n", "<br/>");
            byte[] csvBytes = Encoding.UTF8.GetBytes(csvContent);
            string csvBase64 = Convert.ToBase64String(csvBytes);

            // Prepare the recipient list
            var recipientEmails = recipients.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(email => email.Trim())
                                             .Where(email => !string.IsNullOrEmpty(email))
                                             .Distinct()
                                             .Select(email => new { Email = email });

            // Construct the JSON payload using System.Text.Json
            var message = new
            {
                From = new { Email = sender },
                To = recipientEmails,
                Subject = subject,
                HTMLPart = body,
                Attachments = new[]
                {
            new { ContentType = "text/csv", Filename = "data.csv", Base64Content = csvBase64 }
        }
            };

            var payload = new { Messages = new[] { message } };

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(this._apiKey + ":" + this._apiKeySecret)));
                string url = "https://api.mailjet.com/v3.1/send";

                // Serialize payload to JSON
                var jsonContent = JsonSerializer.Serialize(payload);
                _log.LogInformation("Payload to be sent:\n" + jsonContent);

                using var emailContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, emailContent);

                if (response.IsSuccessStatusCode)
                {
                    _log.LogInformation("Email sent successfully!");
                }
                else
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    _log.LogError($"Failed to send email. Status code: {(int)response.StatusCode}");
                    _log.LogError("Response: " + responseContent);

                    throw new Exception("Error in sending email...");
                }
            }
        }
    }
}