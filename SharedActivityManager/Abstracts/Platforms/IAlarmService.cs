using SharedActivityManager.Models;

namespace SharedActivityManager.Abstracts.Platforms
{
    public interface IAlarmService
    {
        Task ScheduleAlarmAsync(Activity activity);
        Task CancelAlarmAsync(int activityId);
        Task CancelAllAlarmsAsync();
        Task<bool> HasScheduledAlarmAsync(int activityId);
        Task RestoreAlarmsAsync(List<Activity> activities);
        Task TriggerAlarmAsync(Activity activity);
        Task StopCurrentAlarmAsync();
    }
}