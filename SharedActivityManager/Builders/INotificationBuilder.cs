// Builders/INotificationBuilder.cs
using SharedActivityManager.Models;

namespace SharedActivityManager.Builders
{
    public interface INotificationBuilder
    {
        void Reset();
        void SetTitle(string title);
        void SetContent(string content);
        void SetChannel(string channelId);
        void SetPriority(AppNotificationPriority priority);
        void SetVibration(long[] vibrationPattern);
        void SetSound(string ringtone);
        void AddButton(string text, string action, int activityId);
        void SetOngoing(bool ongoing);
        void SetAutoCancel(bool autoCancel);
        void SetIcon(string iconName);
        void SetTimeout(TimeSpan timeout);
        AppNotification GetNotification();
    }
}