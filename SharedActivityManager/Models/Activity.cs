// Models/Activity.cs
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
        public bool IsCompleted { get; set; }
        public string RingTone { get; set; }

        // Proprietăți pentru baza de date
        public ActivityType TypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime NextReminderDate { get; set; }

        // 🔥 NOU: Proprietăți pentru partajare
        public bool IsPublic { get; set; } = false; // true = visible to friends
        public string OwnerId { get; set; } = "current_user"; // Default user
        public DateTime SharedDate { get; set; }
        public int OriginalActivityId { get; set; } = 0; // 0 = original, >0 = copy

        // Pentru ReminderType - stocăm ca int în DB, dar expunem ca enum
        public int ReminderTypeValue { get; set; }

        [Ignore]
        public ReminderType ReminderType
        {
            get => (ReminderType)ReminderTypeValue;
            set => ReminderTypeValue = (int)value;
        }

        // JSON cu date suplimentare
        public string AdditionalDataJson { get; set; }

        // ===== PROTOTYPE PATTERN IMPLEMENTATION =====

        /// <summary>
        /// Shallow Copy - copiază doar referințele (obiectul nou și cel vechi partajează datele)
        /// </summary>
        public Activity ShallowCopy()
        {
            return (Activity)this.MemberwiseClone();
        }

        /// <summary>
        /// Deep Copy - creează o copie complet independentă
        /// </summary>
        public Activity DeepCopy()
        {
            // Facem shallow copy inițial
            Activity clone = (Activity)this.MemberwiseClone();

            // Resetăm ID-ul pentru a fi o activitate nouă
            clone.Id = 0;

            // Păstrăm referința către activitatea originală
            clone.OriginalActivityId = this.Id;

            // Copiem string-urile (sunt reference types)
            clone.Title = string.Copy(this.Title ?? "");
            clone.Desc = string.Copy(this.Desc ?? "");
            clone.RingTone = string.Copy(this.RingTone ?? "");

            // Copiem și AdditionalDataJson dacă există
            if (!string.IsNullOrEmpty(this.AdditionalDataJson))
            {
                clone.AdditionalDataJson = string.Copy(this.AdditionalDataJson);
            }

            // Datele de partajare
            clone.IsPublic = this.IsPublic;
            clone.OwnerId = string.Copy(this.OwnerId ?? "current_user");
            clone.SharedDate = DateTime.Now;

            return clone;
        }

        /// <summary>
        /// Creează o copie pentru partajare (Deep Copy cu setări specifice)
        /// </summary>
        public Activity CreateSharedCopy(string sharedBy)
        {
            var clone = this.DeepCopy();
            clone.OwnerId = sharedBy;
            clone.IsPublic = true;
            clone.SharedDate = DateTime.Now;
            clone.AlarmSet = false; // Activitatea partajată nu are alarmă implicit
            return clone;
        }

        /// <summary>
        /// Implementare ICloneable pentru compatibilitate
        /// </summary>
        public object Clone()
        {
            return this.DeepCopy();
        }

        // Metode din IActivity
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

        // Metodele IActivity - implementare default
        public virtual TimeSpan GetDuration() => TimeSpan.Zero;
        public virtual bool RequiresPreparation() => false;
        public virtual string GetNotificationMessage() => $"⏰ {Title}";
        public virtual Dictionary<string, object> GetAdditionalProperties() => new();
    }
}