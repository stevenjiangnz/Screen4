using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Screen.Notification;

namespace Screen.Test.Notification
{
    public class TestNotificationManager
    {
        [Fact]
        public async Task TestSendEmail()
        {
            var notficationManager = new NotificationManager("", "");

            await notficationManager.SendEmail();
        }
    }
}
