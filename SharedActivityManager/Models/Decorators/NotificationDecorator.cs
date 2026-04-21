using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Services;

namespace SharedActivityManager.Models.Decorators
{
    /// <summary>
    /// Decorator pentru notificări push
    /// </summary>
    public class NotificationDecorator : ActivityDecorator
    {
        private readonly INotificationService _notificationService;

        public NotificationDecorator(IActivityExtra inner) : base(inner)
        {
            _notificationService = PlatformServiceLocator.NotificationService;
        }

        public override string Name => "Push Notifications";
        public override bool IsEnabled => true;

        public override string GetDescription()
        {
            return $"{_inner.GetDescription()} + 🔔 Push Notifications";
        }

        public override int GetExtraCost()
        {
            return _inner.GetExtraCost() + 1; // 1 minut pentru configurare notificare
        }

        public override string GetIcon()
        {
            return "🔔";
        }

        public override async Task ExecuteAsync(Activity activity)
        {
            await _inner.ExecuteAsync(activity);

            // Trimite notificare
            await _notificationService.ShowNotificationAsync(
                new AppNotification
                {
                    Title = $"✅ Activity Completed",
                    Content = $"You completed: {activity.Title}",
                    Priority = AppNotificationPriority.High
                });
        }
    }
}