using SharedActivityManager.Enums;
using SharedActivityManager.Models;

namespace SharedActivityManager.Factories
{
    public abstract class ActivityCreator
    {
        public abstract Activity CreateActivity();

        public virtual Activity CreateActivity(
            string title,
            string desc,
            DateTime startTime,
            bool alarmSet = true,
            string ringTone = "Default")
        {
            var activity = CreateActivity();
            activity.Title = title;
            activity.Desc = desc;
            activity.StartTime = startTime;
            activity.AlarmSet = alarmSet;
            activity.RingTone = ringTone;
            activity.IsCompleted = false;
            activity.StartDate = startTime.Date;

            return activity;
        }

        public virtual async Task<Activity> CreateAndConfigureActivity(
            string title,
            string desc,
            DateTime startTime,
            Dictionary<string, object> additionalParams = null)
        {
            var activity = CreateActivity(title, desc, startTime);
            ConfigureSpecificProperties(activity, additionalParams);
            SetDefaultReminder(activity);

            if (!ValidateActivity(activity))
                throw new InvalidOperationException("Invalid activity configuration");

            await OnActivityCreated(activity);

            return activity;
        }

        protected virtual void ConfigureSpecificProperties(Activity activity, Dictionary<string, object> additionalParams) { }
        protected virtual void SetDefaultReminder(Activity activity) { }
        protected virtual ReminderType GetDefaultReminderType() => ReminderType.None;
        protected virtual bool ValidateActivity(Activity activity) => !string.IsNullOrWhiteSpace(activity.Title);
        protected virtual async Task OnActivityCreated(Activity activity) => await Task.CompletedTask;

        protected T GetParamValue<T>(Dictionary<string, object> dict, string key, T defaultValue = default)
        {
            if (dict != null && dict.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return defaultValue;
        }
    }
}