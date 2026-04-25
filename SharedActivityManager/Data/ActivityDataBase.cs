using SharedActivityManager.Models;
using SQLite;

namespace SharedActivityManager.Data
{
    public class ActivityDataBase
    {
        private readonly SQLiteAsyncConnection _connection;

        public ActivityDataBase()
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "Activity.db");
            _connection = new SQLiteAsyncConnection(dbPath);
            _connection.CreateTableAsync<Activity>().Wait();
            _connection.CreateTableAsync<Category>().Wait();
        }

        // ========== ACTIVITY METHODS ==========

        public Task<List<Activity>> GetActivitiesAsync()
        {
            return _connection.Table<Activity>().ToListAsync();
        }

        public async Task<Activity> GetActivityByIdAsync(int id)
        {
            return await _connection.Table<Activity>().FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<int> InsertActivityAsync(Activity activity)
        {
            return await _connection.InsertAsync(activity);
        }

        public async Task<int> UpdateActivityAsync(Activity activity)
        {
            return await _connection.UpdateAsync(activity);
        }

        public async Task<int> SaveActivityAsync(Activity activity)
        {
            if (activity.Id == 0)
                return await _connection.InsertAsync(activity);
            else
                return await _connection.UpdateAsync(activity);
        }

        public Task<int> DeleteActivityAsync(Activity activity)
        {
            return _connection.DeleteAsync(activity);
        }

        // ========== CATEGORY METHODS ==========

        public Task<List<Category>> GetCategoriesAsync()
        {
            return _connection.Table<Category>().ToListAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            return await _connection.Table<Category>().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<int> SaveCategoryAsync(Category category)
        {
            if (category.Id == 0)
                return await _connection.InsertAsync(category);
            else
                return await _connection.UpdateAsync(category);
        }

        public async Task<int> DeleteCategoryAsync(Category category)
        {
            return await _connection.DeleteAsync(category);
        }

        public Task<List<Category>> GetSubCategoriesAsync(int parentId)
        {
            return _connection.Table<Category>().Where(c => c.ParentCategoryId == parentId).ToListAsync();
        }

        public async Task MoveActivityToCategoryAsync(int activityId, int newCategoryId)
        {
            var activity = await GetActivityByIdAsync(activityId);
            if (activity != null)
            {
                activity.CategoryId = newCategoryId;
                await UpdateActivityAsync(activity);
            }
        }

        // ========== INITIALIZATION ==========

        private async void InitializeDefaultCategories()
        {
            var existing = await GetCategoriesAsync();
            if (existing.Count == 0)
            {
                var defaultCategories = new List<Category>
                {
                    new Category { Id = 1, Name = "💼 Work", ParentCategoryId = 0, DisplayOrder = 1 },
                    new Category { Id = 2, Name = "🏠 Personal", ParentCategoryId = 0, DisplayOrder = 2 },
                    new Category { Id = 3, Name = "💪 Health", ParentCategoryId = 0, DisplayOrder = 3 },
                    new Category { Id = 4, Name = "📚 Study", ParentCategoryId = 0, DisplayOrder = 4 }
                };

                foreach (var cat in defaultCategories)
                {
                    await SaveCategoryAsync(cat);
                }
            }
        }
    }
}