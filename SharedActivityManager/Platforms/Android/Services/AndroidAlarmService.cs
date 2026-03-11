#if ANDROID
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Builders;
using SharedActivityManager.Models;
using SharedActivityManager.Services;
using ActivityModel = SharedActivityManager.Models.Activity;
using AndroidApp = Android.App.Application;
using Application = Android.App.Application;
using MauiApplication = Microsoft.Maui.Controls.Application;

namespace SharedActivityManager.Platforms.Android.Services
{
    public class AndroidAlarmService : IAlarmService
    {
        private const string ALARM_ACTION = "SHARED_ACTIVITY_MANAGER_ALARM";
        private const string REMINDER_ACTION = "SHARED_ACTIVITY_MANAGER_REMINDER";
        private AlarmManager _alarmManager;
        private MediaPlayer _mediaPlayer;
        private bool _isAlarmPlaying;
        private readonly IAudioService _audioService;
        private readonly INotificationService _notificationService;
        private readonly NotificationDirector _notificationDirector;
        private readonly INotificationBuilder _notificationBuilder;

        public AndroidAlarmService()
        {
            _alarmManager = (AlarmManager)Application.Context.GetSystemService(Context.AlarmService);
            _audioService = PlatformServiceLocator.AudioService;
            _notificationService = PlatformServiceLocator.NotificationService;
            _notificationBuilder = new NotificationBuilder();
            _notificationDirector = new NotificationDirector();
            _notificationDirector.Builder = _notificationBuilder;
        }

        // ===== METODE PRIVATE =====

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

                // 🔥 FIX: Oprește orice redare existentă înainte
                if (_mediaPlayer != null)
                {
                    _mediaPlayer.Stop();
                    _mediaPlayer.Release();
                    _mediaPlayer = null;
                }

                var ringtone = await GetRingtoneAsync(ringtoneName);

                if (ringtone != null)
                {
                    var filePath = ringtone.FilePath ??
                        Path.Combine(FileSystem.AppDataDirectory, "Ringtones", ringtone.FileName);

                    if (File.Exists(filePath))
                    {
                        _mediaPlayer = new MediaPlayer();
                        await _mediaPlayer.SetDataSourceAsync(filePath);
                        _mediaPlayer.Prepare();
                        _mediaPlayer.Start();
                        _mediaPlayer.Looping = true;
                        _isAlarmPlaying = true;

                        System.Diagnostics.Debug.WriteLine($"Android: Playing from {filePath}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Android: File not found: {filePath}");
                    }
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

        // ===== METODE PUBLICE =====
        public async Task ScheduleReminderAsync(ActivityModel activity)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== Android: ScheduleReminderAsync START for activity {activity.Id} ===");

                // Anulează reminder-ul vechi dacă există
                await CancelReminderAsync(activity.Id);
                await Task.Delay(50);

                // Calculează timpul pentru reminder (5 minute înainte de alarmă)
                // În ScheduleReminderAsync - corectat
                DateTime alarmDateTime;
                if (activity.StartDate != default && activity.StartTime != default)
                {
                    // StartTime conține deja data completă, extragem doar ora
                    alarmDateTime = activity.StartDate.Date.Add(activity.StartTime.TimeOfDay);
                }
                else
                {
                    alarmDateTime = activity.StartTime;
                }

                // Verifică dacă alarma e în viitor
                if (alarmDateTime <= DateTime.Now)
                {
                    System.Diagnostics.Debug.WriteLine($"Android: Alarm time {alarmDateTime} is in the past, not scheduling reminder");
                    return;
                }

                DateTime reminderDateTime = alarmDateTime.AddMinutes(-5);

                // Dacă reminder-ul e în trecut, nu-l programa
                if (reminderDateTime <= DateTime.Now)
                {
                    System.Diagnostics.Debug.WriteLine($"Android: Reminder time {reminderDateTime} is in the past, not scheduling");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Reminder DateTime: {reminderDateTime} (5 minutes before alarm)");
                System.Diagnostics.Debug.WriteLine($"Current time: {DateTime.Now}");
                System.Diagnostics.Debug.WriteLine($"Time until reminder: {(reminderDateTime - DateTime.Now).TotalMinutes} minutes");

                var intent = new Intent(REMINDER_ACTION);
                intent.SetClass(Application.Context, typeof(AlarmReceiver));
                intent.PutExtra("activity_id", activity.Id);
                intent.PutExtra("activity_title", activity.Title);
                intent.PutExtra("activity_desc", activity.Desc ?? "");
                intent.PutExtra("activity_ringtone", activity.RingTone ?? "Default Alarm");
                intent.PutExtra("is_reminder", true); // Marcăm că e reminder

                var pendingIntent = PendingIntent.GetBroadcast(
                    Application.Context,
                    activity.Id + 1000, // ID diferit pentru reminder
                    intent,
                    PendingIntentFlags.Immutable | PendingIntentFlags.CancelCurrent
                );

                var reminderTimeMillis = (long)(reminderDateTime.ToUniversalTime() -
                    new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

                System.Diagnostics.Debug.WriteLine($"Reminder time in millis: {reminderTimeMillis}");

                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    _alarmManager.SetExactAndAllowWhileIdle(
                        AlarmType.RtcWakeup,
                        reminderTimeMillis,
                        pendingIntent
                    );
                }
                else
                {
                    _alarmManager.SetExact(
                        AlarmType.RtcWakeup,
                        reminderTimeMillis,
                        pendingIntent
                    );
                }

                System.Diagnostics.Debug.WriteLine($"=== Android: ScheduleReminderAsync END ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Android: Error scheduling reminder: {ex.Message}");
            }
        }

        public async Task CancelReminderAsync(int activityId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Android: Attempting to cancel reminder for activity {activityId}");

                var intent = new Intent(REMINDER_ACTION);
                intent.SetClass(Application.Context, typeof(AlarmReceiver)); // ← Adaugă asta

                var pendingIntent = PendingIntent.GetBroadcast(
                    Application.Context,
                    activityId + 1000,
                    intent,
                    PendingIntentFlags.Immutable | PendingIntentFlags.NoCreate
                );

                if (pendingIntent != null)
                {
                    _alarmManager.Cancel(pendingIntent);
                    pendingIntent.Cancel();
                    System.Diagnostics.Debug.WriteLine($"Android: Reminder cancelled for activity {activityId}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Android: Error cancelling reminder: {ex.Message}");
            }
        }

        public async Task ScheduleAlarmAsync(ActivityModel activity)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== Android: ScheduleAlarmAsync START for activity {activity.Id} ===");

                // Anulează alarmele și reminder-urile vechi
                await CancelAlarmAsync(activity.Id);
                await CancelReminderAsync(activity.Id);
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

                // Programează reminder-ul (dacă e cazul)
                if (activity.AlarmSet && !activity.isCompleted)
                {
                    await ScheduleReminderAsync(activity);
                }

                // Programează alarma principală
                System.Diagnostics.Debug.WriteLine($"Alarm DateTime: {alarmDateTime}");

                var intent = new Intent(ALARM_ACTION);
                intent.SetClass(Application.Context, typeof(AlarmReceiver));
                intent.PutExtra("activity_id", activity.Id);
                intent.PutExtra("activity_title", activity.Title);
                intent.PutExtra("activity_desc", activity.Desc ?? "");
                intent.PutExtra("activity_ringtone", activity.RingTone ?? "Default Alarm");
                intent.PutExtra("is_reminder", false);

                var pendingIntent = PendingIntent.GetBroadcast(
                    Application.Context,
                    activity.Id,
                    intent,
                    PendingIntentFlags.Immutable | PendingIntentFlags.CancelCurrent
                );

                var alarmTimeMillis = (long)(alarmDateTime.ToUniversalTime() -
                    new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

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

                // Anulează alarma principală - Folosește același intent ca la creare
                var intent = new Intent(ALARM_ACTION);
                intent.SetClass(Application.Context, typeof(AlarmReceiver)); // ← Adaugă asta

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

                // Anulează și reminder-ul
                await CancelReminderAsync(activityId);

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

        public async Task TriggerAlarmAsync(ActivityModel activity)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Android: ALARM TRIGGERED for: {activity.Title}");

                // Oprește orice redare existentă înainte
                await StopCurrentAlarmAsync();

                // Apoi pornește noua alarmă
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

                if (_mediaPlayer != null)
                {
                    if (_mediaPlayer.IsPlaying)
                    {
                        _mediaPlayer.Stop();
                    }
                    _mediaPlayer.Release();
                    _mediaPlayer = null;
                }

                _isAlarmPlaying = false;

                // Închide pagina de alarmă
                await CloseAlarmPage();

                // Anulează toate notificările
                await _notificationService.DismissAllNotificationsAsync();

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

                    if (intent.Action == ALARM_ACTION || intent.Action == REMINDER_ACTION)
                    {
                        var activityId = intent.GetIntExtra("activity_id", -1);
                        var activityTitle = intent.GetStringExtra("activity_title");
                        var activityDesc = intent.GetStringExtra("activity_desc");
                        var activityRingtone = intent.GetStringExtra("activity_ringtone");
                        var isReminder = intent.GetBooleanExtra("is_reminder", false);

                        System.Diagnostics.Debug.WriteLine($"Android: Received for: {activityTitle}, ID: {activityId}, IsReminder: {isReminder}");

                        if (isReminder)
                        {
                            // Afișează doar notificarea de reminder, fără a declanșa alarma
                            ShowReminderNotification(context, activityId, activityTitle, activityDesc, activityRingtone);
                        }
                        else
                        {
                            // Creează notificare și declanșează alarma
                            CreateNotificationWithBuilder(context, activityId, activityTitle, activityDesc, activityRingtone);

                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                try
                                {
                                    var alarmService = new AndroidAlarmService();
                                    await alarmService.StopCurrentAlarmAsync();

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
                    }
                    System.Diagnostics.Debug.WriteLine($"=== Android: AlarmReceiver.OnReceive END ===");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Android: Error in AlarmReceiver: {ex.Message}");
                }
            }

            private void ShowReminderNotification(Context context, int activityId, string title, string desc, string ringtone)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Android: Starting ShowReminderNotification for {title}");

                    var notificationService = PlatformServiceLocator.NotificationService;
                    var director = new NotificationDirector();
                    var builder = new NotificationBuilder();
                    director.Builder = builder;

                    director.BuildReminderNotification($"🔔 Reminder: {title}", desc ?? "Starts in 5 minutes!");

                    var notification = builder.GetNotification();

                    System.Diagnostics.Debug.WriteLine($"Android: Notification built with ID {notification.Id}");

                    // 🔥 ELIMINAT: Testul direct cu NotificationManager
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        try
                        {
                            await notificationService.ShowNotificationAsync(notification);
                            System.Diagnostics.Debug.WriteLine($"Android: Reminder notification shown for {title} with ID {notification.Id}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Android: UI thread error: {ex.Message}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Android: Error in ShowReminderNotification: {ex.Message}");
                }
            }

            private async void CreateNotificationWithBuilder(Context context, int activityId, string title, string desc, string ringtone)
            {
                try
                {
                    var notificationService = PlatformServiceLocator.NotificationService;

                    var director = new NotificationDirector();
                    var builder = new NotificationBuilder();
                    director.Builder = builder;

                    director.BuildAlarmNotification(title, desc ?? "Time for your activity!", activityId, ringtone);

                    var notification = builder.GetNotification();

                    // Folosește noua metodă ShowNotificationAsync cu AppNotification
                    await notificationService.ShowNotificationAsync(notification);

                    System.Diagnostics.Debug.WriteLine($"Android: Notification created with builder for {title} with ID {notification.Id}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Android: Error creating notification with builder: {ex.Message}");
                    CreateNotificationFallback(context, activityId, title, desc, ringtone);
                }
            }

            // Metoda de fallback (păstrăm metoda veche pentru siguranță)
            private void CreateNotificationFallback(Context context, int activityId, string title, string desc, string ringtone)
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

                    System.Diagnostics.Debug.WriteLine($"Android: Fallback notification created for {title} with ID {activityId}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Android: Error in fallback notification: {ex.Message}");
                }
            }
        }
    }
}
#endif