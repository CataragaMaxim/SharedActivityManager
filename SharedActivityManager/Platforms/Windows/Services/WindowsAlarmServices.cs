#if WINDOWS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SharedActivityManager.Models;
using SharedActivityManager.Abstracts.Platforms;
using MauiApp = Microsoft.Maui.Controls.Application;
using SharedActivityManager.Services;

namespace SharedActivityManager.Platforms.Windows.Services
{
    public class WindowsAlarmService : IAlarmService
    {
        private readonly Dictionary<int, System.Timers.Timer> _alarmTimers;
        private bool _isAlarmPlaying;
        private readonly IAudioService _audioService;

        public WindowsAlarmService()
        {
            _alarmTimers = new Dictionary<int, System.Timers.Timer>();

            // Obținem serviciul audio prin PlatformServiceLocator
            _audioService = PlatformServiceLocator.AudioService;
        }

        // ===== METODE PRIVATE =====

        private async Task ShowAlarmNotification(Activity activity)
        {
            try
            {
                var alarmPage = new AlarmNotificationPage(
                    activity.Title,
                    activity.Desc,
                    activity.RingTone ?? "Default"
                );

                if (MauiApp.Current?.Windows[0]?.Page != null)
                {
                    await MauiApp.Current.Windows[0].Page.Navigation.PushModalAsync(alarmPage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WindowsAlarmService: Error showing notification: {ex.Message}");
            }
        }

        private async Task PlayAlarmSoundAsync(string ringtoneName)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"WindowsAlarmService: Playing alarm sound: {ringtoneName}");

                // 🔥 FIX: Folosește IAudioService pentru redare
                await _audioService.PlayRingtoneAsync(ringtoneName);
                _isAlarmPlaying = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WindowsAlarmService: Error playing alarm sound: {ex.Message}");
            }
        }

        private async Task CloseAlarmPage()
        {
            try
            {
                if (MauiApp.Current?.Windows[0]?.Page != null)
                {
                    var currentPage = MauiApp.Current.Windows[0].Page.Navigation.ModalStack
                        .FirstOrDefault(p => p is AlarmNotificationPage);

                    if (currentPage != null)
                    {
                        await MauiApp.Current.Windows[0].Page.Navigation.PopModalAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WindowsAlarmService: Error closing alarm page: {ex.Message}");
            }
        }

        private async Task SaveAlarmToPreferences(Activity activity)
        {
            var alarms = GetSavedAlarms();
            alarms[activity.Id] = activity.StartTime;

            var json = System.Text.Json.JsonSerializer.Serialize(alarms);
            Preferences.Set("ScheduledAlarms_Windows", json);
            await Task.CompletedTask;
        }

        private async Task RemoveAlarmFromPreferences(int activityId)
        {
            var alarms = GetSavedAlarms();
            if (alarms.ContainsKey(activityId))
            {
                alarms.Remove(activityId);
                var json = System.Text.Json.JsonSerializer.Serialize(alarms);
                Preferences.Set("ScheduledAlarms_Windows", json);
            }
            await Task.CompletedTask;
        }

        private Dictionary<int, DateTime> GetSavedAlarms()
        {
            var json = Preferences.Get("ScheduledAlarms_Windows", "{}");
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

        private async Task OnAlarmTriggered(Activity activity)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await TriggerAlarmAsync(activity);
            });
        }

        // ===== METODE PUBLICE =====

        public async Task ScheduleAlarmAsync(Activity activity)
        {
            try
            {
                await CancelAlarmAsync(activity.Id);

                var alarmTime = activity.StartTime;
                var now = DateTime.Now;

                if (alarmTime <= now)
                {
                    System.Diagnostics.Debug.WriteLine($"WindowsAlarmService: Alarm time {alarmTime} is in the past");
                    return;
                }

                var timeUntilAlarm = alarmTime - now;

                var timer = new System.Timers.Timer(timeUntilAlarm.TotalMilliseconds);
                timer.Elapsed += async (sender, e) => await OnAlarmTriggered(activity);
                timer.AutoReset = false;
                timer.Enabled = true;

                _alarmTimers[activity.Id] = timer;
                await SaveAlarmToPreferences(activity);

                System.Diagnostics.Debug.WriteLine($"WindowsAlarmService: Alarm scheduled for {activity.Title} at {alarmTime}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WindowsAlarmService: Error scheduling alarm: {ex.Message}");
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
                }

                await RemoveAlarmFromPreferences(activityId);
                System.Diagnostics.Debug.WriteLine($"WindowsAlarmService: Alarm cancelled for activity {activityId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WindowsAlarmService: Error cancelling alarm: {ex.Message}");
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

            Preferences.Remove("ScheduledAlarms_Windows");
            await StopCurrentAlarmAsync();

            System.Diagnostics.Debug.WriteLine($"WindowsAlarmService: All alarms cancelled");
        }

        public Task<bool> HasScheduledAlarmAsync(int activityId)
        {
            return Task.FromResult(_alarmTimers.ContainsKey(activityId));
        }

        public async Task RestoreAlarmsAsync(List<Activity> activities)
        {
            try
            {
                var savedAlarms = GetSavedAlarms();
                int restoredCount = 0;

                foreach (var activity in activities)
                {
                    if (savedAlarms.ContainsKey(activity.Id) && activity.AlarmSet && !activity.isCompleted)
                    {
                        if (activity.StartTime > DateTime.Now)
                        {
                            await ScheduleAlarmAsync(activity);
                            restoredCount++;
                        }
                        else
                        {
                            await RemoveAlarmFromPreferences(activity.Id);
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"WindowsAlarmService: Restored {restoredCount} alarms");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WindowsAlarmService: Error restoring alarms: {ex.Message}");
            }
        }

        public async Task TriggerAlarmAsync(Activity activity)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"WindowsAlarmService: ALARM TRIGGERED for: {activity.Title}");

                // 🔥 FIX: Mai întâi arată pagina, apoi redă sunetul
                await ShowAlarmNotification(activity);
                await PlayAlarmSoundAsync(activity.RingTone);

                // Nu mai anula alarma aici - las-o să ruleze până când utilizatorul o oprește
                // await CancelAlarmAsync(activity.Id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WindowsAlarmService: Error triggering alarm: {ex.Message}");
            }
        }

        public async Task StopCurrentAlarmAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"WindowsAlarmService: Stopping current alarm");

                // 🔥 FIX: Folosește IAudioService pentru a opri redarea
                await _audioService.StopPlayingAsync();
                _isAlarmPlaying = false;

                await CloseAlarmPage();

                System.Diagnostics.Debug.WriteLine($"WindowsAlarmService: Stopped current alarm");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WindowsAlarmService: Error stopping alarm: {ex.Message}");
            }
        }

        public Task ScheduleReminderAsync(Activity activity)
        {
            throw new NotImplementedException();
        }
    }
}
#endif