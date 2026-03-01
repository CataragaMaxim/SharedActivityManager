// Platforms/Windows/WindowsAlarmService.cs
#if WINDOWS
using SharedActivityManager.Models;
using SharedActivityManager.Services;
using Microsoft.UI.Xaml;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.Timers;
using Microsoft.Maui.ApplicationModel;
using Windows.Media.Playback;
using Windows.Media.Core;
using MauiApp = Microsoft.Maui.Controls.Application; // Alias pentru MAUI Application

namespace SharedActivityManager.Platforms.Windows
{
    public class WindowsAlarmService : IAlarmService
    {
        private static Dictionary<int, System.Timers.Timer> _alarmTimers = new();
        private MediaPlayer _mediaPlayer;
        private bool _isPlaying;

        public WindowsAlarmService()
        {
            _mediaPlayer = new MediaPlayer();
        }

        public async Task ScheduleAlarmAsync(Activity activity)
        {
            try
            {
                await CancelAlarmAsync(activity.Id);

                var alarmTime = activity.StartTime;
                var now = DateTime.Now;

                if (alarmTime <= now)
                {
                    System.Diagnostics.Debug.WriteLine($"[Windows] Alarm time {alarmTime} is in the past");
                    return;
                }

                var timeUntilAlarm = alarmTime - now;

                System.Diagnostics.Debug.WriteLine($"[Windows] Scheduling alarm for {activity.Title} in {timeUntilAlarm.TotalMinutes:F1} minutes");

                var timer = new System.Timers.Timer(timeUntilAlarm.TotalMilliseconds);
                timer.Elapsed += async (sender, e) => await OnAlarmElapsed(activity);
                timer.AutoReset = false;
                timer.Enabled = true;

                _alarmTimers[activity.Id] = timer;
                ScheduleToastNotification(activity, timeUntilAlarm);
                await SaveAlarmToPreferences(activity);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Windows] Error scheduling alarm: {ex.Message}");
            }
        }

        private async Task OnAlarmElapsed(Activity activity)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[Windows] ALARM ELAPSED for {activity.Title}");
                    await PlayAlarmSoundAsync(activity.RingTone);
                    ShowLocalNotification(activity);
                    await ShowAlarmPage(activity);
                    _alarmTimers.Remove(activity.Id);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Windows] Error in alarm elapsed: {ex.Message}");
                }
            });
        }

        private void ScheduleToastNotification(Activity activity, TimeSpan delay)
        {
            try
            {
                var template = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
                var textNodes = template.GetElementsByTagName("text");
                textNodes[0].AppendChild(template.CreateTextNode($"⏰ {activity.Title}"));
                textNodes[1].AppendChild(template.CreateTextNode(activity.Desc ?? "Time for your activity!"));

                var audio = template.CreateElement("audio");
                audio.SetAttribute("src", "ms-winsoundevent:Notification.Looping.Alarm");
                audio.SetAttribute("loop", "true");
                template.GetElementsByTagName("toast")[0].AppendChild(audio);

                var toast = new ToastNotification(template);
                toast.Tag = activity.Id.ToString();
                toast.Group = "ActivityAlarms";
                toast.ExpirationTime = DateTimeOffset.Now.AddMinutes(30);

                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Windows] Error scheduling toast: {ex.Message}");
            }
        }

        private void ShowLocalNotification(Activity activity)
        {
            try
            {
                var template = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
                var textNodes = template.GetElementsByTagName("text");
                textNodes[0].AppendChild(template.CreateTextNode($"🔔 ALARM: {activity.Title}"));
                textNodes[1].AppendChild(template.CreateTextNode(activity.Desc ?? "Time for your activity!"));

                var audio = template.CreateElement("audio");
                audio.SetAttribute("src", "ms-winsoundevent:Notification.Looping.Alarm");
                audio.SetAttribute("loop", "true");
                template.GetElementsByTagName("toast")[0].AppendChild(audio);

                var toast = new ToastNotification(template);
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Windows] Error showing notification: {ex.Message}");
            }
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
                    var filePath = ringtone.FilePath ?? Path.Combine(FileSystem.AppDataDirectory, "Ringtones", ringtone.FileName);

                    if (File.Exists(filePath))
                    {
                        _mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(filePath));
                        _mediaPlayer.Play();
                        _mediaPlayer.IsLoopingEnabled = true;
                        _isPlaying = true;
                        System.Diagnostics.Debug.WriteLine($"[Windows] Playing sound: {filePath}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[Windows] Sound file not found: {filePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Windows] Error playing sound: {ex.Message}");
            }
        }

        private async Task ShowAlarmPage(Activity activity)
        {
            try
            {
                if (MauiApp.Current?.MainPage != null) // Folosim alias-ul
                {
                    var alarmPage = new AlarmNotificationPage(
                        activity.Title,
                        activity.Desc,
                        activity.RingTone ?? "Default"
                    );
                    await MauiApp.Current.MainPage.Navigation.PushModalAsync(alarmPage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Windows] Error showing alarm page: {ex.Message}");
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
                    System.Diagnostics.Debug.WriteLine($"[Windows] Cancelled alarm for activity {activityId}");
                }
                await RemoveAlarmFromPreferences(activityId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Windows] Error cancelling alarm: {ex.Message}");
            }
        }

        public async Task CancelAllAlarmsAsync()
        {
            try
            {
                foreach (var timer in _alarmTimers.Values)
                {
                    timer.Stop();
                    timer.Dispose();
                }
                _alarmTimers.Clear();
                _mediaPlayer?.Pause();
                _mediaPlayer?.Dispose();
                Preferences.Remove("ScheduledAlarms_Windows");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Windows] Error cancelling all alarms: {ex.Message}");
            }
        }

        public async Task StopCurrentAlarmAsync()
        {
            try
            {
                _mediaPlayer?.Pause();
                _mediaPlayer?.Dispose();
                _mediaPlayer = new MediaPlayer();
                _isPlaying = false;
                System.Diagnostics.Debug.WriteLine($"[Windows] Stopped current alarm");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Windows] Error stopping alarm: {ex.Message}");
            }
        }

        public async Task TriggerAlarmAsync(Activity activity)
        {
            await OnAlarmElapsed(activity);
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
                foreach (var activity in activities)
                {
                    if (savedAlarms.ContainsKey(activity.Id) && activity.AlarmSet && !activity.isCompleted)
                    {
                        if (activity.StartTime > DateTime.Now)
                        {
                            await ScheduleAlarmAsync(activity);
                        }
                        else
                        {
                            await RemoveAlarmFromPreferences(activity.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Windows] Error restoring alarms: {ex.Message}");
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
    }
}
#endif