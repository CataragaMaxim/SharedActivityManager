// Services/IActivityService.cs
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Models;

namespace SharedActivityManager.Services
{
    public interface IActivityService
    {
        // Metode existente
        Task<List<Activity>> GetActivitiesAsync();
        Task<Activity> GetActivityByIdAsync(int id);
        Task SaveActivityAsync(Activity activity);
        Task DeleteActivityAsync(Activity activity);
        Task SaveActivityWithAlarmAsync(Activity activity, IAlarmService alarmService);
        Task<List<Activity>> GetSharedActivitiesAsync(string currentUserId);
        Task<Activity> CopySharedActivityAsync(Activity sourceActivity, string newOwnerId);

        // Metode pentru categorii
        Task<List<Category>> GetCategoriesAsync();
        Task<Category> GetCategoryByIdAsync(int id);
        Task<int> SaveCategoryAsync(Category category);
        Task<int> DeleteCategoryAsync(Category category);
        Task MoveActivityToCategoryAsync(int activityId, int newCategoryId);

        // 🔥 METODĂ NOUĂ
        Task<List<Category>> GetSubCategoriesAsync(int parentId);
    }
}