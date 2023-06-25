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


        public async Task SendNotificationEmail(string sender, string recipients, string subject, string content)
        {
            string sender_email = sender;
            string recipient_email = recipients;
            string body = content.Replace("\r\n", "<br/>").Replace("\n", "<br/>");

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
                            "\"To\": [" +
                                "{" +
                                    "\"Email\": \"" + recipient_email + "\"" +
                                "}" +
                            "]," +
                            "\"Subject\": \"" + subject + "\"," +
                            "\"HTMLPart\": \"" + body + "\"" +
                        "}" +
                    "]" +
                "}";

                StringContent emailContent = new StringContent(payload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, emailContent);

                if (response.IsSuccessStatusCode)
                {
                    this._log.LogInformation("Email sent successfully!");
                }
                else
                {
                    this._log.LogError("Failed to send email. Status code: " + (int)response.StatusCode);
                    this._log.LogError("Response: " + await response.Content.ReadAsStringAsync());

                    throw new Exception("Error in sending email...");
                }
            }
        }


    }
}