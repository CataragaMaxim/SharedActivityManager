using SharedActivityManager.Abstracts.Platforms;

namespace SharedActivityManager.Services
{
    public static class PlatformServiceLocator
    {
        private static IPlatformFactory _platformFactory;
        private static IAlarmService _alarmService;
        private static IAudioService _audioService;
        private static INotificationService _notificationService;
        private static ISettingsService _settingsService;

        private static IPlatformFactory PlatformFactory
        {
            get
            {
                if (_platformFactory == null)
                {
#if ANDROID
                    _platformFactory = new SharedActivityManager.Platforms.Android.AndroidPlatformFactory();
#elif WINDOWS
                    _platformFactory = new SharedActivityManager.Platforms.Windows.WindowsPlatformFactory();
#endif
                }
                return _platformFactory;
            }
        }

        public static IAlarmService AlarmService
        {
            get
            {
                _alarmService ??= PlatformFactory.CreateAlarmService();
                return _alarmService;
            }
        }

        public static IAudioService AudioService
        {
            get
            {
                _audioService ??= PlatformFactory.CreateAudioService();
                return _audioService;
            }
        }

        public static INotificationService NotificationService
        {
            get
            {
                _notificationService ??= PlatformFactory.CreateNotificationService();
                return _notificationService;
            }
        }

        public static ISettingsService SettingsService
        {
            get
            {
                _settingsService ??= PlatformFactory.CreateSettingsService();
                return _settingsService;
            }
        }

        public static string GetPlatformName()
        {
            return PlatformFactory.GetPlatformName();
        }
    }
}