using System.Collections.Concurrent;
using SharedActivityManager.Enums;

namespace SharedActivityManager.Services.Flyweight
{
    /// <summary>
    /// Flyweight Factory - gestionează cache-ul și creează/returnează instanțe partajate
    /// </summary>
    public class ActivityTypeMetadataFactory
    {
        private static readonly Lazy<ActivityTypeMetadataFactory> _instance =
            new Lazy<ActivityTypeMetadataFactory>(() => new ActivityTypeMetadataFactory());

        private readonly ConcurrentDictionary<ActivityType, IActivityTypeMetadata> _cache;

        private ActivityTypeMetadataFactory()
        {
            _cache = new ConcurrentDictionary<ActivityType, IActivityTypeMetadata>();
            InitializeMetadata();
        }

        public static ActivityTypeMetadataFactory Instance => _instance.Value;

        private void InitializeMetadata()
        {
            // Înregistrează toate tipurile de activități
            RegisterMetadata(new ActivityTypeMetadata(
                ActivityType.Work,
                "Work",
                "💼",
                "#2196F3",
                "Professional tasks and projects",
                "Default Alarm",
                true,
                ReminderType.Daily
            ));

            RegisterMetadata(new ActivityTypeMetadata(
                ActivityType.Personal,
                "Personal",
                "🏠",
                "#4CAF50",
                "Personal activities and chores",
                "Default Alarm",
                false,
                ReminderType.None
            ));

            RegisterMetadata(new ActivityTypeMetadata(
                ActivityType.Health,
                "Health & Sport",
                "💪",
                "#F44336",
                "Workouts, exercises and health tracking",
                "Digital Beep",
                true,
                ReminderType.Daily
            ));

            RegisterMetadata(new ActivityTypeMetadata(
                ActivityType.Study,
                "Study",
                "📚",
                "#FF9800",
                "Learning and educational activities",
                "Gentle Wake",
                true,
                ReminderType.Weekly
            ));

            RegisterMetadata(new ActivityTypeMetadata(
                ActivityType.Other,
                "Other",
                "📝",
                "#9E9E9E",
                "Miscellaneous activities",
                "Default Alarm",
                false,
                ReminderType.None
            ));
        }

        public void RegisterMetadata(IActivityTypeMetadata metadata)
        {
            _cache[metadata.Type] = metadata;
        }

        public IActivityTypeMetadata GetMetadata(ActivityType type)
        {
            if (_cache.TryGetValue(type, out var metadata))
                return metadata;

            // Fallback pentru tipuri necunoscute
            return _cache[ActivityType.Other];
        }

        public IActivityTypeMetadata GetMetadata(string typeName)
        {
            if (Enum.TryParse<ActivityType>(typeName, out var type))
                return GetMetadata(type);
            return GetMetadata(ActivityType.Other);
        }

        public IEnumerable<IActivityTypeMetadata> GetAllMetadata()
        {
            return _cache.Values;
        }

        public int CacheSize => _cache.Count;

        public void ClearCache()
        {
            _cache.Clear();
            InitializeMetadata();
        }
    }
}