#if WINDOWS
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Platforms.Windows.Services;
using SharedActivityManager.Services;

namespace SharedActivityManager.Platforms.Windows
{
    /// <summary>
    /// Implementare pentru platforma Windows
    /// </summary>
    public class WindowsPlatformImpl : IPlatformImplementation
    {
        public string PlatformName => "Windows";

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
    }
}
#endif