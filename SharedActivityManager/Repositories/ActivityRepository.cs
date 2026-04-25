using SharedActivityManager.Data;
using SharedActivityManager.Models;
using SharedActivityManager.Repositories;

namespace SharedActivityManager.Repositories
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly ActivityDataBase _database;

        public ActivityRepository()
        {
            _database = new ActivityDataBase();
        }

        public async Task<List<Activity>> GetActivitiesAsync()
        {
            return await _database.GetActivitiesAsync();
        }

        public async Task<Activity> GetActivityByIdAsync(int id)
        {
            var activities = await _database.GetActivitiesAsync();
            return activities.FirstOrDefault(a => a.Id == id);
        }

        public async Task<int> SaveActivityAsync(Activity activity)
        {
            // 🔥 FOLOSEȘTE _database, NU _connection
            // Verifică dacă activitatea există deja
            if (activity.Id > 0)
            {
                var existing = await _database.GetActivityByIdAsync(activity.Id);
                if (existing != null)
                {
                    // Actualizează activitatea existentă
                    return await _database.UpdateActivityAsync(activity);
                }
            }

            // Activitate nouă - inserează
            return await _database.InsertActivityAsync(activity);
        }

        public async Task<int> DeleteActivityAsync(Activity activity)
        {
            return await _database.DeleteActivityAsync(activity);
        }

        public async Task<List<Category>> GetSubCategoriesAsync(int parentId)
        {
            return await _database.GetSubCategoriesAsync(parentId);
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _database.GetCategoriesAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            var categories = await _database.GetCategoriesAsync();
            return categories.FirstOrDefault(c => c.Id == id);
        }

        public async Task<int> SaveCategoryAsync(Category category)
        {
            return await _database.SaveCategoryAsync(category);
        }

        public async Task<int> DeleteCategoryAsync(Category category)
        {
            return await _database.DeleteCategoryAsync(category);
        }

        public async Task MoveActivityToCategoryAsync(int activityId, int newCategoryId)
        {
            await _database.MoveActivityToCategoryAsync(activityId, newCategoryId);
        }

        public async Task<List<Activity>> GetPublicActivitiesAsync(string excludeUserId)
        {
            var allActivities = await GetActivitiesAsync();
            return allActivities
                .Where(a => a.IsPublic && a.OwnerId != excludeUserId)
                .ToList();
        }
    }
}