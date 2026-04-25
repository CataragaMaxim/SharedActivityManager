#if ANDROID
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Platforms.Android.Services;
using SharedActivityManager.Services;

namespace SharedActivityManager.Platforms.Android
{
    /// <summary>
    /// Implementare pentru platforma Android
    /// </summary>
    public class AndroidPlatformImpl : IPlatformImplementation
    {
        public string PlatformName => "Android";

        public IAlarmService CreateAlarmService()
        {
            return new AndroidAlarmService();
        }

        public IAudioService CreateAudioService()
        {
            return new AndroidAudioService();
        }

        public INotificationService CreateNotificationService()
        {
            return new AndroidNotificationService();
        }

        public ISettingsService CreateSettingsService()
        {
            return new AndroidSettingsService();
        }
    }
}
#endif