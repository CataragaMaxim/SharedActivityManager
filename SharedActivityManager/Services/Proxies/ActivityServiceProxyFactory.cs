using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Repositories;

namespace SharedActivityManager.Services.Proxies
{
    /// <summary>
    /// Factory pentru construirea lanțului de proxy-uri
    /// </summary>
    public static class ActivityServiceProxyFactory
    {
        /// <summary>
        /// Creează lanțul complet de proxy-uri:
        /// Cache -> Security -> Virtual -> Real
        /// </summary>
        public static IActivityService CreateFullProxyChain(
            IActivityRepository repository,
            IAlarmService alarmService,
            string currentUserId,
            bool enableVirtualProxy = true,
            bool enableSecurityProxy = true,
            bool enableCacheProxy = true,
            int cacheDurationMinutes = 5)
        {
            // 1. Real Service (baza)
            IActivityService service = new RealActivityService(repository, alarmService);

            // 2. Virtual Proxy (Lazy Loading) - opțional
            VirtualActivityServiceProxy virtualProxy = null;
            if (enableVirtualProxy)
            {
                virtualProxy = new VirtualActivityServiceProxy(service);
                service = virtualProxy;
            }

            // 3. Security Proxy (Control acces) - opțional
            if (enableSecurityProxy)
            {
                service = new SecurityActivityServiceProxy(service, currentUserId);
            }

            // 4. Cache Proxy - opțional (de preferat ultimul pentru a cache-ui rezultatele deja procesate)
            if (enableCacheProxy)
            {
                service = new CachedActivityServiceProxy(service, cacheDurationMinutes);
            }

            System.Diagnostics.Debug.WriteLine("[ProxyFactory] Created proxy chain:");
            System.Diagnostics.Debug.WriteLine($"  - Virtual Proxy: {(enableVirtualProxy ? "Enabled" : "Disabled")}");
            System.Diagnostics.Debug.WriteLine($"  - Security Proxy: {(enableSecurityProxy ? "Enabled" : "Disabled")}");
            System.Diagnostics.Debug.WriteLine($"  - Cache Proxy: {(enableCacheProxy ? "Enabled" : "Disabled")}");

            return service;
        }

        /// <summary>
        /// Creează doar Virtual Proxy
        /// </summary>
        public static IActivityService CreateVirtualProxy(IActivityRepository repository, IAlarmService alarmService)
        {
            var realService = new RealActivityService(repository, alarmService);
            return new VirtualActivityServiceProxy(realService);
        }

        /// <summary>
        /// Creează doar Security Proxy
        /// </summary>
        public static IActivityService CreateSecurityProxy(IActivityRepository repository, IAlarmService alarmService, string currentUserId)
        {
            var realService = new RealActivityService(repository, alarmService);
            return new SecurityActivityServiceProxy(realService, currentUserId);
        }

        /// <summary>
        /// Creează doar Cache Proxy
        /// </summary>
        public static IActivityService CreateCacheProxy(IActivityRepository repository, IAlarmService alarmService, int cacheDurationMinutes = 5)
        {
            var realService = new RealActivityService(repository, alarmService);
            return new CachedActivityServiceProxy(realService, cacheDurationMinutes);
        }

        /// <summary>
        /// Obține Virtual Proxy-ul dintr-un lanț (dacă există)
        /// </summary>
        public static VirtualActivityServiceProxy GetVirtualProxy(IActivityService service)
        {
            while (service != null)
            {
                if (service is VirtualActivityServiceProxy virtualProxy)
                    return virtualProxy;

                // Verifică dacă e un proxy care înfășoară altul
                if (service is CachedActivityServiceProxy cached)
                {
                    // Acces la inner - ar fi nevoie de o proprietate publică
                }

                break;
            }
            return null;
        }
    }
}