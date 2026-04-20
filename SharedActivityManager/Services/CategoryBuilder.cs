// Services/CategoryBuilder.cs - VERSIUNEA CORECTĂ

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedActivityManager.Models;
using SharedActivityManager.Data;
using SharedActivityManager.Enums;

namespace SharedActivityManager.Services
{
    public class CategoryBuilder
    {
        private readonly ActivityDataBase _database;

        public CategoryBuilder(ActivityDataBase database)
        {
            _database = database;
        }

        // 🔥 METODA PRINCIPALĂ - CONSTRUIEȘTE STRUCTURA DIN BAZA DE DATE
        public async Task<ActivityCategory> BuildFromDatabaseAsync()
        {
            // Obține toate categoriile și activitățile din baza de date
            var categories = await _database.GetCategoriesAsync();
            var activities = await _database.GetActivitiesAsync();

            System.Diagnostics.Debug.WriteLine($"=== Building Category Tree ===");
            System.Diagnostics.Debug.WriteLine($"Categories found: {categories.Count}");
            System.Diagnostics.Debug.WriteLine($"Activities found: {activities.Count}");

            // Creează un dicționar pentru categoriile existente
            var categoryDict = new Dictionary<int, ActivityCategory>();

            // Creează categoria rădăcină
            var root = new ActivityCategory(new Category { Id = 0, Name = "All Activities" });

            // Creează toate categoriile
            foreach (var cat in categories)
            {
                categoryDict[cat.Id] = new ActivityCategory(cat);
                System.Diagnostics.Debug.WriteLine($"Created category: ID={cat.Id}, Name={cat.Name}");
            }

            // Adaugă activitățile la categoriile lor
            int activitiesWithCategory = 0;
            int activitiesWithoutCategory = 0;

            foreach (var activity in activities)
            {
                if (activity.CategoryId > 0 && categoryDict.ContainsKey(activity.CategoryId))
                {
                    categoryDict[activity.CategoryId].Add(new ActivityLeaf(activity));
                    activitiesWithCategory++;
                    System.Diagnostics.Debug.WriteLine($"Added activity '{activity.Title}' to category ID={activity.CategoryId}");
                }
                else
                {
                    activitiesWithoutCategory++;
                    System.Diagnostics.Debug.WriteLine($"Activity '{activity.Title}' has no category (CategoryId={activity.CategoryId})");

                    // Dacă activitatea nu are categorie, încearcă să o categorizeze după tip
                    var defaultCategory = await GetOrCreateCategoryByTypeAsync(activity.TypeId);
                    if (defaultCategory != null)
                    {
                        activity.CategoryId = defaultCategory.Id;
                        await _database.SaveActivityAsync(activity);

                        if (categoryDict.ContainsKey(defaultCategory.Id))
                        {
                            categoryDict[defaultCategory.Id].Add(new ActivityLeaf(activity));
                            System.Diagnostics.Debug.WriteLine($"Auto-assigned activity '{activity.Title}' to category '{defaultCategory.Name}'");
                        }
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"Activities with category: {activitiesWithCategory}");
            System.Diagnostics.Debug.WriteLine($"Activities without category: {activitiesWithoutCategory}");

            // Construiește ierarhia (părinte-copil)
            foreach (var cat in categories)
            {
                if (cat.ParentCategoryId == 0)
                {
                    // Categorie rădăcină - adaugă direct la root
                    if (categoryDict.ContainsKey(cat.Id))
                    {
                        root.Add(categoryDict[cat.Id]);
                    }
                }
                else if (categoryDict.ContainsKey(cat.ParentCategoryId) && categoryDict.ContainsKey(cat.Id))
                {
                    // Subcategorie - adaugă la părinte
                    categoryDict[cat.ParentCategoryId].Add(categoryDict[cat.Id]);
                }
            }

            // Adaugă și categoriile implicite dacă nu există
            await EnsureDefaultCategoriesExist(root, categoryDict);

            System.Diagnostics.Debug.WriteLine($"=== Category Tree Built ===");
            System.Diagnostics.Debug.WriteLine($"Root has {root.GetChildren().Count} direct children");

            return root;
        }

        // 🔥 METODĂ PENTRU A ASIGURA EXISTENȚA CATEGORIILOR IMPLICITE
        private async Task EnsureDefaultCategoriesExist(ActivityCategory root, Dictionary<int, ActivityCategory> categoryDict)
        {
            var defaultCategories = new[] { "💼 Work", "🏠 Personal", "💪 Health", "📚 Study" };

            foreach (var catName in defaultCategories)
            {
                var existing = categoryDict.Values.FirstOrDefault(c => c.Name == catName);
                if (existing == null)
                {
                    var newCategory = new Category { Name = catName, ParentCategoryId = 0, DisplayOrder = 1 };
                    await _database.SaveCategoryAsync(newCategory);

                    var newActivityCat = new ActivityCategory(newCategory);
                    categoryDict[newCategory.Id] = newActivityCat;
                    root.Add(newActivityCat);

                    System.Diagnostics.Debug.WriteLine($"Created default category: {catName}");
                }
            }
        }

        // 🔥 METODĂ PENTRU A OBȚINE SAU CREA CATEGORIA DUPĂ TIP
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

            var categories = await _database.GetCategoriesAsync();
            var existing = categories.FirstOrDefault(c => c.Name == categoryName);

            if (existing != null)
                return existing;

            var newCategory = new Category { Name = categoryName, ParentCategoryId = 0, DisplayOrder = 1 };
            await _database.SaveCategoryAsync(newCategory);
            return newCategory;
        }

        // 🔥 METODĂ PENTRU DEBUG - AFIȘEAZĂ STRUCTURA
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
    }
}