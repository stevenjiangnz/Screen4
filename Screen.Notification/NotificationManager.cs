using System.Text;

namespace Screen.Notification
{
    public class NotificationManager
    {
        private string _apiKey;
        private string _apiKeySecret;

        public NotificationManager(string apiKey, string secret)
        {
            this._apiKey = apiKey;
            this._apiKeySecret = secret;
        }

        public async Task SendEmail()
        {
            string api_key = "XXXXXXXXXXXXXXXXXXXXXXXXXXX";
            string api_secret = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
            string sender_email = "stevenjiangnz@gmail.com";
            string recipient_email = "steven.jiang@shell.com";
            string subject = "Example Email - Scan Result";
            string body = "This is the content of the email.";

            string filePath = @"C:\data\Fulllist.csv";
            string fileName = "fulllist.csv";
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string fileBase64 = Convert.ToBase64String(fileBytes);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(api_key + ":" + api_secret)));

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
                            "\"TextPart\": \"" + body + "\"," +
                            "\"Attachments\": [" +
                                "{" +
                                    "\"ContentType\": \"text/csv\"," +
                                    "\"Filename\": \"" + fileName + "\"," +
                                    "\"Base64Content\": \"" + fileBase64 + "\"" +
                                "}" +
                            "]" +
                        "}" +
                    "]" +
                "}";

                StringContent content = new StringContent(payload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Email sent successfully!");
                }
                else
                {
                    Console.WriteLine("Failed to send email. Status code: " + (int)response.StatusCode);
                    Console.WriteLine("Response: " + await response.Content.ReadAsStringAsync());
                }
            }
        }
    }
}