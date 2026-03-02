#if ANDROID
using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Models;
using Application = Android.App.Application;
using ActivityModel = SharedActivityManager.Models.Activity;

namespace SharedActivityManager.Services
{
    public class AndroidNotificationService : INotificationService
    {
        private const string CHANNEL_ID = "activity_alarm_channel";
        private const string CHANNEL_NAME = "Activity Alarms";

        public AndroidNotificationService()
        {
            CreateNotificationChannel();
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(CHANNEL_ID, CHANNEL_NAME, NotificationImportance.High)
                {
                    Description = "Channel for activity alarms"
                };
                channel.EnableVibration(true);
                channel.SetSound(null, null);

                var notificationManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }
        }

        public async Task ShowNotificationAsync(string title, string message, string ringtone = null)
        {
            try
            {
                var intent = new Intent(Application.Context, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);

                var pendingIntent = PendingIntent.GetActivity(
                    Application.Context,
                    0,
                    intent,
                    PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent
                );

                var builder = new Notification.Builder(Application.Context, CHANNEL_ID)
                    .SetContentTitle(title)
                    .SetContentText(message)
                    .SetSmallIcon(global::Android.Resource.Drawable.IcDialogAlert)
                    .SetAutoCancel(true)
                    .SetContentIntent(pendingIntent)
                    .SetPriority((int)NotificationPriority.High)
                    .SetVibrate(new long[] { 0, 500, 1000 });

                var notificationManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
                notificationManager.Notify(new Random().Next(), builder.Build());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing notification: {ex.Message}");
            }
        }

        public async Task ShowAlarmNotificationAsync(ActivityModel activity)
        {
            try
            {
                await ShowNotificationAsync(
                    $"⏰ {activity.Title}",
                    activity.Desc ?? "Time for your activity!",
                    activity.RingTone
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing alarm notification: {ex.Message}");
            }
        }

        public async Task DismissNotificationAsync(int notificationId)
        {
            try
            {
                var notificationManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
                notificationManager.Cancel(notificationId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error dismissing notification: {ex.Message}");
            }
        }

        public async Task DismissAllNotificationsAsync()
        {
            try
            {
                var notificationManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
                notificationManager.CancelAll();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error dismissing all notifications: {ex.Message}");
            }
        }

        public bool AreNotificationsEnabled()
        {
            try
            {
                var notificationManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                {
                    return notificationManager.AreNotificationsEnabled();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
#endif