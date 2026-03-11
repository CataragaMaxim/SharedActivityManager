// Platforms/Windows/Services/WindowsNotificationService.cs
#if WINDOWS
using Microsoft.UI.Xaml.Controls;
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Builders;
using SharedActivityManager.Models;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;

namespace SharedActivityManager.Platforms.Windows.Services
{
    public class WindowsNotificationService : INotificationService
    {
        public async Task ShowNotificationAsync(AppNotification notification)
        {
            try
            {
                // Construim notificarea Windows
                var builder = new ToastContentBuilder()
                    .AddText(notification.Title)
                    .AddText(notification.Content);

                // Adaugă butoane
                foreach (var action in notification.Actions)
                {
                    builder.AddButton(new ToastButton()
                        .SetContent(action.Text)
                        .AddArgument("action", action.Action)
                        .AddArgument("activityId", action.ActivityId));
                }

                // Setează sunetul
                if (!string.IsNullOrEmpty(notification.Sound))
                {
                    // Windows specific audio
                }

                // Afișează notificarea
                var toast = new ToastNotification(builder.GetXml());
                ToastNotificationManager.CreateToastNotifier().Show(toast);

                System.Diagnostics.Debug.WriteLine($"Windows: Showed notification '{notification.Title}'");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Windows: Error showing notification: {ex.Message}");
            }

            await Task.CompletedTask;
        }

        public async Task ShowNotificationAsync(string title, string message, string ringtone = null)
        {
            var builder = new ToastContentBuilder()
                .AddText(title)
                .AddText(message);

            var toast = new ToastNotification(builder.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toast);

            await Task.CompletedTask;
        }

        public async Task ShowAlarmNotificationAsync(Activity activity)
        {
            var director = new NotificationDirector();
            var builder = new NotificationBuilder();
            director.Builder = builder;

            director.BuildAlarmNotification(
                activity.Title,
                activity.Desc ?? "Time for your activity!",
                activity.Id,
                activity.RingTone ?? "Default"
            );

            var notification = builder.GetNotification();
            await ShowNotificationAsync(notification);
        }

        public async Task DismissNotificationAsync(int notificationId)
        {
            // Windows specific - not implemented
            await Task.CompletedTask;
        }

        public async Task DismissAllNotificationsAsync()
        {
            ToastNotificationManager.History.Clear();
            await Task.CompletedTask;
        }

        public bool AreNotificationsEnabled()
        {
            return true; // Windows specific
        }
    }
}
#endif