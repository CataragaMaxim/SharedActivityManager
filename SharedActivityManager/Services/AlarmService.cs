// Services/AlarmService.cs
using SharedActivityManager.Models;
using SharedActivityManager.Services;
using CommunityToolkit.Maui.Views;
using System.Timers;

namespace SharedActivityManager.Services
{
    public class AlarmService : IAlarmService
    {
        private readonly Dictionary<int, System.Timers.Timer> _alarmTimers;
        private readonly IRingtoneService _ringtoneService;
        private MediaElement _mediaPlayer;
        private bool _isAlarmPlaying;

        public AlarmService()
        {
            _alarmTimers = new Dictionary<int, System.Timers.Timer>();
            _ringtoneService = new RingtoneService();

            // Inițializează MediaPlayer
            _mediaPlayer = new MediaElement
            {
                ShouldAutoPlay = true,
                ShouldKeepScreenOn = true,
                Volume = 1.0
            };

            _mediaPlayer.MediaEnded += OnAlarmFinished;
        }

        // Programează o alarmă
        public async Task ScheduleAlarmAsync(Activity activity)
        {
            try
            {
                // Anulează orice alarmă existentă pentru această activitate
                await CancelAlarmAsync(activity.Id);

                // Calculează timpul până la alarmă
                var alarmTime = activity.StartTime;
                var now = DateTime.Now;

                // Dacă timpul a trecut deja, nu programa
                if (alarmTime <= now)
                    return;

                var timeUntilAlarm = alarmTime - now;

                // Creează timer-ul
                var timer = new System.Timers.Timer(timeUntilAlarm.TotalMilliseconds);
                timer.Elapsed += async (sender, e) => await OnAlarmTriggered(activity);
                timer.AutoReset = false;
                timer.Enabled = true;

                // Salvează timer-ul
                _alarmTimers[activity.Id] = timer;

                // Salvează în Preferences pentru persistare
                await SaveAlarmToPreferences(activity);

                System.Diagnostics.Debug.WriteLine($"Alarm scheduled for activity {activity.Title} at {alarmTime}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error scheduling alarm: {ex.Message}");
            }
        }

        // Anulează o alarmă
        public async Task CancelAlarmAsync(int activityId)
        {
            try
            {
                if (_alarmTimers.TryGetValue(activityId, out var timer))
                {
                    timer.Stop();
                    timer.Dispose();
                    _alarmTimers.Remove(activityId);

                    // Elimină din Preferences
                    await RemoveAlarmFromPreferences(activityId);

                    System.Diagnostics.Debug.WriteLine($"Alarm cancelled for activity {activityId}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cancelling alarm: {ex.Message}");
            }
        }

        // Anulează toate alarmele
        public async Task CancelAllAlarmsAsync()
        {
            foreach (var timer in _alarmTimers.Values)
            {
                timer.Stop();
                timer.Dispose();
            }

            _alarmTimers.Clear();

            // Curăță Preferences
            Preferences.Remove("ScheduledAlarms");

            await StopCurrentAlarmAsync();
        }

        // Verifică dacă există alarmă programată
        public Task<bool> HasScheduledAlarmAsync(int activityId)
        {
            return Task.FromResult(_alarmTimers.ContainsKey(activityId));
        }

        // Declanșează alarma
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

        // Oprește alarma curentă
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

        // Redă sunetul alarmei
        public async Task PlayAlarmSoundAsync(string ringtoneName)
        {
            try
            {
                var ringtones = _ringtoneService.GetAvailableRingtones();
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

        // ===== METODE PRIVATE =====

        private async Task OnAlarmTriggered(Activity activity)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await TriggerAlarmAsync(activity);
            });
        }

        // METODA CORECTATĂ - folosește proprietățile obiectului activity
        private async Task ShowAlarmNotification(Activity activity)
        {
            var alarmPage = new AlarmNotificationPage(
                activity.Title,
                activity.Desc,
                activity.RingTone ?? "Default"
            );
            await Application.Current.MainPage.Navigation.PushModalAsync(alarmPage);
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
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, DateTime>>(json)
                   ?? new Dictionary<int, DateTime>();
        }

        public async Task RestoreAlarmsAsync(List<SharedActivityManager.Models.Activity> activities)
        {
            try
            {
                foreach (var activity in activities)
                {
                    if (activity.AlarmSet && !activity.isCompleted && activity.StartTime > DateTime.Now)
                    {
                        await ScheduleAlarmAsync(activity);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring alarms: {ex.Message}");
            }
        }
    }
}