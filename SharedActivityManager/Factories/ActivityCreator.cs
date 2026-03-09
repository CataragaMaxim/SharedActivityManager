// Factories/ActivityCreator.cs
using SharedActivityManager.Enums;
using SharedActivityManager.Models;

public abstract class ActivityCreator
{
    // Factory Method principal - returnează Activity
    public abstract Activity CreateActivity();

    // Factory Method pentru crearea cu parametri
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
        activity.isCompleted = false;

        return activity;
    }

    // Template Method
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

    protected virtual void SetDefaultReminder(Activity activity)
    {
        activity.ReminderTypeId = GetDefaultReminderType();
    }

    protected virtual ReminderType GetDefaultReminderType() => ReminderType.None;

    protected virtual bool ValidateActivity(Activity activity)
    {
        return !string.IsNullOrWhiteSpace(activity.Title);
    }

    protected virtual async Task OnActivityCreated(Activity activity)
    {
        System.Diagnostics.Debug.WriteLine($"Activity created: {activity.Title}");
        await Task.CompletedTask;
    }

    protected T GetParamValue<T>(Dictionary<string, object> dict, string key, T defaultValue = default)
    {
        if (dict != null && dict.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;
        return defaultValue;
    }
}