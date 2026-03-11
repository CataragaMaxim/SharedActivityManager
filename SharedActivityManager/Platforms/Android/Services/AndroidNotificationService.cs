// Platforms/Android/Services/AndroidNotificationService.cs
#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Builders;
using SharedActivityManager.Models;
using Application = Android.App.Application;

namespace SharedActivityManager.Platforms.Android.Services
{
    public class AndroidNotificationService : INotificationService
    {
        private const string CHANNEL_ID = "default_channel";
        private const string CHANNEL_NAME = "Default Channel";
        private NotificationManager _notificationManager;

        public AndroidNotificationService()
        {
            _notificationManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
            CreateNotificationChannel();
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(CHANNEL_ID, CHANNEL_NAME, NotificationImportance.Default);
                _notificationManager.CreateNotificationChannel(channel);
            }
        }

        public async Task ShowNotificationAsync(AppNotification notification)
        {
            try
            {
                string channelId = !string.IsNullOrEmpty(notification.ChannelId) ? notification.ChannelId : CHANNEL_ID;

                // Creează canalul dacă nu există (pentru Android O+)
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    var channel = new NotificationChannel(channelId,
                        notification.ChannelId == "alarm_channel" ? "Alarm Channel" : "Default Channel",
                        NotificationImportance.High);
                    channel.EnableVibration(true);
                    channel.SetSound(null, null);
                    channel.LockscreenVisibility = NotificationVisibility.Public;
                    _notificationManager.CreateNotificationChannel(channel);
                }

                var builder = new Notification.Builder(Application.Context, channelId)
                    .SetContentTitle(notification.Title)
                    .SetContentText(notification.Content)
                    .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)
                    .SetAutoCancel(notification.AutoCancel)
                    .SetOngoing(notification.IsOngoing)
                    .SetVisibility(NotificationVisibility.Public);

                // Setează prioritatea
                if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                {
                    var priority = notification.Priority switch
                    {
                        AppNotificationPriority.Low => NotificationPriority.Low,
                        AppNotificationPriority.Normal => NotificationPriority.Default,
                        AppNotificationPriority.High => NotificationPriority.High,
                        AppNotificationPriority.Urgent => NotificationPriority.Max,
                        _ => NotificationPriority.Default
                    };
                    builder.SetPriority((int)priority);
                }

                // Setează vibrația
                if (notification.VibrationPattern != null)
                {
                    builder.SetVibrate(notification.VibrationPattern);
                }

                // Adaugă butoane de acțiune
                foreach (var action in notification.Actions)
                {
                    var intent = new Intent(action.Action);
                    intent.PutExtra("activity_id", action.ActivityId);
                    var pendingIntent = PendingIntent.GetBroadcast(
                        Application.Context,
                        notification.Id + action.GetHashCode(),
                        intent,
                        PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent
                    );
                    builder.AddAction(0, action.Text, pendingIntent);
                }

                _notificationManager.Notify(notification.Id, builder.Build());

                System.Diagnostics.Debug.WriteLine($"Android: Showed notification '{notification.Title}' with ID {notification.Id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Android: Error showing notification: {ex.Message}");
            }

            await Task.CompletedTask;
        }

        public async Task ShowNotificationAsync(string title, string message, string ringtone = null)
        {
            var builder = new Notification.Builder(Application.Context, CHANNEL_ID)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)
                .SetAutoCancel(true);

            _notificationManager.Notify(new Random().Next(1000, 9999), builder.Build());
            await Task.CompletedTask;
        }

        // 🔥 FIX: Folosește activity.Desc, nu activity.Description
        public async Task ShowAlarmNotificationAsync(Models.Activity activity)
        {
            var director = new NotificationDirector();
            var builder = new NotificationBuilder();
            director.Builder = builder;

            director.BuildAlarmNotification(
                activity.Title,
                activity.Desc ?? "Time for your activity!",  // ← FIX: Desc în loc de Description
                activity.Id,
                activity.RingTone ?? "Default"
            );

            var notification = builder.GetNotification();
            await ShowNotificationAsync(notification);
        }

        public async Task DismissNotificationAsync(int notificationId)
        {
            _notificationManager.Cancel(notificationId);
            await Task.CompletedTask;
        }

        public async Task DismissAllNotificationsAsync()
        {
            _notificationManager.CancelAll();
            await Task.CompletedTask;
        }

        public bool AreNotificationsEnabled()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = _notificationManager.GetNotificationChannel(CHANNEL_ID);
                // 🔥 FIX: Folosește Importance, nu GetImportance()
                return channel?.Importance != NotificationImportance.None;
            }
            else
            {
#pragma warning disable CS0618 // Tipul sau membrul este învechit
                return _notificationManager.AreNotificationsEnabled();
#pragma warning restore CS0618
            }
        }
    }
}
#endif