using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Proxies
{
    /// <summary>
    /// Virtual Proxy - încarcă datele treptat (lazy loading)
    /// Versiune corectată - fără deadlock
    /// </summary>
    public class VirtualActivityServiceProxy : IActivityService
    {
        private readonly IActivityService _realService;
        private List<Activity> _loadedActivities;
        private bool _isFullyLoaded;
        private readonly int _pageSize;
        private int _currentPage;
        private readonly object _lockObject = new object();
        private bool _isLoading = false;

        public VirtualActivityServiceProxy(IActivityService realService, int pageSize = 20)
        {
            _realService = realService;
            _pageSize = pageSize;
            _loadedActivities = new List<Activity>();
            _currentPage = 0;
            _isFullyLoaded = false;
        }

        /// <summary>
        /// Obține activitățile (încarcă prima pagină dacă e necesar)
        /// </summary>
        public async Task<List<Activity>> GetActivitiesAsync()
        {
            // Dacă nu avem activități încărcate, încarcă prima pagină
            if (_loadedActivities.Count == 0 && !_isLoading)
            {
                await LoadNextPageAsync();
            }

            lock (_lockObject)
            {
                // Returnează o copie pentru a evita modificări în timpul iterării
                return _loadedActivities.ToList();
            }
        }

        /// <summary>
        /// Încarcă următoarea pagină de activități
        /// </summary>
        public async Task<List<Activity>> LoadNextPageAsync()
        {
            // Evită încărcări multiple simultane
            if (_isLoading || _isFullyLoaded)
                return new List<Activity>();

            _isLoading = true;

            try
            {
                System.Diagnostics.Debug.WriteLine($"[VirtualProxy] Loading page {_currentPage + 1}...");

                // Obține toate activitățile (o singură dată)
                var allActivities = await _realService.GetActivitiesAsync();

                lock (_lockObject)
                {
                    var skip = _currentPage * _pageSize;
                    var nextPage = allActivities.Skip(skip).Take(_pageSize).ToList();

                    if (nextPage.Any())
                    {
                        // Adaugă doar activitățile care nu există deja
                        var existingIds = _loadedActivities.Select(a => a.Id).ToHashSet();
                        var newActivities = nextPage.Where(a => !existingIds.Contains(a.Id)).ToList();

                        _loadedActivities.AddRange(newActivities);
                        _currentPage++;

                        System.Diagnostics.Debug.WriteLine($"[VirtualProxy] Loaded page {_currentPage} with {newActivities.Count} new activities");
                    }

                    if (skip + _pageSize >= allActivities.Count)
                    {
                        _isFullyLoaded = true;
                        System.Diagnostics.Debug.WriteLine($"[VirtualProxy] All activities loaded");
                    }
                }

                return _loadedActivities.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VirtualProxy] Error loading page: {ex.Message}");
                return new List<Activity>();
            }
            finally
            {
                _isLoading = false;
            }
        }

        /// <summary>
        /// Verifică dacă mai sunt activități de încărcat
        /// </summary>
        public bool HasMoreActivities()
        {
            lock (_lockObject)
            {
                return !_isFullyLoaded;
            }
        }

        /// <summary>
        /// Resetează proxy-ul (de exemplu după o căutare)
        /// </summary>
        public void Reset()
        {
            lock (_lockObject)
            {
                _loadedActivities.Clear();
                _currentPage = 0;
                _isFullyLoaded = false;
                _isLoading = false;
                System.Diagnostics.Debug.WriteLine("[VirtualProxy] Reset called");
            }
        }

        public async Task<Activity> GetActivityByIdAsync(int id)
        {
            // Caută mai întâi în activitățile încărcate
            Activity cached;
            lock (_lockObject)
            {
                cached = _loadedActivities.FirstOrDefault(a => a.Id == id);
            }

            if (cached != null)
            {
                System.Diagnostics.Debug.WriteLine($"[VirtualProxy] Activity {id} found in loaded list");
                return cached;
            }

            // Dacă nu e încărcată, o ia din sursa reală
            System.Diagnostics.Debug.WriteLine($"[VirtualProxy] Activity {id} not in loaded list, loading from real service");
            return await _realService.GetActivityByIdAsync(id);
        }

        public async Task SaveActivityAsync(Activity activity)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[VirtualProxy] Saving activity: {activity.Title}");
                await _realService.SaveActivityAsync(activity);

                lock (_lockObject)
                {
                    var existingIndex = _loadedActivities.FindIndex(a => a.Id == activity.Id);
                    if (existingIndex >= 0)
                    {
                        _loadedActivities[existingIndex] = activity;
                    }
                    else
                    {
                        _loadedActivities.Add(activity);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[VirtualProxy] Activity saved and added to loaded list");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VirtualProxy] Error saving activity: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteActivityAsync(Activity activity)
        {
            await _realService.DeleteActivityAsync(activity);

            lock (_lockObject)
            {
                var existing = _loadedActivities.FirstOrDefault(a => a.Id == activity.Id);
                if (existing != null)
                {
                    _loadedActivities.Remove(existing);
                    System.Diagnostics.Debug.WriteLine($"[VirtualProxy] Activity {activity.Title} removed from loaded list");
                }
            }
        }

        // Metodele rămase delegă direct la real service

        public Task SaveActivityWithAlarmAsync(Activity activity, IAlarmService alarmService)
        {
            return _realService.SaveActivityWithAlarmAsync(activity, alarmService);
        }

        public Task<List<Activity>> GetSharedActivitiesAsync(string currentUserId)
        {
            return _realService.GetSharedActivitiesAsync(currentUserId);
        }

        public Task<Activity> CopySharedActivityAsync(Activity sourceActivity, string newOwnerId)
        {
            return _realService.CopySharedActivityAsync(sourceActivity, newOwnerId);
        }

        public Task<List<Category>> GetCategoriesAsync()
        {
            return _realService.GetCategoriesAsync();
        }

        public Task<Category> GetCategoryByIdAsync(int id)
        {
            return _realService.GetCategoryByIdAsync(id);
        }

        public Task<int> SaveCategoryAsync(Category category)
        {
            return _realService.SaveCategoryAsync(category);
        }

        public Task<int> DeleteCategoryAsync(Category category)
        {
            return _realService.DeleteCategoryAsync(category);
        }

        public Task MoveActivityToCategoryAsync(int activityId, int newCategoryId)
        {
            return _realService.MoveActivityToCategoryAsync(activityId, newCategoryId);
        }

        public Task<List<Category>> GetSubCategoriesAsync(int parentId)
        {
            return _realService.GetSubCategoriesAsync(parentId);
        }

        public Task<int> GetOrCreateCategoryIdAsync(string categoryName, int parentId = 0)
        {
            return _realService.GetOrCreateCategoryIdAsync(categoryName, parentId);
        }

        public async Task<int> GetTotalActivitiesCountAsync()
        {
            lock (_lockObject)
            {
                if (_isFullyLoaded)
                    return _loadedActivities.Count;
            }
            return await _realService.GetTotalActivitiesCountAsync();
        }

        public async Task<int> GetCompletedActivitiesCountAsync()
        {
            lock (_lockObject)
            {
                if (_isFullyLoaded)
                    return _loadedActivities.Count(a => a.IsCompleted);
            }
            return await _realService.GetCompletedActivitiesCountAsync();
        }
    }
}