using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using SharedActivityManager.Models;
using SharedActivityManager.Services;
using AndroidApp = Android.App;
using Application = Android.App.Application;
using ActivityModel = SharedActivityManager.Models.Activity;
using MauiApplication = Microsoft.Maui.Controls.Application; // Alias pentru MAUI Application

namespace SharedActivityManager.Platforms.Android
{
    public class AlarmService : IAlarmService
    {
        private const string ALARM_ACTION = "SHARED_ACTIVITY_MANAGER_ALARM";
        private AndroidApp.AlarmManager _alarmManager;
        private MediaPlayer _mediaPlayer;
        private bool _isAlarmPlaying;

        public AlarmService()
        {
            _alarmManager = (AndroidApp.AlarmManager)Application.Context.GetSystemService(Context.AlarmService);
        }

        // ===== METODE PRIVATE (definite înainte de a fi folosite) =====

        private async Task ShowAlarmNotification(ActivityModel activity)
        {
            try
            {
                var alarmPage = new AlarmNotificationPage(
                    activity.Title,
                    activity.Desc,
                    activity.RingTone ?? "Default"
                );

                if (MauiApplication.Current?.MainPage != null)
                {
                    await MauiApplication.Current.MainPage.Navigation.PushModalAsync(alarmPage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing notification: {ex.Message}");
            }
        }

        private async Task PlayAlarmSoundAsync(string ringtoneName)
        {
            try
            {
                var ringtoneService = new RingtoneService();
                await ringtoneService.PlayRingtoneAsync(ringtoneName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing alarm sound: {ex.Message}");
            }
        }

        private async Task CloseAlarmPage()
        {
            try
            {
                if (MauiApplication.Current?.MainPage != null)
                {
                    var currentPage = MauiApplication.Current.MainPage.Navigation.ModalStack
                        .FirstOrDefault(p => p is AlarmNotificationPage);

                    if (currentPage != null)
                    {
                        await MauiApplication.Current.MainPage.Navigation.PopModalAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error closing alarm page: {ex.Message}");
            }
        }

        private async Task SaveAlarmToPreferences(ActivityModel activity)
        {
            var alarms = GetSavedAlarms();
            alarms[activity.Id] = activity.StartTime;

            var json = System.Text.Json.JsonSerializer.Serialize(alarms);
            Preferences.Set("ScheduledAlarms", json);
            await Task.CompletedTask;
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
            await Task.CompletedTask;
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

        // ===== METODE PUBLICE =====

        public async Task ScheduleAlarmAsync(ActivityModel activity)
        {
            try
            {
                await CancelAlarmAsync(activity.Id);

                var intent = new Intent(ALARM_ACTION);
                intent.PutExtra("activity_id", activity.Id);
                intent.PutExtra("activity_title", activity.Title);
                intent.PutExtra("activity_desc", activity.Desc ?? "");
                intent.PutExtra("activity_ringtone", activity.RingTone ?? "Default Alarm");

                var pendingIntent = PendingIntent.GetBroadcast(
                    Application.Context,
                    activity.Id,
                    intent,
                    PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent
                );

                var alarmTimeMillis = (long)(activity.StartTime.ToUniversalTime() -
                    new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    _alarmManager.SetExactAndAllowWhileIdle(
                        AndroidApp.AlarmType.RtcWakeup,
                        alarmTimeMillis,
                        pendingIntent
                    );
                }
                else
                {
                    _alarmManager.SetExact(
                        AndroidApp.AlarmType.RtcWakeup,
                        alarmTimeMillis,
                        pendingIntent
                    );
                }

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
                var intent = new Intent(ALARM_ACTION);
                var pendingIntent = PendingIntent.GetBroadcast(
                    Application.Context,
                    activityId,
                    intent,
                    PendingIntentFlags.Immutable | PendingIntentFlags.NoCreate
                );

                if (pendingIntent != null)
                {
                    _alarmManager.Cancel(pendingIntent);
                    pendingIntent.Cancel();
                }

                await RemoveAlarmFromPreferences(activityId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cancelling alarm: {ex.Message}");
            }
        }

        public async Task CancelAllAlarmsAsync()
        {
            var alarms = GetSavedAlarms();
            foreach (var activityId in alarms.Keys)
            {
                await CancelAlarmAsync(activityId);
            }
            Preferences.Remove("ScheduledAlarms");
        }

        public Task<bool> HasScheduledAlarmAsync(int activityId)
        {
            var alarms = GetSavedAlarms();
            return Task.FromResult(alarms.ContainsKey(activityId));
        }

        public async Task RestoreAlarmsAsync(List<ActivityModel> activities)
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
                System.Diagnostics.Debug.WriteLine($"Error restoring alarms: {ex.Message}");
            }
        }

        public async Task TriggerAlarmAsync(ActivityModel activity)
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
                _mediaPlayer?.Release();
                _mediaPlayer = null;
                _isAlarmPlaying = false;
                await CloseAlarmPage();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping alarm: {ex.Message}");
            }
        }

        // ===== BROADCAST RECEIVER =====

        [BroadcastReceiver(Enabled = true, Exported = false)]
        public class AlarmReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                try
                {
                    if (intent.Action == ALARM_ACTION)
                    {
                        var activityId = intent.GetIntExtra("activity_id", -1);
                        var activityTitle = intent.GetStringExtra("activity_title");
                        var activityDesc = intent.GetStringExtra("activity_desc");
                        var activityRingtone = intent.GetStringExtra("activity_ringtone");

                        System.Diagnostics.Debug.WriteLine($"Alarm received for: {activityTitle}");

                        // Creează obiectul Activity
                        var activity = new ActivityModel
                        {
                            Id = activityId,
                            Title = activityTitle,
                            Desc = activityDesc,
                            RingTone = activityRingtone
                        };

                        // Rulează pe UI thread
                        var handler = new Handler(Looper.MainLooper);
                        handler.Post(() =>
                        {
                            try
                            {
                                var alarmService = new AlarmService();
                                alarmService.TriggerAlarmAsync(activity).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error in alarm handler: {ex.Message}");
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in AlarmReceiver: {ex.Message}");
                }
            }
        }
    }
}