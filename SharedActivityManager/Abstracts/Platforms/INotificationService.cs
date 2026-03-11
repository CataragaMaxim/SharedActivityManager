// Abstracts/Platforms/INotificationService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedActivityManager.Models;

namespace SharedActivityManager.Abstracts.Platforms
{
    public interface INotificationService
    {
        // Metodele existente
        Task ShowNotificationAsync(string title, string message, string ringtone = null);
        Task ShowAlarmNotificationAsync(Activity activity);
        Task DismissNotificationAsync(int notificationId);
        Task DismissAllNotificationsAsync();
        bool AreNotificationsEnabled();

        // Metodă nouă pentru a lucra cu builder-ul
        Task ShowNotificationAsync(AppNotification notification);
    }
}