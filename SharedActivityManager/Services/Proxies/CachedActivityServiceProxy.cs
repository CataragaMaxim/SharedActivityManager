using System.Collections.Concurrent;
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Proxies
{
    /// <summary>
    /// Cache Proxy - memorează rezultatele interogărilor pentru a evita accesul repetat la bază
    /// </summary>
    public class CachedActivityServiceProxy : IActivityService
    {
        private readonly IActivityService _realService;
        private readonly ConcurrentDictionary<string, CachedResult> _cache;
        private readonly TimeSpan _cacheDuration;

        private class CachedResult
        {
            public object Data { get; set; }
            public DateTime ExpiryTime { get; set; }

            public bool IsExpired => DateTime.Now > ExpiryTime;
        }

        public CachedActivityServiceProxy(IActivityService realService, int cacheDurationMinutes = 5)
        {
            _realService = realService;
            _cache = new ConcurrentDictionary<string, CachedResult>();
            _cacheDuration = TimeSpan.FromMinutes(cacheDurationMinutes);
        }

        private string GetCacheKey(string methodName, params object[] parameters)
        {
            var paramStr = string.Join("_", parameters.Select(p => p?.ToString() ?? "null"));
            return $"{methodName}_{paramStr}";
        }

        private T GetOrAddReference<T>(string cacheKey, Func<T> factory) where T : class
        {
            if (_cache.TryGetValue(cacheKey, out var cached) && !cached.IsExpired)
            {
                System.Diagnostics.Debug.WriteLine($"[CacheProxy] Cache HIT for key: {cacheKey}");
                return (T)cached.Data;
            }

            System.Diagnostics.Debug.WriteLine($"[CacheProxy] Cache MISS for key: {cacheKey}");
            var result = factory();
            _cache[cacheKey] = new CachedResult
            {
                Data = result,
                ExpiryTime = DateTime.Now.Add(_cacheDuration)
            };
            return result;
        }

        private T GetOrAddValue<T>(string cacheKey, Func<T> factory) where T : struct
        {
            if (_cache.TryGetValue(cacheKey, out var cached) && !cached.IsExpired)
            {
                System.Diagnostics.Debug.WriteLine($"[CacheProxy] Cache HIT for key: {cacheKey}");
                return (T)cached.Data;
            }

            System.Diagnostics.Debug.WriteLine($"[CacheProxy] Cache MISS for key: {cacheKey}");
            var result = factory();
            _cache[cacheKey] = new CachedResult
            {
                Data = result,
                ExpiryTime = DateTime.Now.Add(_cacheDuration)
            };
            return result;
        }


        private void InvalidateCache(string pattern = null)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                _cache.Clear();
                System.Diagnostics.Debug.WriteLine("[CacheProxy] Entire cache cleared");
                return;
            }

            var keysToRemove = _cache.Keys.Where(k => k.Contains(pattern)).ToList();
            foreach (var key in keysToRemove)
            {
                _cache.TryRemove(key, out _);
            }
            System.Diagnostics.Debug.WriteLine($"[CacheProxy] Invalidated {keysToRemove.Count} cache entries matching pattern: {pattern}");
        }

        public async Task<List<Activity>> GetActivitiesAsync()
        {
            try
            {
                var cacheKey = GetCacheKey(nameof(GetActivitiesAsync));
                System.Diagnostics.Debug.WriteLine($"[CacheProxy] GetActivitiesAsync called, key: {cacheKey}");

                if (_cache.TryGetValue(cacheKey, out var cached) && !cached.IsExpired)
                {
                    System.Diagnostics.Debug.WriteLine($"[CacheProxy] GetActivitiesAsync - CACHE HIT");
                    return (List<Activity>)cached.Data;
                }

                System.Diagnostics.Debug.WriteLine($"[CacheProxy] GetActivitiesAsync - CACHE MISS, calling real service");
                var result = await _realService.GetActivitiesAsync();

                _cache[cacheKey] = new CachedResult
                {
                    Data = result,
                    ExpiryTime = DateTime.Now.Add(_cacheDuration)
                };

                System.Diagnostics.Debug.WriteLine($"[CacheProxy] GetActivitiesAsync - Cached {result.Count} activities");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CacheProxy] GetActivitiesAsync ERROR: {ex.Message}");
                throw;
            }
        }

        public async Task<Activity> GetActivityByIdAsync(int id)
        {
            var cacheKey = GetCacheKey(nameof(GetActivityByIdAsync), id);
            return GetOrAddReference(cacheKey, () => _realService.GetActivityByIdAsync(id).Result);
        }

        public async Task SaveActivityAsync(Activity activity)
{
    try
    {
        System.Diagnostics.Debug.WriteLine($"[CacheProxy] Saving activity: {activity.Title}");
        await _realService.SaveActivityAsync(activity);
        InvalidateCache();
        System.Diagnostics.Debug.WriteLine($"[CacheProxy] Activity saved successfully");
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"[CacheProxy] Error saving activity: {ex.Message}");
        throw;
    }
}

        public async Task DeleteActivityAsync(Activity activity)
        {
            await _realService.DeleteActivityAsync(activity);

            // Invalidează cache-ul
            InvalidateCache(nameof(GetActivitiesAsync));
            InvalidateCache(nameof(GetActivityByIdAsync));
            InvalidateCache(nameof(GetTotalActivitiesCountAsync));
            InvalidateCache(nameof(GetCompletedActivitiesCountAsync));
        }

        public async Task SaveActivityWithAlarmAsync(Activity activity, IAlarmService alarmService)
        {
            await _realService.SaveActivityWithAlarmAsync(activity, alarmService);
            InvalidateCache();
        }

        public async Task<List<Activity>> GetSharedActivitiesAsync(string currentUserId)
        {
            var cacheKey = GetCacheKey(nameof(GetSharedActivitiesAsync), currentUserId);

            // 🔥 Folosește GetOrAddReference pentru List<Activity> (care este o clasă)
            return GetOrAddReference(cacheKey, () =>
            {
                return _realService.GetSharedActivitiesAsync(currentUserId).Result;
            });
        }

        public async Task<Activity> CopySharedActivityAsync(Activity sourceActivity, string newOwnerId)
        {
            var result = await _realService.CopySharedActivityAsync(sourceActivity, newOwnerId);
            InvalidateCache(); // Invalidează tot cache-ul
            return result;
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            try
            {
                var cacheKey = GetCacheKey(nameof(GetCategoriesAsync));
                System.Diagnostics.Debug.WriteLine($"[CacheProxy] GetCategoriesAsync called, key: {cacheKey}");

                // Verifică manual cache-ul
                if (_cache.TryGetValue(cacheKey, out var cached) && !cached.IsExpired)
                {
                    System.Diagnostics.Debug.WriteLine($"[CacheProxy] GetCategoriesAsync - CACHE HIT");
                    return (List<Category>)cached.Data;
                }

                System.Diagnostics.Debug.WriteLine($"[CacheProxy] GetCategoriesAsync - CACHE MISS, calling real service");
                var result = await _realService.GetCategoriesAsync();

                _cache[cacheKey] = new CachedResult
                {
                    Data = result,
                    ExpiryTime = DateTime.Now.Add(_cacheDuration)
                };

                System.Diagnostics.Debug.WriteLine($"[CacheProxy] GetCategoriesAsync - Cached {result.Count} categories");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CacheProxy] GetCategoriesAsync ERROR: {ex.Message}");
                throw;
            }
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            var cacheKey = GetCacheKey(nameof(GetCategoryByIdAsync), id);
            return GetOrAddReference(cacheKey, () => _realService.GetCategoryByIdAsync(id).Result);
        }

        public async Task<int> SaveCategoryAsync(Category category)
        {
            var result = await _realService.SaveCategoryAsync(category);
            InvalidateCache(nameof(GetCategoriesAsync));
            InvalidateCache(nameof(GetCategoryByIdAsync));
            return result;
        }

        public async Task<int> DeleteCategoryAsync(Category category)
        {
            var result = await _realService.DeleteCategoryAsync(category);
            InvalidateCache();
            return result;
        }

        public async Task MoveActivityToCategoryAsync(int activityId, int newCategoryId)
        {
            await _realService.MoveActivityToCategoryAsync(activityId, newCategoryId);
            InvalidateCache();
        }

        public Task<List<Category>> GetSubCategoriesAsync(int parentId)
            => _realService.GetSubCategoriesAsync(parentId);

        public async Task<int> GetOrCreateCategoryIdAsync(string categoryName, int parentId = 0)
        {
            var cacheKey = GetCacheKey(nameof(GetOrCreateCategoryIdAsync), categoryName, parentId);
            return GetOrAddValue(cacheKey, () => _realService.GetOrCreateCategoryIdAsync(categoryName, parentId).Result);
        }

        public async Task<int> GetTotalActivitiesCountAsync()
        {
            var cacheKey = GetCacheKey(nameof(GetTotalActivitiesCountAsync));
            return GetOrAddValue(cacheKey, () => _realService.GetTotalActivitiesCountAsync().Result);
        }

        public async Task<int> GetCompletedActivitiesCountAsync()
        {
            var cacheKey = GetCacheKey(nameof(GetCompletedActivitiesCountAsync));
            return GetOrAddValue(cacheKey, () => _realService.GetCompletedActivitiesCountAsync().Result);
        }
    }
}