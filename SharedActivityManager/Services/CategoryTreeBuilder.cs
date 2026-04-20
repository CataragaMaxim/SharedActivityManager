using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedActivityManager.Models;
using SharedActivityManager.Data;

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
            // 🔥 FOLOSIM SERVICE, NU DATABASE DIRECT
            var categories = await _activityService.GetCategoriesAsync();
            var activities = await _activityService.GetActivitiesAsync();

            // Creează un dicționar pentru categoriile principale
            var categoryDict = new Dictionary<int, ActivityCategory>();

            // Creează categorii
            foreach (var cat in categories)
            {
                categoryDict[cat.Id] = new ActivityCategory(cat);
            }

            // Adaugă activități la categoriile lor
            foreach (var activity in activities)
            {
                if (categoryDict.ContainsKey(activity.CategoryId))
                {
                    categoryDict[activity.CategoryId].Add(new ActivityLeaf(activity));
                }
            }

            // Construiește ierarhia (părinte-copil)
            var rootCategories = new List<ActivityCategory>();

            foreach (var cat in categories)
            {
                if (cat.ParentCategoryId == 0)
                {
                    rootCategories.Add(categoryDict[cat.Id]);
                }
                else if (categoryDict.ContainsKey(cat.ParentCategoryId))
                {
                    categoryDict[cat.ParentCategoryId].Add(categoryDict[cat.Id]);
                }
            }

            // Creează rădăcina principală
            var root = new ActivityCategory(new Category { Id = 0, Name = "All Activities" });
            foreach (var rootCat in rootCategories.OrderBy(c => c.GetCategory().DisplayOrder))
            {
                root.Add(rootCat);
            }

            return root;
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