// Data/ActivityDataBase.cs
using SharedActivityManager.Models;
using SQLite;
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

            // Creare tabele
            _connection.CreateTableAsync<Activity>().Wait();
            _connection.CreateTableAsync<Category>().Wait();

            // Inițializare categorii implicite dacă nu există
            InitializeDefaultCategories();
        }

        public async Task<Category> GetCategoryByNameAsync(string name)
        {
            return await _connection.Table<Category>().FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task<int> GetOrCreateCategoryIdAsync(string categoryName, int parentId = 0)
        {
            var existing = await _connection.Table<Category>().FirstOrDefaultAsync(c => c.Name == categoryName);
            if (existing != null)
                return existing.Id;

            var newCategory = new Category
            {
                Name = categoryName,
                ParentCategoryId = parentId,
                DisplayOrder = 1
            };

            await _connection.InsertAsync(newCategory);
            return newCategory.Id;
        }

        private async void InitializeDefaultCategories()
        {
            var existing = await _connection.Table<Category>().ToListAsync();
            if (existing.Count == 0)
            {
                var defaultCategories = new List<Category>
                {
                    new Category { Id = 1, Name = "💼 Work", ParentCategoryId = 0, DisplayOrder = 1, Icon = "work.png" },
                    new Category { Id = 2, Name = "🏠 Personal", ParentCategoryId = 0, DisplayOrder = 2, Icon = "personal.png" },
                    new Category { Id = 3, Name = "💪 Health", ParentCategoryId = 0, DisplayOrder = 3, Icon = "health.png" },
                    new Category { Id = 4, Name = "📚 Study", ParentCategoryId = 0, DisplayOrder = 4, Icon = "study.png" },
                    new Category { Id = 5, Name = "🛒 Shopping", ParentCategoryId = 2, DisplayOrder = 1, Icon = "shopping.png" },
                    new Category { Id = 6, Name = "🎮 Leisure", ParentCategoryId = 2, DisplayOrder = 2, Icon = "leisure.png" },
                };

                foreach (var cat in defaultCategories)
                {
                    await _connection.InsertAsync(cat);
                }
            }
        }

        // Activity CRUD
        public Task<List<Activity>> GetActivitiesAsync()
        {
            return _connection.Table<Activity>().ToListAsync();
        }

        public Task<List<Activity>> GetActivitiesByCategoryAsync(int categoryId)
        {
            return _connection.Table<Activity>().Where(a => a.CategoryId == categoryId).ToListAsync();
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

        // Category CRUD
        public Task<List<Category>> GetCategoriesAsync()
        {
            return _connection.Table<Category>().ToListAsync();
        }

        public Task<List<Category>> GetSubCategoriesAsync(int parentId)
        {
            return _connection.Table<Category>().Where(c => c.ParentCategoryId == parentId).ToListAsync();
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
            // Șterge și subcategoriile
            var subCategories = await GetSubCategoriesAsync(category.Id);
            foreach (var sub in subCategories)
            {
                await DeleteCategoryAsync(sub);
            }

            // Mută activitățile din această categorie la categoria părinte
            var activities = await GetActivitiesByCategoryAsync(category.Id);
            foreach (var activity in activities)
            {
                activity.CategoryId = category.ParentCategoryId;
                await SaveActivityAsync(activity);
            }

            return await _connection.DeleteAsync(category);
        }

        public async Task MoveActivityToCategoryAsync(int activityId, int newCategoryId)
        {
            var activity = await _connection.FindAsync<Activity>(activityId);
            if (activity != null)
            {
                activity.CategoryId = newCategoryId;
                await _connection.UpdateAsync(activity);
            }
        }
    }
}