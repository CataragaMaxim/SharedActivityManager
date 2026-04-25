using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Proxies
{
    /// <summary>
    /// Protection Proxy - controlează accesul pe baza permisiunilor
    /// </summary>
    public class SecurityActivityServiceProxy : IActivityService
    {
        private readonly IActivityService _realService;
        private readonly string _currentUserId;

        public SecurityActivityServiceProxy(IActivityService realService, string currentUserId)
        {
            _realService = realService;
            _currentUserId = currentUserId;
        }

        /// <summary>
        /// Verifică dacă utilizatorul curent este proprietarul activității
        /// </summary>
        private bool IsOwner(Activity activity)
        {
            return activity.OwnerId == _currentUserId;
        }

        /// <summary>
        /// Verifică dacă utilizatorul are dreptul să modifice activitatea
        /// </summary>
        private bool CanModify(Activity activity)
        {
            // Doar proprietarul poate modifica
            return IsOwner(activity);
        }

        /// <summary>
        /// Verifică dacă utilizatorul are dreptul să șteargă activitatea
        /// </summary>
        private bool CanDelete(Activity activity)
        {
            // Doar proprietarul poate șterge
            return IsOwner(activity);
        }

        public async Task<List<Activity>> GetActivitiesAsync()
        {
            System.Diagnostics.Debug.WriteLine($"[SecurityProxy] Getting activities for user: {_currentUserId}");
            var allActivities = await _realService.GetActivitiesAsync();

            // Filtrează activitățile la care utilizatorul are acces
            var accessibleActivities = allActivities.Where(a =>
                a.OwnerId == _currentUserId || a.IsPublic).ToList();

            System.Diagnostics.Debug.WriteLine($"[SecurityProxy] Returned {accessibleActivities.Count} accessible activities (out of {allActivities.Count})");
            return accessibleActivities;
        }

        public async Task<Activity> GetActivityByIdAsync(int id)
        {
            var activity = await _realService.GetActivityByIdAsync(id);

            if (activity == null)
                return null;

            // Verifică accesul
            if (activity.OwnerId != _currentUserId && !activity.IsPublic)
            {
                System.Diagnostics.Debug.WriteLine($"[SecurityProxy] Access denied to activity {id} for user {_currentUserId}");
                throw new UnauthorizedAccessException($"You don't have permission to view this activity");
            }

            System.Diagnostics.Debug.WriteLine($"[SecurityProxy] Access granted to activity {id}");
            return activity;
        }

        public async Task SaveActivityAsync(Activity activity)
        {
            // Pentru activități noi (Id == 0), tot timpul e permis
            if (activity.Id == 0)
            {
                // Asigură-te că owner-ul e setat corect
                activity.OwnerId = _currentUserId;
                System.Diagnostics.Debug.WriteLine($"[SecurityProxy] Creating new activity for user {_currentUserId}");
                await _realService.SaveActivityAsync(activity);
                return;
            }

            // Pentru activități existente, verifică permisiunea
            var existing = await _realService.GetActivityByIdAsync(activity.Id);
            if (!CanModify(existing))
            {
                System.Diagnostics.Debug.WriteLine($"[SecurityProxy] User {_currentUserId} cannot modify activity {activity.Id}");
                throw new UnauthorizedAccessException("You don't have permission to modify this activity");
            }

            System.Diagnostics.Debug.WriteLine($"[SecurityProxy] User {_currentUserId} is modifying activity {activity.Id}");
            await _realService.SaveActivityAsync(activity);
        }

        public async Task DeleteActivityAsync(Activity activity)
        {
            if (!CanDelete(activity))
            {
                System.Diagnostics.Debug.WriteLine($"[SecurityProxy] User {_currentUserId} cannot delete activity {activity.Id}");
                throw new UnauthorizedAccessException("You don't have permission to delete this activity");
            }

            System.Diagnostics.Debug.WriteLine($"[SecurityProxy] User {_currentUserId} is deleting activity {activity.Id}");
            await _realService.DeleteActivityAsync(activity);
        }

        public async Task SaveActivityWithAlarmAsync(Activity activity, IAlarmService alarmService)
        {
            if (activity.Id != 0)
            {
                var existing = await _realService.GetActivityByIdAsync(activity.Id);
                if (!CanModify(existing))
                {
                    throw new UnauthorizedAccessException("You don't have permission to modify this activity");
                }
            }

            activity.OwnerId = _currentUserId;
            await _realService.SaveActivityWithAlarmAsync(activity, alarmService);
        }

        public async Task<List<Activity>> GetSharedActivitiesAsync(string currentUserId)
        {
            // Pentru activitățile partajate, utilizatorul poate vedea doar activitățile publice
            // care nu sunt ale sale
            var allPublic = await _realService.GetSharedActivitiesAsync(currentUserId);
            return allPublic.Where(a => a.OwnerId != _currentUserId).ToList();
        }

        public async Task<Activity> CopySharedActivityAsync(Activity sourceActivity, string newOwnerId)
        {
            // Verifică dacă utilizatorul are dreptul să copieze activitatea
            if (!sourceActivity.IsPublic && sourceActivity.OwnerId != _currentUserId)
            {
                throw new UnauthorizedAccessException("You don't have permission to copy this activity");
            }

            System.Diagnostics.Debug.WriteLine($"[SecurityProxy] User {_currentUserId} copying activity {sourceActivity.Id}");
            return await _realService.CopySharedActivityAsync(sourceActivity, newOwnerId);
        }

        // Metodele pentru categorii - fără restricții speciale
        public Task<List<Category>> GetCategoriesAsync()
            => _realService.GetCategoriesAsync();

        public Task<Category> GetCategoryByIdAsync(int id)
            => _realService.GetCategoryByIdAsync(id);

        public Task<int> SaveCategoryAsync(Category category)
            => _realService.SaveCategoryAsync(category);

        public Task<int> DeleteCategoryAsync(Category category)
            => _realService.DeleteCategoryAsync(category);

        public Task MoveActivityToCategoryAsync(int activityId, int newCategoryId)
        {
            // Verifică dacă utilizatorul poate muta activitatea
            return _realService.MoveActivityToCategoryAsync(activityId, newCategoryId);
        }

        public Task<List<Category>> GetSubCategoriesAsync(int parentId)
            => _realService.GetSubCategoriesAsync(parentId);

        public async Task<int> GetTotalActivitiesCountAsync()
        {
            var activities = await GetActivitiesAsync();
            return activities.Count;
        }

        public async Task<int> GetCompletedActivitiesCountAsync()
        {
            var activities = await GetActivitiesAsync();
            return activities.Count(a => a.IsCompleted);
        }

        public async Task<int> GetOrCreateCategoryIdAsync(string categoryName, int parentId = 0)
        {
            return await _realService.GetOrCreateCategoryIdAsync(categoryName, parentId);
        }
    }
}