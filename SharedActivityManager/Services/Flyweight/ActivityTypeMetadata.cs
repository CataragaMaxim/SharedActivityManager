using SharedActivityManager.Enums;

namespace SharedActivityManager.Services.Flyweight
{
    /// <summary>
    /// Implementare concretă a Flyweight - conține datele comune pentru un tip de activitate
    /// </summary>
    public class ActivityTypeMetadata : IActivityTypeMetadata
    {
        public ActivityType Type { get; }
        public string DisplayName { get; }
        public string Icon { get; }
        public string Color { get; }
        public string Description { get; }
        public string DefaultRingtone { get; }
        public bool DefaultAlarmSet { get; }
        public ReminderType DefaultReminderType { get; }

        public ActivityTypeMetadata(
            ActivityType type,
            string displayName,
            string icon,
            string color,
            string description,
            string defaultRingtone = "Default Alarm",
            bool defaultAlarmSet = true,
            ReminderType defaultReminderType = ReminderType.None)
        {
            Type = type;
            DisplayName = displayName;
            Icon = icon;
            Color = color;
            Description = description;
            DefaultRingtone = defaultRingtone;
            DefaultAlarmSet = defaultAlarmSet;
            DefaultReminderType = defaultReminderType;
        }

        public string GetFormattedName()
        {
            return $"{Icon} {DisplayName}";
        }

        public string GetNotificationTitle(string activityTitle)
        {
            return $"{Icon} {activityTitle} ({DisplayName})";
        }

        public string GetStatisticsDisplay(int count)
        {
            return $"{Icon} {DisplayName}: {count} activities";
        }
    }
}