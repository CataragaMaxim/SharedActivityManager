// ViewModels/ActivityFormModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using SharedActivityManager.Enums;
using SharedActivityManager.Models;

public partial class ActivityFormModel : ObservableObject
{
    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string description;

    [ObservableProperty]
    private ActivityType activityType = ActivityType.Other;

    [ObservableProperty]
    private DateTime startDate = DateTime.Today;

    [ObservableProperty]
    private TimeSpan startTime = DateTime.Now.TimeOfDay;

    [ObservableProperty]
    private bool alarmSet;

    [ObservableProperty]
    private ReminderType reminderType = ReminderType.None;

    [ObservableProperty]
    private string ringTone = "Default Alarm";

    [ObservableProperty]
    private RingtoneProj ringtoneObject;

    [ObservableProperty]
    private bool isPublic;

    public DateTime CombinedStartDateTime => StartDate.Add(StartTime);
}