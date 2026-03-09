#if ANDROID
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using SharedActivityManager.Models;
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Services;
using AndroidApp = Android.App.Application;
using Application = Android.App.Application;
using ActivityModel = SharedActivityManager.Models.Activity; // ← Alias-ul tău
using MauiApplication = Microsoft.Maui.Controls.Application;

namespace SharedActivityManager.Platforms.Android.Services
{
    public class AndroidAlarmService : IAlarmService
    {
        private const string ALARM_ACTION = "SHARED_ACTIVITY_MANAGER_ALARM";
        private AlarmManager _alarmManager;
        private MediaPlayer _mediaPlayer;
        private bool _isAlarmPlaying;
        private readonly IAudioService _audioService;

        public AndroidAlarmService()
        {
            _alarmManager = (AlarmManager)Application.Context.GetSystemService(Context.AlarmService);
            _audioService = PlatformServiceLocator.AudioService;
        }

        // ===== METODE PRIVATE (folosesc ActivityModel) =====

        private async Task ShowAlarmNotification(ActivityModel activity)
        {
            try
            {
                var alarmPage = new AlarmNotificationPage(
                    activity.Title,
                    activity.Desc,
                    activity.RingTone ?? "Default"
                );

                if (MauiApplication.Current?.Windows[0]?.Page != null)
                {
                    await MauiApplication.Current.Windows[0].Page.Navigation.PushModalAsync(alarmPage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Android: Error showing notification: {ex.Message}");
            }
        }

        private async Task PlayAlarmSoundAsync(string ringtoneName)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Android: Playing alarm sound: {ringtoneName}");

                var ringtone = await GetRingtoneAsync(ringtoneName);

                if (ringtone != null)
                {
                    var filePath = ringtone.FilePath ??
                        Path.Combine(FileSystem.AppDataDirectory, "Ringtones", ringtone.FileName);

                    if (File.Exists(filePath))
                    {
                        System.Diagnostics.Debug.WriteLine($"Android: File exists at: {filePath}");
                        System.Diagnostics.Debug.WriteLine($"Android: Creating MediaPlayer...");

                        _mediaPlayer = new MediaPlayer();

                        await _mediaPlayer.SetDataSourceAsync(filePath);
                        System.Diagnostics.Debug.WriteLine($"Android: SetDataSourceAsync OK");

                        _mediaPlayer.Prepare();
                        System.Diagnostics.Debug.WriteLine($"Android: Prepare OK");

                        _mediaPlayer.Start();
                        System.Diagnostics.Debug.WriteLine($"Android: Start OK - SHOULD BE PLAYING NOW");

                        _mediaPlayer.Looping = true;
                        _isAlarmPlaying = true;

                        System.Diagnostics.Debug.WriteLine($"Android: Playing from {filePath}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Android: File not found: {filePath}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Android: Ringtone not found: {ringtoneName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Android: Error playing alarm sound: {ex.Message}");
            }
        }

        private async Task CloseAlarmPage()
        {
            try
            {
                if (MauiApplication.Current?.Windows[0]?.Page != null)
                {
                    var currentPage = MauiApplication.Current.Windows[0].Page.Navigation.ModalStack
                        .FirstOrDefault(p => p is AlarmNotificationPage);

                    if (currentPage != null)
                    {
                        await MauiApplication.Current.Windows[0].Page.Navigation.PopModalAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Android: Error closing alarm page: {ex.Message}");
            }
        }

        private async Task<RingtoneProj> GetRingtoneAsync(string ringtoneName)
        {
            try
            {
                var ringtones = await _audioService.GetAvailableRingtonesAsync();
                return ringtones.FirstOrDefault(r =>
                    r.DisplayName == ringtoneName || r.Title == ringtoneName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Android: Error getting ringtone: {ex.Message}");
                return null;
            }
        }

        private async Task SaveAlarmToPreferences(ActivityModel activity)
        {
            var alarms = GetSavedAlarms();
            alarms[activity.Id] = activity.StartTime;

            var json = System.Text.Json.JsonSerializer.Serialize(alarms);
            Preferences.Set("ScheduledAlarms_Android", json);
            await Task.CompletedTask;
        }

        private async Task RemoveAlarmFromPreferences(int activityId)
        {
            var alarms = GetSavedAlarms();
            if (alarms.ContainsKey(activityId))
            {
                alarms.Remove(activityId);
                var json = System.Text.Json.JsonSerializer.Serialize(alarms);
                Preferences.Set("ScheduledAlarms_Android", json);
            }
            await Task.CompletedTask;
        }

        private Dictionary<int, DateTime> GetSavedAlarms()
        {
            var json = Preferences.Get("ScheduledAlarms_Android", "{}");
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

        private async Task OnAlarmTriggered(ActivityModel activity)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await TriggerAlarmAsync(activity);
            });
        }

        // ===== METODE PUBLICE (TOATE folosesc ActivityModel) =====

        // 🔥 FIX: Schimbă parametrul din Activity în ActivityModel
        public async Task ScheduleAlarmAsync(ActivityModel activity)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== Android: ScheduleAlarmAsync START for activity {activity.Id} ===");

                await CancelAlarmAsync(activity.Id);
                await Task.Delay(100);

                DateTime alarmDateTime;
                if (activity.StartDate != default && activity.StartTime != default)
                {
                    alarmDateTime = activity.StartDate.Date.Add(activity.StartTime.TimeOfDay);
                }
                else
                {
                    alarmDateTime = activity.StartTime;
                }

                if (alarmDateTime <= DateTime.Now)
                {
                    System.Diagnostics.Debug.WriteLine($"Android: Alarm time {alarmDateTime} is in the past, not scheduling");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Alarm DateTime: {alarmDateTime}");
                System.Diagnostics.Debug.WriteLine($"Current time: {DateTime.Now}");
                System.Diagnostics.Debug.WriteLine($"Time until alarm: {(alarmDateTime - DateTime.Now).TotalMinutes} minutes");

                var intent = new Intent(ALARM_ACTION);
                intent.SetClass(Application.Context, typeof(AlarmReceiver));
                intent.PutExtra("activity_id", activity.Id);
                intent.PutExtra("activity_title", activity.Title);
                intent.PutExtra("activity_desc", activity.Desc ?? "");
                intent.PutExtra("activity_ringtone", activity.RingTone ?? "Default Alarm");

                var pendingIntent = PendingIntent.GetBroadcast(
                    Application.Context,
                    activity.Id,
                    intent,
                    PendingIntentFlags.Immutable | PendingIntentFlags.CancelCurrent
                );

                var alarmTimeMillis = (long)(alarmDateTime.ToUniversalTime() -
                    new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

                System.Diagnostics.Debug.WriteLine($"Alarm time in millis: {alarmTimeMillis}");

                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    _alarmManager.SetExactAndAllowWhileIdle(
                        AlarmType.RtcWakeup,
                        alarmTimeMillis,
                        pendingIntent
                    );
                }
                else
                {
                    _alarmManager.SetExact(
                        AlarmType.RtcWakeup,
                        alarmTimeMillis,
                        pendingIntent
                    );
                }

                await SaveAlarmToPreferences(activity);
                System.Diagnostics.Debug.WriteLine($"=== Android: ScheduleAlarmAsync END ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Android: Error scheduling alarm: {ex.Message}");
            }
        }

        public async Task CancelAlarmAsync(int activityId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Android: Attempting to cancel alarm for activity {activityId}");

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
                    System.Diagnostics.Debug.WriteLine($"Android: Alarm cancelled for activity {activityId}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Android: No pending intent found for activity {activityId}");
                }

                await RemoveAlarmFromPreferences(activityId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Android: Error cancelling alarm: {ex.Message}");
            }
        }

        public async Task CancelAllAlarmsAsync()
        {
            var alarms = GetSavedAlarms();
            foreach (var activityId in alarms.Keys)
            {
                await CancelAlarmAsync(activityId);
            }
            Preferences.Remove("ScheduledAlarms_Android");
        }

        public Task<bool> HasScheduledAlarmAsync(int activityId)
        {
            var alarms = GetSavedAlarms();
            return Task.FromResult(alarms.ContainsKey(activityId));
        }

        // 🔥 FIX: Schimbă parametrul din List<Activity> în List<ActivityModel>
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

                System.Diagnostics.Debug.WriteLine($"Android: Restored {savedAlarms.Count} alarms");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Android: Error restoring alarms: {ex.Message}");
            }
        }

        // 🔥 FIX: Schimbă parametrul din Activity în ActivityModel
        public async Task TriggerAlarmAsync(ActivityModel activity)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Android: ALARM TRIGGERED for: {activity.Title}");
                await ShowAlarmNotification(activity);
                await PlayAlarmSoundAsync(activity.RingTone);
                await CancelAlarmAsync(activity.Id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Android: Error triggering alarm: {ex.Message}");
            }
        }

        public async Task StopCurrentAlarmAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Android: Stopping current alarm");

                _mediaPlayer?.Stop();
                _mediaPlayer?.Release();
                _mediaPlayer = null;
                _isAlarmPlaying = false;

                await CloseAlarmPage();

                var notificationManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
                notificationManager.CancelAll();

                System.Diagnostics.Debug.WriteLine($"Android: Stopped current alarm and cleared notifications");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Android: Error stopping alarm: {ex.Message}");
            }
        }

        [BroadcastReceiver(Enabled = true, Exported = false)]
        public class AlarmReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== Android: AlarmReceiver.OnReceive START ===");
                    System.Diagnostics.Debug.WriteLine($"Action: {intent.Action}");
                    System.Diagnostics.Debug.WriteLine($"Time: {DateTime.Now}");

                    if (intent.Action == ALARM_ACTION)
                    {
                        var activityId = intent.GetIntExtra("activity_id", -1);
                        var activityTitle = intent.GetStringExtra("activity_title");
                        var activityDesc = intent.GetStringExtra("activity_desc");
                        var activityRingtone = intent.GetStringExtra("activity_ringtone");

                        System.Diagnostics.Debug.WriteLine($"Android: Alarm received for: {activityTitle}, ID: {activityId}");

                        CreateNotification(context, activityId, activityTitle, activityDesc, activityRingtone);

                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            try
                            {
                                System.Diagnostics.Debug.WriteLine($"Android: Creating alarm service instance...");
                                var alarmService = new AndroidAlarmService();

                                await alarmService.StopCurrentAlarmAsync();

                                System.Diagnostics.Debug.WriteLine($"Android: Triggering alarm...");
                                await alarmService.TriggerAlarmAsync(new ActivityModel
                                {
                                    Id = activityId,
                                    Title = activityTitle,
                                    Desc = activityDesc,
                                    RingTone = activityRingtone
                                });
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Android: Error in alarm handler: {ex.Message}");
                            }
                        });
                    }
                    System.Diagnostics.Debug.WriteLine($"=== Android: AlarmReceiver.OnReceive END ===");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Android: Error in AlarmReceiver: {ex.Message}");
                }
            }

            private void CreateNotification(Context context, int activityId, string title, string desc, string ringtone)
            {
                try
                {
                    var intent = new Intent(context, typeof(MainActivity));
                    intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
                    intent.PutExtra("show_alarm", true);
                    intent.PutExtra("activity_id", activityId);
                    intent.PutExtra("activity_title", title);
                    intent.PutExtra("activity_desc", desc);
                    intent.PutExtra("activity_ringtone", ringtone);

                    var pendingIntent = PendingIntent.GetActivity(
                        context,
                        activityId,
                        intent,
                        PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent
                    );

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        var channel = new NotificationChannel(
                            "alarm_channel",
                            "Activity Alarms",
                            NotificationImportance.High
                        );
                        channel.EnableVibration(true);
                        channel.SetSound(null, null);
                        channel.LockscreenVisibility = NotificationVisibility.Public;
                        channel.SetShowBadge(true);

                        var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
                        notificationManager.CreateNotificationChannel(channel);
                    }

                    var builder = new Notification.Builder(context, "alarm_channel")
                        .SetContentTitle($"⏰ {title}")
                        .SetContentText(desc ?? "Time for your activity!")
                        .SetSmallIcon(global::Android.Resource.Drawable.IcDialogAlert)
                        .SetAutoCancel(true)
                        .SetContentIntent(pendingIntent)
                        .SetPriority((int)NotificationPriority.High)
                        .SetVibrate(new long[] { 0, 500, 1000, 500 })
                        .SetVisibility(NotificationVisibility.Public)
                        .SetOngoing(true);

                    var manager = (NotificationManager)context.GetSystemService(Context.NotificationService);
                    manager.Notify(activityId, builder.Build());

                    System.Diagnostics.Debug.WriteLine($"Android: Notification created for {title} with ID {activityId}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Android: Error creating notification: {ex.Message}");
                }
            }
        }
    }
}
#endif