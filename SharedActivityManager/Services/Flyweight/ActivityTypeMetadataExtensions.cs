using SharedActivityManager.Enums;
using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Flyweight
{
    public static class ActivityTypeMetadataExtensions
    {
        private static readonly ActivityTypeMetadataFactory _factory = ActivityTypeMetadataFactory.Instance;

        public static IActivityTypeMetadata GetMetadata(this ActivityType type)
        {
            return _factory.GetMetadata(type);
        }

        public static IActivityTypeMetadata GetMetadata(this Activity activity)
        {
            return _factory.GetMetadata(activity.TypeId);
        }

        public static string GetTypeIcon(this Activity activity)
        {
            return activity.GetMetadata().Icon;
        }

        public static string GetTypeColor(this Activity activity)
        {
            return activity.GetMetadata().Color;
        }

        public static string GetFormattedTypeName(this Activity activity)
        {
            return activity.GetMetadata().GetFormattedName();
        }

        public static void ApplyDefaultSettings(this Activity activity)
        {
            var metadata = activity.GetMetadata();
            activity.RingTone = metadata.DefaultRingtone;
            activity.AlarmSet = metadata.DefaultAlarmSet;
            activity.ReminderType = metadata.DefaultReminderType;
        }
    }
}