using System.Text;
using SharedActivityManager.Models;
using SharedActivityManager.Enums;
using SharedActivityManager.Data;

namespace SharedActivityManager.Services.Adapters
{
    /// <summary>
    /// Adapter pentru format CSV (Comma Separated Values)
    /// </summary>
    public class CSVAdapter : BaseFileAdapter
    {
        public override string FormatName => "CSV";
        public override string[] SupportedExtensions => new[] { ".csv", ".txt" };

        public override async Task<List<Activity>> ImportAsync(string filePath)
        {
            var activities = new List<Activity>();

            if (!FileExists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            var lines = await File.ReadAllLinesAsync(filePath, Encoding.UTF8);

            if (lines.Length <= 1)
                return activities;

            // Citește header-ul pentru a determina coloanele
            var header = lines[0].ToLower();
            bool hasCategoryId = header.Contains("categoryid");

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(',');
                if (parts.Length >= 6)
                {
                    try
                    {
                        var type = Enum.TryParse<ActivityType>(parts[2].Trim('"'), out var t) ? t : ActivityType.Other;

                        var activity = new Activity
                        {
                            Title = parts[0].Trim('"'),
                            Desc = parts[1].Trim('"'),
                            TypeId = type,
                            StartDate = DateTime.TryParse(parts[3], out var date) ? date : DateTime.Today,
                            StartTime = DateTime.TryParse(parts[4], out var time) ? time : DateTime.Now,
                            IsCompleted = parts[5].Trim() == "True",
                            AlarmSet = parts.Length > 6 ? parts[6].Trim() == "True" : false,
                            IsPublic = parts.Length > 7 ? parts[7].Trim() == "True" : false,
                            // 🔥 CategoryId - dacă există în fișier, folosește-l
                            CategoryId = (hasCategoryId && parts.Length > 8) ? int.TryParse(parts[8].Trim(), out int catId) ? catId : 0 : 0
                        };

                        // 🔥 Dacă CategoryId este 0, setează-l pe baza tipului
                        if (activity.CategoryId == 0)
                        {
                            activity.CategoryId = await GetCategoryIdForActivityType(type);
                        }

                        activities.Add(activity);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"CSV Import error at line {i}: {ex.Message}");
                    }
                }
            }

            return activities;
        }

        private async Task<int> GetCategoryIdForActivityType(ActivityType type)
        {
            var database = new ActivityDataBase();
            string categoryName = type switch
            {
                ActivityType.Work => "💼 Work",
                ActivityType.Personal => "🏠 Personal",
                ActivityType.Health => "💪 Health",
                ActivityType.Study => "📚 Study",
                _ => "Other"
            };

            var categories = await database.GetCategoriesAsync();
            var existing = categories.FirstOrDefault(c => c.Name == categoryName);

            if (existing != null)
                return existing.Id;

            var newCategory = new Category
            {
                Name = categoryName,
                ParentCategoryId = 0,
                DisplayOrder = 1
            };

            await database.SaveCategoryAsync(newCategory);
            return newCategory.Id;
        }

        public override async Task ExportAsync(string filePath, List<Activity> activities)
        {
            var sb = new StringBuilder();

            // 🔥 HEADER ACTUALIZAT - include CategoryId
            sb.AppendLine("\"Title\",\"Description\",\"Type\",\"StartDate\",\"StartTime\",\"IsCompleted\",\"AlarmSet\",\"IsPublic\",\"CategoryId\"");

            // Date
            foreach (var activity in activities)
            {
                sb.AppendLine($"\"{EscapeCsv(activity.Title)}\",\"{EscapeCsv(activity.Desc)}\",{activity.TypeId},{activity.StartDate:yyyy-MM-dd},{activity.StartTime:HH:mm:ss},{activity.IsCompleted},{activity.AlarmSet},{activity.IsPublic},{activity.CategoryId}");
            }

            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Replace("\"", "\"\"");
        }
    }
}