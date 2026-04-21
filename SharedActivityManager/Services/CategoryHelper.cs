using SharedActivityManager.Data;
using SharedActivityManager.Enums;
using SharedActivityManager.Models;

namespace SharedActivityManager.Services
{
    public class CategoryHelper
    {
        private readonly ActivityDataBase _database;

        public CategoryHelper(ActivityDataBase database)
        {
            _database = database;
        }

        public async Task<int> GetCategoryIdForActivityType(ActivityType type)
        {
            string categoryName = type switch
            {
                ActivityType.Work => "💼 Work",
                ActivityType.Personal => "🏠 Personal",
                ActivityType.Health => "💪 Health",
                ActivityType.Study => "📚 Study",
                _ => "Other"
            };

            var categories = await _database.GetCategoriesAsync();
            var existing = categories.FirstOrDefault(c => c.Name == categoryName);

            if (existing != null)
                return existing.Id;

            var newCategory = new Category
            {
                Name = categoryName,
                ParentCategoryId = 0,
                DisplayOrder = 1
            };

            await _database.SaveCategoryAsync(newCategory);
            return newCategory.Id;
        }

        public async Task FixActivitiesCategoryIds()
        {
            var activities = await _database.GetActivitiesAsync();
            var fixedCount = 0;

            foreach (var activity in activities)
            {
                if (activity.CategoryId == 0)
                {
                    activity.CategoryId = await GetCategoryIdForActivityType(activity.TypeId);
                    await _database.SaveActivityAsync(activity);
                    fixedCount++;
                }
            }

            System.Diagnostics.Debug.WriteLine($"Fixed {fixedCount} activities with missing CategoryId");
        }
    }
}