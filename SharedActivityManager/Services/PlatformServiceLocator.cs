using SharedActivityManager.Abstracts.Platforms;

namespace SharedActivityManager.Services
{
    /// <summary>
    /// Service locator (legacy) - recomandat să folosești PlatformBridge
    /// </summary>
    public static class PlatformServiceLocator
    {
        private static IAlarmService _alarmService;
        private static IAudioService _audioService;
        private static INotificationService _notificationService;

        public static IAlarmService AlarmService
        {
            get
            {
                if (_alarmService == null)
                    _alarmService = PlatformBridge.Instance.AlarmService;
                return _alarmService;
            }
        }

        public static IAudioService AudioService
        {
            get
            {
                if (_audioService == null)
                    _audioService = PlatformBridge.Instance.AudioService;
                return _audioService;
            }
        }

        public static INotificationService NotificationService
        {
            get
            {
                if (_notificationService == null)
                    _notificationService = PlatformBridge.Instance.NotificationService;
                return _notificationService;
            }
        }
    }
}