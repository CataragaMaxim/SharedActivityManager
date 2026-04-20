using System.Text.Json;
using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Adapters
{
    /// <summary>
    /// Adapter pentru format JSON
    /// </summary>
    public class JSONAdapter : BaseFileAdapter
    {
        public override string FormatName => "JSON";
        public override string[] SupportedExtensions => new[] { ".json" };

        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public override async Task<List<Activity>> ImportAsync(string filePath)
        {
            if (!FileExists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            var json = await File.ReadAllTextAsync(filePath);

            // Suportă atât listă directă, cât și obiect cu proprietatea "Activities"
            try
            {
                var activities = JsonSerializer.Deserialize<List<Activity>>(json, _options);
                if (activities != null)
                    return activities;
            }
            catch { }

            try
            {
                var wrapper = JsonSerializer.Deserialize<ActivityWrapper>(json, _options);
                if (wrapper?.Activities != null)
                    return wrapper.Activities;
            }
            catch { }

            return new List<Activity>();
        }

        public override async Task ExportAsync(string filePath, List<Activity> activities)
        {
            var wrapper = new ActivityWrapper
            {
                ExportDate = DateTime.Now,
                Version = "1.0",
                ActivityCount = activities.Count,
                Activities = activities
            };

            var json = JsonSerializer.Serialize(wrapper, _options);
            await File.WriteAllTextAsync(filePath, json);
        }

        private class ActivityWrapper
        {
            public DateTime ExportDate { get; set; }
            public string Version { get; set; }
            public int ActivityCount { get; set; }
            public List<Activity> Activities { get; set; }
        }
    }
}