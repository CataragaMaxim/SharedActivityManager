// Services/CategoryTreeBuilder.cs
using SharedActivityManager.Models;
using SharedActivityManager.Data;
using SharedActivityManager.Enums;

namespace SharedActivityManager.Services
{
    public class CategoryTreeBuilder
    {
        private readonly IActivityService _activityService;

        public CategoryTreeBuilder(IActivityService activityService)
        {
            _activityService = activityService;
        }

        // Services/CategoryTreeBuilder.cs - metoda BuildFromDatabaseAsync

        public async Task<ActivityCategory> BuildFromDatabaseAsync()
        {
            // Obține categoriile și activitățile
            var categories = await _activityService.GetCategoriesAsync();
            var activities = await _activityService.GetActivitiesAsync();

            // 🔥 ELIMINĂ DUBLICATELE DIN CATEGORII
            var uniqueCategories = categories
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .ToList();

            System.Diagnostics.Debug.WriteLine($"=== Building Category Tree ===");
            System.Diagnostics.Debug.WriteLine($"Original categories: {categories.Count}");
            System.Diagnostics.Debug.WriteLine($"Unique categories: {uniqueCategories.Count}");
            System.Diagnostics.Debug.WriteLine($"Activities: {activities.Count}");

            // Creează dicționar pentru categorii unice
            var categoryDict = new Dictionary<int, ActivityCategory>();
            var root = new ActivityCategory(new Category { Id = 0, Name = "All Activities" });

            // 🔥 CREEAREA CATEGORIILOR (doar cele unice)
            foreach (var cat in uniqueCategories)
            {
                if (!categoryDict.ContainsKey(cat.Id))
                {
                    categoryDict[cat.Id] = new ActivityCategory(cat);
                    System.Diagnostics.Debug.WriteLine($"Added category: ID={cat.Id}, Name={cat.Name}");
                }
            }

            // 🔥 ADĂUGĂ ACTIVITĂȚILE (evită duplicate)
            foreach (var activity in activities)
            {
                if (activity.CategoryId > 0 && categoryDict.ContainsKey(activity.CategoryId))
                {
                    // Verifică dacă activitatea nu există deja
                    var existingActivities = categoryDict[activity.CategoryId].GetAllActivities();
                    if (!existingActivities.Any(a => a.Id == activity.Id))
                    {
                        categoryDict[activity.CategoryId].Add(new ActivityLeaf(activity));
                        System.Diagnostics.Debug.WriteLine($"Added activity '{activity.Title}' to category ID={activity.CategoryId}");
                    }
                }
            }

            // 🔥 CONSTRUIEȘTE IERARHIA (doar relații unice)
            var processedRelations = new HashSet<string>();

            foreach (var cat in uniqueCategories)
            {
                if (cat.ParentCategoryId == 0)
                {
                    if (categoryDict.ContainsKey(cat.Id) && !root.GetChildren().Any(c => c.Id == cat.Id.ToString()))
                    {
                        root.Add(categoryDict[cat.Id]);
                    }
                }
                else if (categoryDict.ContainsKey(cat.ParentCategoryId) && categoryDict.ContainsKey(cat.Id))
                {
                    var relationKey = $"{cat.ParentCategoryId}_{cat.Id}";
                    if (!processedRelations.Contains(relationKey))
                    {
                        processedRelations.Add(relationKey);
                        categoryDict[cat.ParentCategoryId].Add(categoryDict[cat.Id]);
                    }
                }
            }

            return root;
        }

        private async Task<Category> GetOrCreateCategoryByTypeAsync(ActivityType type)
        {
            string categoryName = type switch
            {
                ActivityType.Work => "💼 Work",
                ActivityType.Personal => "🏠 Personal",
                ActivityType.Health => "💪 Health",
                ActivityType.Study => "📚 Study",
                _ => "Other"
            };

            var categories = await _activityService.GetCategoriesAsync();
            var existing = categories.FirstOrDefault(c => c.Name == categoryName);

            if (existing != null)
                return existing;

            var newCategory = new Category
            {
                Name = categoryName,
                ParentCategoryId = 0,
                DisplayOrder = 1
            };

            await _activityService.SaveCategoryAsync(newCategory);
            System.Diagnostics.Debug.WriteLine($"Created new category: {categoryName}");
            return newCategory;
        }

        private async Task EnsureDefaultCategoriesExist(ActivityCategory root, Dictionary<int, ActivityCategory> categoryDict)
        {
            var defaultCategories = new[] { "💼 Work", "🏠 Personal", "💪 Health", "📚 Study" };

            foreach (var catName in defaultCategories)
            {
                var existing = categoryDict.Values.FirstOrDefault(c => c.Name == catName);
                if (existing == null)
                {
                    var newCategory = new Category { Name = catName, ParentCategoryId = 0, DisplayOrder = 1 };
                    await _activityService.SaveCategoryAsync(newCategory);

                    var newActivityCat = new ActivityCategory(newCategory);
                    categoryDict[newCategory.Id] = newActivityCat;
                    root.Add(newActivityCat);

                    System.Diagnostics.Debug.WriteLine($"Created default category: {catName}");
                }
            }
        }

        public void DisplayTree(ActivityCategory category, int level = 0)
        {
            var indent = new string(' ', level * 2);
            System.Diagnostics.Debug.WriteLine($"{indent}📁 {category.Name} ({category.GetTotalActivitiesCount()} activities)");

            foreach (var child in category.GetChildren())
            {
                if (child is ActivityCategory subCat)
                {
                    DisplayTree(subCat, level + 1);
                }
                else if (child is ActivityLeaf leaf)
                {
                    System.Diagnostics.Debug.WriteLine($"{indent}  📌 {leaf.Name}");
                }
            }
        }

        public async Task MoveActivityToCategoryAsync(int activityId, int newCategoryId)
        {
            await _activityService.MoveActivityToCategoryAsync(activityId, newCategoryId);
        }

        public async Task<Category> CreateCategoryAsync(string name, int parentId = 0)
        {
            var newCategory = new Category
            {
                Name = name,
                ParentCategoryId = parentId,
                DisplayOrder = await GetNextDisplayOrder(parentId)
            };

            await _activityService.SaveCategoryAsync(newCategory);
            return newCategory;
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            var category = (await _activityService.GetCategoriesAsync()).FirstOrDefault(c => c.Id == categoryId);
            if (category != null)
            {
                await _activityService.DeleteCategoryAsync(category);
            }
        }

        private async Task<int> GetNextDisplayOrder(int parentId)
        {
            var siblings = await _activityService.GetSubCategoriesAsync(parentId);
            return siblings.Count > 0 ? siblings.Max(s => s.DisplayOrder) + 1 : 1;
        }
    }
}