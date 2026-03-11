// Builders/NotificationBuilder.cs
using SharedActivityManager.Models;

namespace SharedActivityManager.Builders
{
    public class NotificationBuilder : INotificationBuilder
    {
        private AppNotification _notification;

        public NotificationBuilder()
        {
            Reset();
        }

        public void Reset()
        {
            _notification = new AppNotification
            {
                ChannelId = "default",
                Priority = AppNotificationPriority.Normal,
                IsOngoing = false,
                AutoCancel = true
            };
        }

        public void SetTitle(string title)
        {
            _notification.Title = title;
        }

        public void SetContent(string content)
        {
            _notification.Content = content;
        }

        public void SetChannel(string channelId)
        {
            _notification.ChannelId = channelId;
        }

        public void SetPriority(AppNotificationPriority priority)
        {
            _notification.Priority = priority;
        }

        public void SetVibration(long[] vibrationPattern)
        {
            _notification.VibrationPattern = vibrationPattern;
        }

        public void SetSound(string ringtone)
        {
            _notification.Sound = ringtone;
        }

        public void AddButton(string text, string action, int activityId)
        {
            _notification.Actions.Add(new NotificationAction
            {
                Text = text,
                Action = action,
                ActivityId = activityId
            });
        }

        public void SetOngoing(bool ongoing)
        {
            _notification.IsOngoing = ongoing;
        }

        public void SetAutoCancel(bool autoCancel)
        {
            _notification.AutoCancel = autoCancel;
        }

        public void SetIcon(string iconName)
        {
            _notification.Icon = iconName;
        }

        public void SetTimeout(TimeSpan timeout)
        {
            _notification.Timeout = timeout;
        }

        public AppNotification GetNotification()
        {
            var result = _notification;
            Reset();
            return result;
        }
    }
}