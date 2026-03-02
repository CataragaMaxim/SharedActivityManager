using SharedActivityManager.Models;
using CommunityToolkit.Maui.Views;
using SharedActivityManager.Abstracts.Platforms;

namespace SharedActivityManager.Services
{
    public class AlarmService : IAlarmService
    {
        private readonly Dictionary<int, System.Timers.Timer> _alarmTimers;
        private MediaElement? _mediaPlayer; // Permite null
        private bool _isAlarmPlaying;

        public AlarmService()
        {
            _alarmTimers = new Dictionary<int, System.Timers.Timer>();
            _mediaPlayer = new MediaElement
            {
                ShouldAutoPlay = true,
                ShouldKeepScreenOn = true,
                Volume = 1.0
            };
            _mediaPlayer.MediaEnded += OnAlarmFinished;
        }

        public async Task ScheduleAlarmAsync(Activity activity)
        {
            try
            {
                await CancelAlarmAsync(activity.Id);

                var alarmTime = activity.StartTime;
                var now = DateTime.Now;

                if (alarmTime <= now)
                    return;

                var timeUntilAlarm = alarmTime - now;

                var timer = new System.Timers.Timer(timeUntilAlarm.TotalMilliseconds);
                timer.Elapsed += async (sender, e) => await OnAlarmTriggered(activity);
                timer.AutoReset = false;
                timer.Enabled = true;

                _alarmTimers[activity.Id] = timer;
                await SaveAlarmToPreferences(activity);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error scheduling alarm: {ex.Message}");
            }
        }

        public async Task CancelAlarmAsync(int activityId)
        {
            try
            {
                if (_alarmTimers.TryGetValue(activityId, out var timer))
                {
                    timer.Stop();
                    timer.Dispose();
                    _alarmTimers.Remove(activityId);
                    await RemoveAlarmFromPreferences(activityId);
                    System.Diagnostics.Debug.WriteLine($"Alarm cancelled for activity {activityId}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cancelling alarm: {ex.Message}");
            }
        }

        public async Task CancelAllAlarmsAsync()
        {
            foreach (var timer in _alarmTimers.Values)
            {
                timer.Stop();
                timer.Dispose();
            }
            _alarmTimers.Clear();
            Preferences.Remove("ScheduledAlarms");
            await StopCurrentAlarmAsync();
        }

        public Task<bool> HasScheduledAlarmAsync(int activityId)
        {
            return Task.FromResult(_alarmTimers.ContainsKey(activityId));
        }

        public async Task TriggerAlarmAsync(Activity activity)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"ALARM TRIGGERED for: {activity.Title}");
                await ShowAlarmNotification(activity);
                await PlayAlarmSoundAsync(activity.RingTone);
                await CancelAlarmAsync(activity.Id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error triggering alarm: {ex.Message}");
            }
        }

        public async Task StopCurrentAlarmAsync()
        {
            try
            {
                _mediaPlayer?.Stop();
                _isAlarmPlaying = false;
                await CloseAlarmPage();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping alarm: {ex.Message}");
            }
        }

        public async Task RestoreAlarmsAsync(List<Activity> activities)
        {
            try
            {
                var savedAlarms = GetSavedAlarms();
                foreach (var activity in activities)
                {
                    if (savedAlarms.ContainsKey(activity.Id) && activity.AlarmSet && !activity.isCompleted)
                    {
                        if (activity.StartTime > DateTime.Now)
                        {
                            await ScheduleAlarmAsync(activity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring alarms: {ex.Message}");
            }
        }

        private async Task OnAlarmTriggered(Activity activity)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await TriggerAlarmAsync(activity);
            });
        }

        private async Task ShowAlarmNotification(Activity activity)
        {
            var alarmPage = new AlarmNotificationPage(
                activity.Title,
                activity.Desc,
                activity.RingTone ?? "Default"
            );
            await Application.Current.MainPage.Navigation.PushModalAsync(alarmPage);
        }

        private async Task PlayAlarmSoundAsync(string ringtoneName)
        {
            try
            {
                var ringtoneService = new RingtoneService();
                var ringtones = ringtoneService.GetAvailableRingtones();
                var ringtone = ringtones.FirstOrDefault(r =>
                    r.DisplayName == ringtoneName || r.Title == ringtoneName);

                if (ringtone != null)
                {
                    var filePath = Path.Combine(FileSystem.AppDataDirectory, "Ringtones", ringtone.FileName);

                    if (File.Exists(filePath))
                    {
                        _mediaPlayer.Source = MediaSource.FromFile(filePath);
                        _mediaPlayer.Play();
                        _isAlarmPlaying = true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing alarm sound: {ex.Message}");
            }
        }

        private async Task CloseAlarmPage()
        {
            var currentPage = Application.Current.MainPage.Navigation.ModalStack
                .FirstOrDefault(p => p is AlarmNotificationPage);

            if (currentPage != null)
            {
                await Application.Current.MainPage.Navigation.PopModalAsync();
            }
        }

        private void OnAlarmFinished(object sender, EventArgs e)
        {
            _isAlarmPlaying = false;
        }

        private async Task SaveAlarmToPreferences(Activity activity)
        {
            var alarms = GetSavedAlarms();
            alarms[activity.Id] = activity.StartTime;
            var json = System.Text.Json.JsonSerializer.Serialize(alarms);
            Preferences.Set("ScheduledAlarms", json);
        }

        private async Task RemoveAlarmFromPreferences(int activityId)
        {
            var alarms = GetSavedAlarms();
            if (alarms.ContainsKey(activityId))
            {
                alarms.Remove(activityId);
                var json = System.Text.Json.JsonSerializer.Serialize(alarms);
                Preferences.Set("ScheduledAlarms", json);
            }
        }

        private Dictionary<int, DateTime> GetSavedAlarms()
        {
            var json = Preferences.Get("ScheduledAlarms", "{}");
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, DateTime>>(json)
                       ?? new Dictionary<int, DateTime>();
            }
            catch
            {
                return new Dictionary<int, DateTime>();
            }
        }
    }
}