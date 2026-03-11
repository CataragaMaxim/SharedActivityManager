// Models/AppNotification.cs (fostul Notification.cs)
using System.Text;

namespace SharedActivityManager.Models
{
    public class AppNotification
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string ChannelId { get; set; }
        public AppNotificationPriority Priority { get; set; }
        public long[] VibrationPattern { get; set; }
        public string Sound { get; set; }
        public List<NotificationAction> Actions { get; set; } = new();
        public bool IsOngoing { get; set; }
        public bool AutoCancel { get; set; }
        public string Icon { get; set; }
        public TimeSpan? Timeout { get; set; }
        public int Id { get; set; } = new Random().Next(1000, 9999);

        public string GetDescription()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Notification: {Title}");
            sb.AppendLine($"Content: {Content}");
            sb.AppendLine($"Channel: {ChannelId}");
            sb.AppendLine($"Priority: {Priority}");
            sb.AppendLine($"Actions: {Actions.Count}");
            sb.AppendLine($"Ongoing: {IsOngoing}");
            return sb.ToString();
        }
    }

    public class NotificationAction
    {
        public string Text { get; set; }
        public string Action { get; set; }
        public int ActivityId { get; set; }
    }

    public enum AppNotificationPriority
    {
        Low,
        Normal,
        High,
        Urgent
    }
}