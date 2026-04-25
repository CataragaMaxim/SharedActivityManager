using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Models;
using SharedActivityManager.Repositories;

namespace SharedActivityManager.Services
{
    /// <summary>
    /// Real Subject - obiectul real care face munca efectivă
    /// </summary>
    public class RealActivityService : IActivityService
    {
        private readonly IActivityRepository _repository;
        private readonly IAlarmService _alarmService;

        public RealActivityService(IActivityRepository repository, IAlarmService alarmService)
        {
            _repository = repository;
            _alarmService = alarmService;
        }

        public async Task<List<Activity>> GetActivitiesAsync()
        {
            System.Diagnostics.Debug.WriteLine("[RealService] Getting all activities from database");
            return await _repository.GetActivitiesAsync();
        }

        public async Task<Activity> GetActivityByIdAsync(int id)
        {
            System.Diagnostics.Debug.WriteLine($"[RealService] Getting activity by ID: {id}");
            return await _repository.GetActivityByIdAsync(id);
        }

        public async Task SaveActivityAsync(Activity activity)
        {
            System.Diagnostics.Debug.WriteLine($"[RealService] Saving activity: {activity.Title}");
            await _repository.SaveActivityAsync(activity);
        }

        public async Task DeleteActivityAsync(Activity activity)
        {
            System.Diagnostics.Debug.WriteLine($"[RealService] Deleting activity: {activity.Title}");
            await _repository.DeleteActivityAsync(activity);
        }

        public async Task SaveActivityWithAlarmAsync(Activity activity, IAlarmService alarmService)
        {
            System.Diagnostics.Debug.WriteLine($"[RealService] Saving activity with alarm: {activity.Title}");
            await _repository.SaveActivityAsync(activity);

            if (activity.AlarmSet && !activity.IsCompleted)
            {
                await alarmService.ScheduleAlarmAsync(activity);
            }
        }

        public async Task<List<Activity>> GetSharedActivitiesAsync(string currentUserId)
        {
            System.Diagnostics.Debug.WriteLine($"[RealService] Getting shared activities for user: {currentUserId}");
            return await _repository.GetPublicActivitiesAsync(currentUserId);
        }

        public async Task<Activity> CopySharedActivityAsync(Activity sourceActivity, string newOwnerId)
        {
            System.Diagnostics.Debug.WriteLine($"[RealService] Copying shared activity: {sourceActivity.Title}");
            var copy = sourceActivity.DeepCopy();
            copy.OwnerId = newOwnerId;
            copy.IsPublic = false;
            copy.OriginalActivityId = sourceActivity.Id;

            await _repository.SaveActivityAsync(copy);
            return copy;
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            System.Diagnostics.Debug.WriteLine("[RealService] Getting all categories");
            return await _repository.GetCategoriesAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            System.Diagnostics.Debug.WriteLine($"[RealService] Getting category by ID: {id}");
            return await _repository.GetCategoryByIdAsync(id);
        }

        public async Task<int> SaveCategoryAsync(Category category)
        {
            System.Diagnostics.Debug.WriteLine($"[RealService] Saving category: {category.Name}");
            return await _repository.SaveCategoryAsync(category);
        }

        public async Task<int> DeleteCategoryAsync(Category category)
        {
            System.Diagnostics.Debug.WriteLine($"[RealService] Deleting category: {category.Name}");
            return await _repository.DeleteCategoryAsync(category);
        }

        public async Task MoveActivityToCategoryAsync(int activityId, int newCategoryId)
        {
            System.Diagnostics.Debug.WriteLine($"[RealService] Moving activity {activityId} to category {newCategoryId}");
            await _repository.MoveActivityToCategoryAsync(activityId, newCategoryId);
        }

        public async Task<List<Category>> GetSubCategoriesAsync(int parentId)
        {
            System.Diagnostics.Debug.WriteLine($"[RealService] Getting subcategories for parent: {parentId}");
            return await _repository.GetSubCategoriesAsync(parentId);
        }

        public async Task<int> GetOrCreateCategoryIdAsync(string categoryName, int parentId = 0)
        {
            var categories = await GetCategoriesAsync();
            var existing = categories.FirstOrDefault(c => c.Name == categoryName);

            if (existing != null)
                return existing.Id;

            var newCategory = new Category
            {
                Name = categoryName,
                ParentCategoryId = parentId,
                DisplayOrder = 1
            };

            await SaveCategoryAsync(newCategory);
            return newCategory.Id;
        }

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