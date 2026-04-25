using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Proxies
{
    /// <summary>
    /// Virtual Proxy - încarcă datele treptat (lazy loading)
    /// </summary>
    public class VirtualActivityServiceProxy : IActivityService
    {
        private readonly IActivityService _realService;
        private List<Activity> _loadedActivities;
        private bool _isFullyLoaded;
        private int _pageSize = 20;
        private int _currentPage = 0;

        public VirtualActivityServiceProxy(IActivityService realService)
        {
            _realService = realService;
            _loadedActivities = new List<Activity>();
            _isFullyLoaded = false;
        }

        public async Task<List<Activity>> GetActivitiesAsync()
        {
            // Dacă nu avem activități încărcate, încarcă prima pagină
            if (_loadedActivities.Count == 0)
            {
                await LoadNextPageAsync();
            }

            System.Diagnostics.Debug.WriteLine($"[VirtualProxy] Returning {_loadedActivities.Count} loaded activities (fully loaded: {_isFullyLoaded})");
            return _loadedActivities.ToList();
        }

        /// <summary>
        /// Încarcă următoarea pagină de activități
        /// </summary>
        public async Task<List<Activity>> LoadNextPageAsync()
        {
            if (_isFullyLoaded)
            {
                System.Diagnostics.Debug.WriteLine("[VirtualProxy] Already fully loaded, no more pages");
                return _loadedActivities;
            }

            var allActivities = await _realService.GetActivitiesAsync();
            var skip = _currentPage * _pageSize;
            var nextPage = allActivities.Skip(skip).Take(_pageSize).ToList();

            if (nextPage.Any())
            {
                _loadedActivities.AddRange(nextPage);
                _currentPage++;
                System.Diagnostics.Debug.WriteLine($"[VirtualProxy] Loaded page {_currentPage} with {nextPage.Count} activities");
            }

            if (skip + _pageSize >= allActivities.Count)
            {
                _isFullyLoaded = true;
                System.Diagnostics.Debug.WriteLine("[VirtualProxy] All activities loaded");
            }

            return nextPage;
        }

        /// <summary>
        /// Verifică dacă mai sunt activități de încărcat
        /// </summary>
        public bool HasMoreActivities()
        {
            return !_isFullyLoaded;
        }

        /// <summary>
        /// Resetează proxy-ul (de exemplu după o căutare)
        /// </summary>
        public void Reset()
        {
            _loadedActivities.Clear();
            _currentPage = 0;
            _isFullyLoaded = false;
            System.Diagnostics.Debug.WriteLine("[VirtualProxy] Reset called");
        }

        public async Task<Activity> GetActivityByIdAsync(int id)
        {
            // Caută mai întâi în activitățile încărcate
            var cached = _loadedActivities.FirstOrDefault(a => a.Id == id);
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
            // Salvează în sursa reală
            await _realService.SaveActivityAsync(activity);

            // Adaugă în lista încărcată dacă nu există deja
            var existing = _loadedActivities.FirstOrDefault(a => a.Id == activity.Id);
            if (existing != null)
            {
                var index = _loadedActivities.IndexOf(existing);
                _loadedActivities[index] = activity;
            }
            else
            {
                _loadedActivities.Add(activity);
            }

            System.Diagnostics.Debug.WriteLine($"[VirtualProxy] Activity {activity.Title} saved and added to loaded list");
        }

        public async Task DeleteActivityAsync(Activity activity)
        {
            await _realService.DeleteActivityAsync(activity);

            // Elimină din lista încărcată
            var existing = _loadedActivities.FirstOrDefault(a => a.Id == activity.Id);
            if (existing != null)
            {
                _loadedActivities.Remove(existing);
                System.Diagnostics.Debug.WriteLine($"[VirtualProxy] Activity {activity.Title} removed from loaded list");
            }
        }

        // Metodele rămase delegă direct la real service
        public Task SaveActivityWithAlarmAsync(Activity activity, IAlarmService alarmService)
            => _realService.SaveActivityWithAlarmAsync(activity, alarmService);

        public Task<List<Activity>> GetSharedActivitiesAsync(string currentUserId)
            => _realService.GetSharedActivitiesAsync(currentUserId);

        public Task<Activity> CopySharedActivityAsync(Activity sourceActivity, string newOwnerId)
            => _realService.CopySharedActivityAsync(sourceActivity, newOwnerId);

        public Task<List<Category>> GetCategoriesAsync()
            => _realService.GetCategoriesAsync();

        public Task<Category> GetCategoryByIdAsync(int id)
            => _realService.GetCategoryByIdAsync(id);

        public Task<int> SaveCategoryAsync(Category category)
            => _realService.SaveCategoryAsync(category);

        public Task<int> DeleteCategoryAsync(Category category)
            => _realService.DeleteCategoryAsync(category);

        public Task MoveActivityToCategoryAsync(int activityId, int newCategoryId)
            => _realService.MoveActivityToCategoryAsync(activityId, newCategoryId);

        public Task<List<Category>> GetSubCategoriesAsync(int parentId)
            => _realService.GetSubCategoriesAsync(parentId);

        public Task<int> GetOrCreateCategoryIdAsync(string categoryName, int parentId = 0)
        {
            return _realService.GetOrCreateCategoryIdAsync(categoryName, parentId);
        }

        public async Task<int> GetTotalActivitiesCountAsync()
        {
            // Dacă sunt toate încărcate, folosește lista locală
            if (_isFullyLoaded)
                return _loadedActivities.Count;

            return await _realService.GetTotalActivitiesCountAsync();
        }

        public async Task<int> GetCompletedActivitiesCountAsync()
        {
            if (_isFullyLoaded)
                return _loadedActivities.Count(a => a.IsCompleted);

            return await _realService.GetCompletedActivitiesCountAsync();
        }
    }
}