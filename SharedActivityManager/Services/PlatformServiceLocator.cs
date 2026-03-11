// Services/PlatformServiceLocator.cs
using SharedActivityManager.Abstracts.Platforms;

namespace SharedActivityManager.Services
{
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
                {
#if ANDROID
                    _alarmService = new SharedActivityManager.Platforms.Android.Services.AndroidAlarmService();
#elif WINDOWS
                    _alarmService = new SharedActivityManager.Platforms.Windows.Services.WindowsAlarmService();
#endif
                }
                return _alarmService;
            }
        }

        public static IAudioService AudioService
        {
            get
            {
                if (_audioService == null)
                {
#if ANDROID
                    _audioService = new SharedActivityManager.Platforms.Android.Services.AndroidAudioService();
#elif WINDOWS
                    _audioService = new SharedActivityManager.Platforms.Windows.Services.WindowsAudioService();
#endif
                }
                return _audioService;
            }
        }

        public static INotificationService NotificationService
        {
            get
            {
                if (_notificationService == null)
                {
#if ANDROID
                    _notificationService = new SharedActivityManager.Platforms.Android.Services.AndroidNotificationService();
#elif WINDOWS
                    _notificationService = new SharedActivityManager.Platforms.Windows.Services.WindowsNotificationService();
#endif
                }
                return _notificationService;
            }
        }
    }
}