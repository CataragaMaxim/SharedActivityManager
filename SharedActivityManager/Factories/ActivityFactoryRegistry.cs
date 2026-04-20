using SharedActivityManager.Enums;

namespace SharedActivityManager.Factories
{
    public static class ActivityFactoryRegistry
    {
        private static readonly Dictionary<ActivityType, Func<ActivityCreator>> _factories = new();

        static ActivityFactoryRegistry()
        {
            RegisterFactory(ActivityType.Work, () => new WorkActivityCreator());
            RegisterFactory(ActivityType.Personal, () => new ShoppingActivityCreator());
            RegisterFactory(ActivityType.Health, () => new SportActivityCreator());
            RegisterFactory(ActivityType.Study, () => new StudyActivityCreator());
            RegisterFactory(ActivityType.Other, () => new WorkActivityCreator());
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
            return new WorkActivityCreator();
        }

        public static IEnumerable<ActivityType> GetSupportedTypes()
        {
            return _factories.Keys;
        }

        // Metodă pentru a obține descrierea funcționalităților fiecărui tip
        public static string GetTypeDescription(ActivityType type)
        {
            return type switch
            {
                ActivityType.Work => "📋 Work - Timer, Priority, Deadline, Tags",
                ActivityType.Personal => "🛒 Shopping - Budget, Items list, Store",
                ActivityType.Health => "🏃 Sport - Timer, Repetitions, Calories, Distance",
                ActivityType.Study => "📚 Study - Video player, Notes, Quiz",
                _ => "📝 Other - Basic activity"
            };
        }
    }
}