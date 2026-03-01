// Services/PlatformAlarmService.cs
using SharedActivityManager.Models;

namespace SharedActivityManager.Services
{
    public class PlatformAlarmService : IAlarmService
    {
        private IAlarmService _platformService;

        public PlatformAlarmService()
        {
#if ANDROID
            _platformService = new SharedActivityManager.Platforms.Android.AlarmService();
#elif WINDOWS
            _platformService = new SharedActivityManager.Platforms.Windows.WindowsAlarmService();
#else
            _platformService = new FallbackAlarmService();
#endif
        }

        public async Task ScheduleAlarmAsync(Activity activity)
            => await _platformService.ScheduleAlarmAsync(activity);

        public async Task CancelAlarmAsync(int activityId)
            => await _platformService.CancelAlarmAsync(activityId);

        public async Task CancelAllAlarmsAsync()
            => await _platformService.CancelAllAlarmsAsync();

        public async Task<bool> HasScheduledAlarmAsync(int activityId)
            => await _platformService.HasScheduledAlarmAsync(activityId);

        public async Task RestoreAlarmsAsync(List<Activity> activities)
            => await _platformService.RestoreAlarmsAsync(activities);

        public async Task TriggerAlarmAsync(Activity activity)
            => await _platformService.TriggerAlarmAsync(activity);

        public async Task StopCurrentAlarmAsync()
            => await _platformService.StopCurrentAlarmAsync();
    }

    public class FallbackAlarmService : IAlarmService
    {
        public Task ScheduleAlarmAsync(Activity activity) => Task.CompletedTask;
        public Task CancelAlarmAsync(int activityId) => Task.CompletedTask;
        public Task CancelAllAlarmsAsync() => Task.CompletedTask;
        public Task<bool> HasScheduledAlarmAsync(int activityId) => Task.FromResult(false);
        public Task RestoreAlarmsAsync(List<Activity> activities) => Task.CompletedTask;
        public Task TriggerAlarmAsync(Activity activity) => Task.CompletedTask;
        public Task StopCurrentAlarmAsync() => Task.CompletedTask;
    }
}