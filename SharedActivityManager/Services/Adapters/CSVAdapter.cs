using System.Text;
using SharedActivityManager.Models;
using SharedActivityManager.Enums;

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

            // Prima linie e header-ul
            if (lines.Length <= 1)
                return activities;

            // Salt peste header
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
                        var activity = new Activity
                        {
                            Title = parts[0].Trim('"'),
                            Desc = parts[1].Trim('"'),
                            TypeId = Enum.TryParse<ActivityType>(parts[2], out var type) ? type : ActivityType.Other,
                            StartDate = DateTime.TryParse(parts[3], out var date) ? date : DateTime.Today,
                            StartTime = DateTime.TryParse(parts[4], out var time) ? time : DateTime.Now,
                            IsCompleted = parts[5].Trim() == "True",
                            AlarmSet = parts.Length > 6 ? parts[6].Trim() == "True" : false,
                            IsPublic = parts.Length > 7 ? parts[7].Trim() == "True" : false
                        };
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

        public override async Task ExportAsync(string filePath, List<Activity> activities)
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("\"Title\",\"Description\",\"Type\",\"StartDate\",\"StartTime\",\"IsCompleted\",\"AlarmSet\",\"IsPublic\"");

            // Date
            foreach (var activity in activities)
            {
                sb.AppendLine($"\"{EscapeCsv(activity.Title)}\",\"{EscapeCsv(activity.Desc)}\",{activity.TypeId},{activity.StartDate:yyyy-MM-dd},{activity.StartTime:HH:mm:ss},{activity.IsCompleted},{activity.AlarmSet},{activity.IsPublic}");
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