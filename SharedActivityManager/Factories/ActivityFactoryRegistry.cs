// Factories/ActivityFactoryRegistry.cs
using SharedActivityManager.Enums;

namespace SharedActivityManager.Factories
{
    public static class ActivityFactoryRegistry
    {
        private static readonly Dictionary<ActivityType, Func<ActivityCreator>> _factories = new();

        static ActivityFactoryRegistry()
        {
            // Înregistrează fabricile disponibile
            RegisterFactory(ActivityType.Work, () => new WorkActivityCreator());
            RegisterFactory(ActivityType.Personal, () => new PersonalActivityCreator());
            RegisterFactory(ActivityType.Health, () => new HealthActivityCreator());
            RegisterFactory(ActivityType.Study, () => new StudyActivityCreator());

            // Factory default pentru Other
            RegisterFactory(ActivityType.Other, () => new WorkActivityCreator()); // sau orice alt default
        }

        public static void RegisterFactory(ActivityType type, Func<ActivityCreator> factoryCreator)
        {
            _factories[type] = factoryCreator;
        }

        public static ActivityCreator GetCreator(ActivityType type)
        {
            if (_factories.TryGetValue(type, out var factoryCreator))
            {
                return factoryCreator();
            }

            // Default fallback
            return new WorkActivityCreator();
        }

        public static IEnumerable<ActivityType> GetSupportedTypes()
        {
            return _factories.Keys;
        }
    }
}