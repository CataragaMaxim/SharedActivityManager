using SharedActivityManager.Services;

namespace SharedActivityManager.Abstracts.Platforms
{
    /// <summary>
    /// Abstraction interface - interfața principală folosită de client
    /// </summary>
    public interface IPlatformAbstraction
    {
        /// <summary>
        /// Numele platformei curente
        /// </summary>
        string PlatformName { get; }

        /// <summary>
        /// Serviciul de alarme
        /// </summary>
        IAlarmService AlarmService { get; }

        /// <summary>
        /// Serviciul audio
        /// </summary>
        IAudioService AudioService { get; }

        /// <summary>
        /// Serviciul de notificări
        /// </summary>
        INotificationService NotificationService { get; }

        /// <summary>
        /// Serviciul de setări
        /// </summary>
        ISettingsService SettingsService { get; }
    }
}