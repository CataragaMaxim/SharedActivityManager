// Services/IActivityService.cs
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Models;

namespace SharedActivityManager.Services
{
    public interface IActivityService
    {
        // Activity methods
        Task<List<Activity>> GetActivitiesAsync();
        Task<Activity> GetActivityByIdAsync(int id);
        Task SaveActivityAsync(Activity activity);
        Task DeleteActivityAsync(Activity activity);
        Task SaveActivityWithAlarmAsync(Activity activity, IAlarmService alarmService);
        Task<List<Activity>> GetSharedActivitiesAsync(string currentUserId);
        Task<Activity> CopySharedActivityAsync(Activity sourceActivity, string newOwnerId);

        // Category methods
        Task<List<Category>> GetCategoriesAsync();
        Task<Category> GetCategoryByIdAsync(int id);
        Task<int> SaveCategoryAsync(Category category);
        Task<int> DeleteCategoryAsync(Category category);
        Task MoveActivityToCategoryAsync(int activityId, int newCategoryId);
        Task<List<Category>> GetSubCategoriesAsync(int parentId);
        Task<int> GetOrCreateCategoryIdAsync(string categoryName, int parentId = 0);

        // Statistics
        Task<int> GetTotalActivitiesCountAsync();
        Task<int> GetCompletedActivitiesCountAsync();
    }
}