// Models/Activity.cs
using SharedActivityManager.Abstracts;
using SharedActivityManager.Enums;
using SQLite;

namespace SharedActivityManager.Models
{
    public class Activity : IActivity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public ActivityType TypeId { get; set; }
        public string Title { get; set; }
        public string Desc { get; set; } // ← Păstrăm Desc
        public DateTime StartTime { get; set; }
        public DateTime StartDate { get; set; }
        public bool AlarmSet { get; set; }
        public bool isCompleted { get; set; }
        public ReminderType ReminderTypeId { get; set; }
        public DateTime NextReminderDate { get; set; }
        public string RingTone { get; set; }

        // Implementarea IActivity
        [Ignore]
        public ReminderType ReminderType
        {
            get => ReminderTypeId;
            set => ReminderTypeId = value;
        }

        [Ignore]
        public bool IsCompleted
        {
            get => isCompleted;
            set => isCompleted = value;
        }

        // Metodele IActivity - implementare default
        public virtual string GetActivityDetails() => Title;
        public virtual TimeSpan GetDuration() => TimeSpan.Zero;
        public virtual bool RequiresPreparation() => false;
        public virtual string GetNotificationMessage() => $"⏰ {Title}";
        public virtual Dictionary<string, object> GetAdditionalProperties() => new();
    }
}