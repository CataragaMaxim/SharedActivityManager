using SharedActivityManager.Services;

namespace SharedActivityManager.Abstracts.Platforms
{
    /// <summary>
    /// Implementation interface - definește operațiile de bază pentru platforme
    /// </summary>
    public interface IPlatformImplementation
    {
        /// <summary>
        /// Numele platformei (Android, Windows, iOS)
        /// </summary>
        string PlatformName { get; }

        /// <summary>
        /// Creează serviciul de alarme specific platformei
        /// </summary>
        IAlarmService CreateAlarmService();

        /// <summary>
        /// Creează serviciul audio specific platformei
        /// </summary>
        IAudioService CreateAudioService();

        /// <summary>
        /// Creează serviciul de notificări specific platformei
        /// </summary>
        INotificationService CreateNotificationService();

        /// <summary>
        /// Creează serviciul de setări specific platformei
        /// </summary>
        ISettingsService CreateSettingsService();
    }
}