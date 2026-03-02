using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Models;

namespace SharedActivityManager.Services
{
    internal class WindowsNotificationService : INotificationService
    {
        public bool AreNotificationsEnabled()
        {
            throw new NotImplementedException();
        }

        public Task DismissAllNotificationsAsync()
        {
            throw new NotImplementedException();
        }

        public Task DismissNotificationAsync(int notificationId)
        {
            throw new NotImplementedException();
        }

        public Task ShowAlarmNotificationAsync(Activity activity)
        {
            throw new NotImplementedException();
        }

        public Task ShowNotificationAsync(string title, string message, string ringtone = null)
        {
            throw new NotImplementedException();
        }
    }
}
