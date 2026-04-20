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

        public async Task<ActivityCategory> BuildFromDatabaseAsync()
        {
            // Obține toate categoriile și activitățile
            var categories = await _activityService.GetCategoriesAsync();
            var activities = await _activityService.GetActivitiesAsync();

            System.Diagnostics.Debug.WriteLine($"=== Building Category Tree ===");
            System.Diagnostics.Debug.WriteLine($"Categories found: {categories.Count}");
            System.Diagnostics.Debug.WriteLine($"Activities found: {activities.Count}");

            // Creează un dicționar pentru categoriile principale
            var categoryDict = new Dictionary<int, ActivityCategory>();

            // Creează categoria rădăcină
            var root = new ActivityCategory(new Category { Id = 0, Name = "All Activities" });

            // 🔥 Creează toate categoriile
            foreach (var cat in categories)
            {
                categoryDict[cat.Id] = new ActivityCategory(cat);
                System.Diagnostics.Debug.WriteLine($"Created category: ID={cat.Id}, Name={cat.Name}");
            }

            // 🔥 Adaugă activitățile la categoriile lor
            int activitiesWithCategory = 0;
            int activitiesWithoutCategory = 0;

            foreach (var activity in activities)
            {
                System.Diagnostics.Debug.WriteLine($"Processing activity: '{activity.Title}', TypeId={activity.TypeId}, CategoryId={activity.CategoryId}");

                if (activity.CategoryId > 0 && categoryDict.ContainsKey(activity.CategoryId))
                {
                    categoryDict[activity.CategoryId].Add(new ActivityLeaf(activity));
                    activitiesWithCategory++;
                    System.Diagnostics.Debug.WriteLine($"  → Added to category ID={activity.CategoryId}");
                }
                else
                {
                    // 🔥 Dacă activitatea nu are categorie, încearcă să o categorizeze după tip
                    activitiesWithoutCategory++;
                    System.Diagnostics.Debug.WriteLine($"  → No category found! Attempting to assign by type...");

                    var defaultCategory = await GetOrCreateCategoryByTypeAsync(activity.TypeId);
                    if (defaultCategory != null)
                    {
                        activity.CategoryId = defaultCategory.Id;
                        await _activityService.SaveActivityAsync(activity);

                        if (categoryDict.ContainsKey(defaultCategory.Id))
                        {
                            categoryDict[defaultCategory.Id].Add(new ActivityLeaf(activity));
                            System.Diagnostics.Debug.WriteLine($"  → Auto-assigned to category '{defaultCategory.Name}' (ID={defaultCategory.Id})");
                        }
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"Activities with category: {activitiesWithCategory}");
            System.Diagnostics.Debug.WriteLine($"Activities without category: {activitiesWithoutCategory}");

            // 🔥 Construiește ierarhia (părinte-copil)
            foreach (var cat in categories)
            {
                if (cat.ParentCategoryId == 0)
                {
                    if (categoryDict.ContainsKey(cat.Id))
                    {
                        root.Add(categoryDict[cat.Id]);
                        System.Diagnostics.Debug.WriteLine($"Added root category: {cat.Name}");
                    }
                }
                else if (categoryDict.ContainsKey(cat.ParentCategoryId) && categoryDict.ContainsKey(cat.Id))
                {
                    categoryDict[cat.ParentCategoryId].Add(categoryDict[cat.Id]);
                    System.Diagnostics.Debug.WriteLine($"Added subcategory: {cat.Name} under parent ID={cat.ParentCategoryId}");
                }
            }

            // 🔥 Asigură-te că toate categoriile implicite există
            await EnsureDefaultCategoriesExist(root, categoryDict);

            System.Diagnostics.Debug.WriteLine($"=== Category Tree Built ===");
            System.Diagnostics.Debug.WriteLine($"Root has {root.GetChildren().Count} direct children");

            // Afișează structura
            DisplayTree(root);

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