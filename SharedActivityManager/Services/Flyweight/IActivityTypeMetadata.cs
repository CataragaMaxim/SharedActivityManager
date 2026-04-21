using SharedActivityManager.Enums;

namespace SharedActivityManager.Services.Flyweight
{
    /// <summary>
    /// Interfața Flyweight - datele comune partajate între toate activitățile de același tip
    /// </summary>
    public interface IActivityTypeMetadata
    {
        ActivityType Type { get; }
        string DisplayName { get; }
        string Icon { get; }
        string Color { get; }
        string Description { get; }
        string DefaultRingtone { get; }
        bool DefaultAlarmSet { get; }
        ReminderType DefaultReminderType { get; }

        // Metode pentru acțiuni specifice tipului
        string GetFormattedName();
        string GetNotificationTitle(string activityTitle);
        string GetStatisticsDisplay(int count);
    }
}