using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Memento
{
    /// <summary>
    /// Memento concret - stochează o copie completă a listei de activități
    /// </summary>
    public class ActivityMemento : IActivityMemento
    {
        private readonly List<Activity> _activities;
        private readonly DateTime _timestamp;
        private readonly string _description;

        public string Name => $"{_timestamp:yyyy-MM-dd HH:mm:ss} - {_description}";
        public DateTime Timestamp => _timestamp;
        public int ActivityCount => _activities.Count;
        public string Description => _description;

        public ActivityMemento(List<Activity> activities, string description)
        {
            _activities = DeepCopyActivities(activities);
            _timestamp = DateTime.Now;
            _description = description;
        }

        public List<Activity> GetActivities()
        {
            return DeepCopyActivities(_activities);
        }

        /// <summary>
        /// Creează o copie profundă a listei de activități
        /// </summary>
        private List<Activity> DeepCopyActivities(List<Activity> source)
        {
            if (source == null) return new List<Activity>();

            var copy = new List<Activity>();
            foreach (var activity in source)
            {
                copy.Add(activity.DeepCopy());
            }
            return copy;
        }

        /// <summary>
        /// Returnează informații despre snapshot pentru afișare
        /// </summary>
        public string GetInfo()
        {
            var completed = _activities.Count(a => a.IsCompleted);
            return $"{_timestamp:HH:mm:ss} - {_description}\n" +
                   $"   📊 {ActivityCount} activities, {completed} completed";
        }
    }
}