using SharedActivityManager.Abstracts.Platforms;

namespace SharedActivityManager.Services
{
    /// <summary>
    /// Factory pentru a crea implementarea corectă a platformei
    /// </summary>
    public static class PlatformFactory
    {
        public static IPlatformImplementation Create()
        {
#if ANDROID
            return new Platforms.Android.AndroidPlatformImpl();
#elif WINDOWS
            return new Platforms.Windows.WindowsPlatformImpl();
#else
            throw new PlatformNotSupportedException("Platform not supported");
#endif
        }

        /// <summary>
        /// Inițializează PlatformBridge cu implementarea corectă
        /// </summary>
        public static void InitializePlatformBridge()
        {
            var implementation = Create();
            PlatformBridge.Initialize(implementation);

            System.Diagnostics.Debug.WriteLine($"PlatformBridge initialized for: {implementation.PlatformName}");
        }
    }
}