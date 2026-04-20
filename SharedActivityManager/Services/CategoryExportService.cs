using System.Text.Json;
using SharedActivityManager.Enums;
using SharedActivityManager.Models;

namespace SharedActivityManager.Services
{
    public class CategoryExportService
    {
        private readonly IActivityService _activityService;

        public CategoryExportService(IActivityService activityService)
        {
            _activityService = activityService;
        }

        // Clase pentru serializare
        private class ExportCategory
        {
            public string Type { get; set; } = "category";
            public int Id { get; set; }
            public string Name { get; set; }
            public int ParentId { get; set; }
            public int DisplayOrder { get; set; }
            public List<object> Children { get; set; } = new();
        }

        private class ExportActivity
        {
            public string Type { get; set; } = "activity";
            public int Id { get; set; }
            public string Title { get; set; }
            public string Desc { get; set; }
            public string TypeId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime StartTime { get; set; }
            public bool IsCompleted { get; set; }
        }

        /// <summary>
        /// Exportă structura completă în JSON
        /// </summary>
        public async Task<string> ExportToJsonAsync(ActivityCategory root)
        {
            var exportData = new
            {
                ExportDate = DateTime.Now,
                Version = "1.0",
                Categories = await ExportCategoryAsync(root)
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(exportData, options);
        }

        private async Task<List<object>> ExportCategoryAsync(ActivityCategory category)
        {
            var result = new List<object>();

            // Adaugă categoria curentă
            var catData = new ExportCategory
            {
                Id = category.CategoryId,
                Name = category.Name,
                ParentId = category.GetCategory().ParentCategoryId,
                DisplayOrder = category.GetCategory().DisplayOrder
            };

            // Procesează copiii
            foreach (var child in category.GetChildren())
            {
                if (child is ActivityCategory subCat)
                {
                    var subCatData = new ExportCategory
                    {
                        Id = subCat.CategoryId,
                        Name = subCat.Name,
                        ParentId = subCat.GetCategory().ParentCategoryId,
                        DisplayOrder = subCat.GetCategory().DisplayOrder,
                        Children = await ExportCategoryAsync(subCat)
                    };
                    catData.Children.Add(subCatData);
                }
                else if (child is ActivityLeaf leaf)
                {
                    var activity = leaf.GetActivity();
                    var activityData = new ExportActivity
                    {
                        Id = activity.Id,
                        Title = activity.Title,
                        Desc = activity.Desc,
                        TypeId = activity.TypeId.ToString(),
                        StartDate = activity.StartDate,
                        StartTime = activity.StartTime,
                        IsCompleted = activity.IsCompleted
                    };
                    catData.Children.Add(activityData);
                }
            }

            result.Add(catData);
            return result;
        }

        /// <summary>
        /// Importă structura din JSON
        /// </summary>
        public async Task<ActivityCategory> ImportFromJsonAsync(string json)
        {
            try
            {
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Creează categoria rădăcină
                var rootCategory = new ActivityCategory(new Category { Id = 0, Name = "All Activities" });

                // Procesează categoriile
                if (root.TryGetProperty("Categories", out var categoriesElement))
                {
                    foreach (var catElement in categoriesElement.EnumerateArray())
                    {
                        await ImportCategoryAsync(catElement, rootCategory);
                    }
                }

                return rootCategory;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Import error: {ex.Message}");
                throw;
            }
        }

        private async Task ImportCategoryAsync(JsonElement element, ActivityCategory parent)
        {
            var name = element.GetProperty("Name").GetString();
            var displayOrder = element.GetProperty("DisplayOrder").GetInt32();

            // Creează categoria în baza de date
            var category = new Category
            {
                Name = name,
                ParentCategoryId = parent.CategoryId,
                DisplayOrder = displayOrder
            };

            await _activityService.SaveCategoryAsync(category);
            var newCategory = new ActivityCategory(category);
            parent.Add(newCategory);

            // Procesează copiii
            if (element.TryGetProperty("Children", out var children))
            {
                foreach (var child in children.EnumerateArray())
                {
                    var type = child.GetProperty("Type").GetString();

                    if (type == "category")
                    {
                        await ImportCategoryAsync(child, newCategory);
                    }
                    else if (type == "activity")
                    {
                        await ImportActivityAsync(child, newCategory);
                    }
                }
            }
        }

        private async Task ImportActivityAsync(JsonElement element, ActivityCategory parent)
        {
            var activity = new Activity
            {
                Title = element.GetProperty("Title").GetString(),
                Desc = element.GetProperty("Desc").GetString(),
                TypeId = Enum.Parse<ActivityType>(element.GetProperty("TypeId").GetString()),
                StartDate = element.GetProperty("StartDate").GetDateTime(),
                StartTime = element.GetProperty("StartTime").GetDateTime(),
                IsCompleted = element.GetProperty("IsCompleted").GetBoolean(),
                CategoryId = parent.CategoryId,
                AlarmSet = false,
                ReminderType = ReminderType.None,
                RingTone = "Default Alarm"
            };

            await _activityService.SaveActivityAsync(activity);
            parent.Add(new ActivityLeaf(activity));
        }
    }
}