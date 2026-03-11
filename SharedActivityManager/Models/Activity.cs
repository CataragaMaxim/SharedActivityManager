// Models/Activity.cs (versiunea corectată)
using System.Text.Json;
using SharedActivityManager.Abstracts;
using SharedActivityManager.Enums;
using SQLite;

namespace SharedActivityManager.Models
{
    public class Activity : IActivity, ICloneable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Proprietăți comune
        public string Title { get; set; }
        public string Desc { get; set; }
        public DateTime StartTime { get; set; }
        public bool AlarmSet { get; set; }

        // 🔥 FIX: Adaugă ambele forme pentru compatibilitate
        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set => _isCompleted = value;
        }

        // Proprietate pentru compatibilitate cu codul existent
        [Ignore]
        public bool isCompleted
        {
            get => IsCompleted;
            set => IsCompleted = value;
        }

        public string RingTone { get; set; }

        // Proprietăți pentru baza de date
        public ActivityType TypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime NextReminderDate { get; set; }

        // 🔥 FIX: Proprietăți pentru partajare
        public bool IsPublic { get; set; } = false;
        public string OwnerId { get; set; } = "current_user";
        public DateTime SharedDate { get; set; }
        public int OriginalActivityId { get; set; } = 0;

        // 🔥 FIX: Pentru ReminderType
        public int ReminderTypeValue { get; set; }

        [Ignore]
        public ReminderType ReminderType
        {
            get => (ReminderType)ReminderTypeValue;
            set => ReminderTypeValue = (int)value;
        }

        // Proprietate pentru compatibilitate
        [Ignore]
        public ReminderType ReminderTypeId
        {
            get => ReminderType;
            set => ReminderType = value;
        }

        // JSON cu date suplimentare
        public string AdditionalDataJson { get; set; }

        // ===== METODE =====
        public Activity ShallowCopy()
        {
            return (Activity)this.MemberwiseClone();
        }

        public Activity DeepCopy()
        {
            Activity clone = (Activity)this.MemberwiseClone();
            clone.Id = 0;
            clone.OriginalActivityId = this.Id;
            clone.Title = string.Copy(this.Title ?? "");
            clone.Desc = string.Copy(this.Desc ?? "");
            clone.RingTone = string.Copy(this.RingTone ?? "");

            if (!string.IsNullOrEmpty(this.AdditionalDataJson))
            {
                clone.AdditionalDataJson = string.Copy(this.AdditionalDataJson);
            }

            return clone;
        }

        public object Clone() => DeepCopy();

        public virtual string GetActivityType() => TypeId.ToString();
        public virtual string GetActivityDetails() => Title;

        public virtual Dictionary<string, object> GetAdditionalData()
        {
            return new Dictionary<string, object>();
        }

        public virtual void LoadAdditionalData(Dictionary<string, object> data) { }

        public string SerializeAdditionalData()
        {
            return JsonSerializer.Serialize(GetAdditionalData());
        }

        public void DeserializeAdditionalData(string data)
        {
            if (string.IsNullOrEmpty(data)) return;
            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(data);
                LoadAdditionalData(dict);
            }
            catch { }
        }

        // Metodele IActivity
        public virtual TimeSpan GetDuration() => TimeSpan.Zero;
        public virtual bool RequiresPreparation() => false;
        public virtual string GetNotificationMessage() => $"⏰ {Title}";
        public virtual Dictionary<string, object> GetAdditionalProperties() => new();
    }
}