// Abstracts/IActivity.cs
using SharedActivityManager.Enums;

namespace SharedActivityManager.Abstracts
{
    public interface IActivity
    {
        int Id { get; set; }
        string Title { get; set; }
        string Desc { get; set; }
        DateTime StartTime { get; set; }
        bool AlarmSet { get; set; }
        bool IsCompleted { get; set; }
        ReminderType ReminderType { get; set; }
        string RingTone { get; set; }

        // Metode specifice pentru diferite tipuri de activități
        string GetActivityDetails();
        TimeSpan GetDuration();
        bool RequiresPreparation();
        string GetNotificationMessage();
        Dictionary<string, object> GetAdditionalProperties();
    }
}