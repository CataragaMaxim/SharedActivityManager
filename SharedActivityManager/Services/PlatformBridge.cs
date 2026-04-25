using SharedActivityManager.Abstracts.Platforms;

namespace SharedActivityManager.Services
{
    /// <summary>
    /// Bridge Pattern - leagă Abstraction de Implementation
    /// </summary>
    public class PlatformBridge : IPlatformAbstraction
    {
        private static PlatformBridge _instance;
        private static readonly object _lock = new object();

        private readonly IPlatformImplementation _implementation;

        private IAlarmService _alarmService;
        private IAudioService _audioService;
        private INotificationService _notificationService;
        private ISettingsService _settingsService;

        // Constructor privat pentru Singleton
        private PlatformBridge(IPlatformImplementation implementation)
        {
            _implementation = implementation;
        }

        /// <summary>
        /// Inițializează Bridge-ul cu implementarea specifică platformei
        /// </summary>
        public static void Initialize(IPlatformImplementation implementation)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new PlatformBridge(implementation);
                }
            }
        }

        /// <summary>
        /// Instanța Singleton a Bridge-ului
        /// </summary>
        public static PlatformBridge Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException(
                        "PlatformBridge not initialized. Call Initialize() first.");
                }
                return _instance;
            }
        }

        public string PlatformName => _implementation.PlatformName;

        public IAlarmService AlarmService
        {
            get
            {
                if (_alarmService == null)
                    _alarmService = _implementation.CreateAlarmService();
                return _alarmService;
            }
        }

        public IAudioService AudioService
        {
            get
            {
                if (_audioService == null)
                    _audioService = _implementation.CreateAudioService();
                return _audioService;
            }
        }

        public INotificationService NotificationService
        {
            get
            {
                if (_notificationService == null)
                    _notificationService = _implementation.CreateNotificationService();
                return _notificationService;
            }
        }

        public ISettingsService SettingsService
        {
            get
            {
                if (_settingsService == null)
                    _settingsService = _implementation.CreateSettingsService();
                return _settingsService;
            }
        }

        /// <summary>
        /// Resetează toate serviciile (folosit la testare)
        /// </summary>
        public void ResetServices()
        {
            _alarmService = null;
            _audioService = null;
            _notificationService = null;
            _settingsService = null;
        }
    }
}