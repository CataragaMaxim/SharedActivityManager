// Platforms/Windows/WindowsPlatformFactory.cs
#if WINDOWS
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Platforms.Windows.Services;
using SharedActivityManager.Services;

namespace SharedActivityManager.Platforms.Windows
{
    public class WindowsPlatformFactory : IPlatformFactory
    {
        public IAlarmService CreateAlarmService()
        {
            return new WindowsAlarmService();
        }

        public IAudioService CreateAudioService()
        {
            return new WindowsAudioService();
        }

        public INotificationService CreateNotificationService()
        {
            return new WindowsNotificationService();
        }

        public ISettingsService CreateSettingsService()
        {
            return new WindowsSettingsService();
        }

        public string GetPlatformName()
        {
            return "Windows";
        }
    }
}
#endif