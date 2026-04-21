using SharedActivityManager.Data;
using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Flyweight
{
    public class ActivityStatisticsService
    {
        private readonly ActivityDataBase _database;
        private readonly ActivityTypeMetadataFactory _metadataFactory;

        public ActivityStatisticsService(ActivityDataBase database)
        {
            _database = database;
            _metadataFactory = ActivityTypeMetadataFactory.Instance;
        }

        public async Task<Dictionary<string, int>> GetActivitiesCountByTypeAsync()
        {
            var activities = await _database.GetActivitiesAsync();
            var result = new Dictionary<string, int>();

            foreach (var metadata in _metadataFactory.GetAllMetadata())
            {
                var count = activities.Count(a => a.TypeId == metadata.Type);
                result[metadata.GetFormattedName()] = count;
            }

            return result;
        }

        public async Task<string> GetStatisticsReportAsync()
        {
            var activities = await _database.GetActivitiesAsync();
            var report = "📊 ACTIVITY STATISTICS\n";
            report += "═══════════════════════\n\n";

            foreach (var metadata in _metadataFactory.GetAllMetadata())
            {
                var count = activities.Count(a => a.TypeId == metadata.Type);
                var completed = activities.Count(a => a.TypeId == metadata.Type && a.IsCompleted);
                var percent = count > 0 ? (double)completed / count * 100 : 0;

                report += $"{metadata.GetFormattedName()}\n";
                report += $"   ├─ Total: {count}\n";
                report += $"   ├─ Completed: {completed}\n";
                report += $"   └─ Progress: {percent:F0}%\n\n";
            }

            return report;
        }
    }
}