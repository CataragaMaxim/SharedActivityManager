using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Strategies
{
    /// <summary>
    /// Sortează activitățile după data începerii
    /// </summary>
    public class SortByDateStrategy : ISortStrategy
    {
        public string Name => "Sort by Date";
        public string Description => "Sort activities by start date";
        public SortOrder Order { get; set; } = SortOrder.Ascending;

        public List<Activity> Sort(List<Activity> activities)
        {
            if (Order == SortOrder.Ascending)
            {
                return activities.OrderBy(a => a.StartDate).ThenBy(a => a.StartTime).ToList();
            }
            else
            {
                return activities.OrderByDescending(a => a.StartDate).ThenByDescending(a => a.StartTime).ToList();
            }
        }
    }
}