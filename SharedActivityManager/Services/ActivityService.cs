// Services/ActivityService.cs
using SharedActivityManager.Models;
using SharedActivityManager.Repositories;
using SharedActivityManager.Abstracts.Platforms;

namespace SharedActivityManager.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IActivityRepository _repository;

        public ActivityService(IActivityRepository repository)
        {
            _repository = repository;
        }

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
            await _repository.SaveActivityAsync(activity);
        }

        public async Task DeleteActivityAsync(Activity activity)
        {
            await _repository.DeleteActivityAsync(activity);
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _repository.GetCategoriesAsync();
        }

        public async Task<List<Category>> GetSubCategoriesAsync(int parentId)
        {
            return await _repository.GetSubCategoriesAsync(parentId);
        }


        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            return await _repository.GetCategoryByIdAsync(id);
        }

        public async Task<int> SaveCategoryAsync(Category category)
        {
            return await _repository.SaveCategoryAsync(category);
        }

        public async Task<int> DeleteCategoryAsync(Category category)
        {
            return await _repository.DeleteCategoryAsync(category);
        }

        public async Task MoveActivityToCategoryAsync(int activityId, int newCategoryId)
        {
            await _repository.MoveActivityToCategoryAsync(activityId, newCategoryId);
        }

        public async Task SaveActivityWithAlarmAsync(Activity activity, IAlarmService alarmService)
        {
            // Salvează activitatea
            await _repository.SaveActivityAsync(activity);

            // Programează alarma dacă e cazul
            if (activity.AlarmSet && !activity.isCompleted)
            {
                await alarmService.ScheduleAlarmAsync(activity);
            }
        }

        public async Task<List<Activity>> GetSharedActivitiesAsync(string currentUserId)
        {
            return await _repository.GetPublicActivitiesAsync(currentUserId);
        }

        public async Task<Activity> CopySharedActivityAsync(Activity sourceActivity, string newOwnerId)
        {
            // PROTOTYPE PATTERN: Deep Copy
            var copy = sourceActivity.DeepCopy();

            // Configurează copia
            copy.OwnerId = newOwnerId;
            copy.IsPublic = false;
            copy.AlarmSet = false;
            copy.OriginalActivityId = sourceActivity.Id;

            // Salvează copia
            await _repository.SaveActivityAsync(copy);

            return copy;
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
    }
}