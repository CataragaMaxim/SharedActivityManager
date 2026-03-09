// Platforms/Android/AndroidPlatformFactory.cs
#if ANDROID
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Platforms.Android.Services;
using SharedActivityManager.Services;

namespace SharedActivityManager.Platforms.Android
{
    public class AndroidPlatformFactory : IPlatformFactory
    {
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

        public string GetPlatformName()
        {
            return "Android";
        }
    }
}
#endif