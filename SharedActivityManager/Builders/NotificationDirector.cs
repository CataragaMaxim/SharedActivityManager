// Builders/NotificationDirector.cs
using SharedActivityManager.Models;

namespace SharedActivityManager.Builders
{
    public class NotificationDirector
    {
        private INotificationBuilder _builder;

        public INotificationBuilder Builder
        {
            set { _builder = value; }
        }

        public void BuildReminderNotification(string title, string content)
        {
            _builder.Reset();
            _builder.SetTitle($"🔔 {title}");
            _builder.SetContent(content);
            _builder.SetChannel("reminder_channel");
            _builder.SetPriority(AppNotificationPriority.Normal);  // ← Schimbat
            _builder.SetVibration(new long[] { 0, 300 });
            _builder.SetAutoCancel(true);
        }

        public void BuildAlarmNotification(string title, string content, int activityId, string ringtone)
        {
            _builder.Reset();
            _builder.SetTitle($"⏰ {title}");
            _builder.SetContent(content ?? "Time for your activity!");
            _builder.SetChannel("alarm_channel");
            _builder.SetPriority(AppNotificationPriority.High);  // ← Schimbat
            _builder.SetVibration(new long[] { 0, 500, 1000, 500 });
            _builder.SetSound(ringtone);
            _builder.AddButton("Stop", "stop_alarm", activityId);
            _builder.AddButton("Snooze", "snooze_alarm", activityId);
            _builder.SetOngoing(true);
            _builder.SetAutoCancel(false);
        }

        public void BuildSuccessNotification(string title, string content)
        {
            _builder.Reset();
            _builder.SetTitle($"✅ {title}");
            _builder.SetContent(content);
            _builder.SetChannel("success_channel");
            _builder.SetPriority(AppNotificationPriority.Low);  // ← Schimbat
            _builder.SetVibration(new long[] { 0, 100 });
            _builder.SetAutoCancel(true);
            _builder.SetTimeout(TimeSpan.FromSeconds(3));
        }

        public void BuildErrorNotification(string title, string content)
        {
            _builder.Reset();
            _builder.SetTitle($"❌ {title}");
            _builder.SetContent(content);
            _builder.SetChannel("error_channel");
            _builder.SetPriority(AppNotificationPriority.Urgent);  // ← Schimbat
            _builder.SetVibration(new long[] { 0, 1000 });
            _builder.SetAutoCancel(false);
        }
    }
}