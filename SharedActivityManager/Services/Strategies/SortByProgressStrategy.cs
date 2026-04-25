using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Strategies
{
    /// <summary>
    /// Sortează activitățile după progres (completate vs necompletate)
    /// </summary>
    public class SortByProgressStrategy : ISortStrategy
    {
        public string Name => "Sort by Progress";
        public string Description => "Sort activities by completion status (completed first or last)";
        public SortOrder Order { get; set; } = SortOrder.Ascending;

        public List<Activity> Sort(List<Activity> activities)
        {
            if (Order == SortOrder.Ascending)
            {
                // Necompletate primele, apoi completate
                return activities.OrderBy(a => a.IsCompleted).ThenBy(a => a.StartDate).ToList();
            }
            else
            {
                // Completate primele, apoi necompletate
                return activities.OrderByDescending(a => a.IsCompleted).ThenBy(a => a.StartDate).ToList();
            }
        }
    }
}