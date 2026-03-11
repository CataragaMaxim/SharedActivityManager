// Abstracts/Platforms/IAlarmService.cs
using SharedActivityManager.Models;
using ActivityModel = SharedActivityManager.Models.Activity;

namespace SharedActivityManager.Abstracts.Platforms
{
    public interface IAlarmService
    {
        Task ScheduleAlarmAsync(ActivityModel activity);
        Task ScheduleReminderAsync(ActivityModel activity);
        Task CancelAlarmAsync(int activityId);
        Task CancelAllAlarmsAsync();
        Task<bool> HasScheduledAlarmAsync(int activityId);
        Task RestoreAlarmsAsync(List<ActivityModel> activities);
        Task TriggerAlarmAsync(ActivityModel activity);
        Task StopCurrentAlarmAsync();
    }
}