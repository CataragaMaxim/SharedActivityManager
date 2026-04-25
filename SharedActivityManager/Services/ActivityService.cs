using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Models;
using SharedActivityManager.Repositories;
using SharedActivityManager.Services.Observers;
using System.Collections.Concurrent;

namespace SharedActivityManager.Services
{
    public class ActivityService : IActivityService, IActivitySubject
    {
        private readonly IActivityRepository _repository;
        private readonly ConcurrentBag<IActivityObserver> _observers;

        public ActivityService(IActivityRepository repository)
        {
            _repository = repository;
            _observers = new ConcurrentBag<IActivityObserver>();
        }

        // ========== OBSERVER METHODS ==========

        public void Attach(IActivityObserver observer)
        {
            _observers.Add(observer);
            System.Diagnostics.Debug.WriteLine($"[Observer] Attached: {observer.GetType().Name}");
        }

        public void Detach(IActivityObserver observer)
        {
            // ConcurrentBag nu suportă Remove direct, deci creăm o nouă colecție
            var newList = new ConcurrentBag<IActivityObserver>();
            foreach (var obs in _observers)
            {
                if (obs != observer)
                    newList.Add(obs);
            }
            while (_observers.TryTake(out _)) { }
            foreach (var obs in newList)
            {
                _observers.Add(obs);
            }
            System.Diagnostics.Debug.WriteLine($"[Observer] Detached: {observer.GetType().Name}");
        }

        public async Task NotifyObservers(string action, Activity activity = null, int activityCount = 0)
        {
            System.Diagnostics.Debug.WriteLine($"[Observer] Notifying {_observers.Count} observers: Action={action}");

            var tasks = _observers.Select(observer => observer.OnActivityChanged(action, activity, activityCount));
            await Task.WhenAll(tasks);
        }

        // ========== ACTIVITY METHODS ==========

        public async Task<List<Activity>> GetActivitiesAsync()
        {
            return await _repository.GetActivitiesAsync();
        }

        public async Task<Activity> GetActivityByIdAsync(int id)
        {
            return await _repository.GetActivityByIdAsync(id);
        }

        public async Task SaveActivityAsync(Activity activity)
        {
            var isNew = activity.Id == 0;
            await _repository.SaveActivityAsync(activity);

            // Notifică observer-ii
            await NotifyObservers(isNew ? "Added" : "Updated", activity);
        }

        public async Task DeleteActivityAsync(Activity activity)
        {
            await _repository.DeleteActivityAsync(activity);
            await NotifyObservers("Deleted", activity);
        }

        public async Task SaveActivityWithAlarmAsync(Activity activity, IAlarmService alarmService)
        {
            var isNew = activity.Id == 0;
            await _repository.SaveActivityAsync(activity);

            if (activity.AlarmSet && !activity.IsCompleted)
            {
                await alarmService.ScheduleAlarmAsync(activity);
            }

            await NotifyObservers(isNew ? "Added" : "Updated", activity);
        }

        public async Task<List<Activity>> GetSharedActivitiesAsync(string currentUserId)
        {
            return await _repository.GetPublicActivitiesAsync(currentUserId);
        }

        public async Task<Activity> CopySharedActivityAsync(Activity sourceActivity, string newOwnerId)
        {
            var copy = sourceActivity.DeepCopy();
            copy.OwnerId = newOwnerId;
            copy.IsPublic = false;
            copy.OriginalActivityId = sourceActivity.Id;

            await _repository.SaveActivityAsync(copy);
            await NotifyObservers("Copied", copy);

            return copy;
        }

        public async Task DeleteAllActivitiesAsync()
        {
            var activities = await GetActivitiesAsync();
            var count = activities.Count;

            foreach (var activity in activities)
            {
                await _repository.DeleteActivityAsync(activity);
            }

            await NotifyObservers("DeletedAll", null, count);
        }

        public async Task ImportActivitiesAsync(List<Activity> activities)
        {
            foreach (var activity in activities)
            {
                await _repository.SaveActivityAsync(activity);
            }

            await NotifyObservers("Imported", null, activities.Count);
        }

        // ========== CATEGORY METHODS ==========

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _repository.GetCategoriesAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            return await _repository.GetCategoryByIdAsync(id);
        }

        public async Task<int> SaveCategoryAsync(Category category)
        {
            var result = await _repository.SaveCategoryAsync(category);
            await NotifyObservers("CategoryChanged", null, 0);
            return result;
        }

        public async Task<int> DeleteCategoryAsync(Category category)
        {
            var result = await _repository.DeleteCategoryAsync(category);
            await NotifyObservers("CategoryChanged", null, 0);
            return result;
        }

        public async Task MoveActivityToCategoryAsync(int activityId, int newCategoryId)
        {
            await _repository.MoveActivityToCategoryAsync(activityId, newCategoryId);
            var activity = await GetActivityByIdAsync(activityId);
            await NotifyObservers("Updated", activity);
        }

        public async Task<List<Category>> GetSubCategoriesAsync(int parentId)
        {
            return await _repository.GetSubCategoriesAsync(parentId);
        }

        public async Task<int> GetOrCreateCategoryIdAsync(string categoryName, int parentId = 0)
        {
            var categories = await GetCategoriesAsync();
            var existing = categories.FirstOrDefault(c => c.Name == categoryName);

            if (existing != null)
                return existing.Id;

            var newCategory = new Category { Name = categoryName, ParentCategoryId = parentId, DisplayOrder = 1 };
            await SaveCategoryAsync(newCategory);
            return newCategory.Id;
        }

        // ========== STATISTICS METHODS ==========

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
    }
}