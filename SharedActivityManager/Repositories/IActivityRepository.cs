// Repositories/IActivityRepository.cs
using SharedActivityManager.Models;

namespace SharedActivityManager.Repositories
{
    public interface IActivityRepository
    {
        // Metode existente
        Task<List<Activity>> GetActivitiesAsync();
        Task<Activity> GetActivityByIdAsync(int id);
        Task<int> SaveActivityAsync(Activity activity);
        Task<int> DeleteActivityAsync(Activity activity);
        Task<List<Activity>> GetPublicActivitiesAsync(string excludeUserId);

        // 🔥 METODE NOI PENTRU CATEGORII
        Task<List<Category>> GetCategoriesAsync();
        Task<Category> GetCategoryByIdAsync(int id);
        Task<int> SaveCategoryAsync(Category category);
        Task<int> DeleteCategoryAsync(Category category);
        Task MoveActivityToCategoryAsync(int activityId, int newCategoryId);

        Task<List<Category>> GetSubCategoriesAsync(int parentId);
    }
}