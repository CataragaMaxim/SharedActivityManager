using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Strategies
{
    /// <summary>
    /// Sortează activitățile după proprietar (util pentru activități partajate)
    /// </summary>
    public class SortByOwnerStrategy : ISortStrategy
    {
        public string Name => "Sort by Owner";
        public string Description => "Sort activities by owner name";
        public SortOrder Order { get; set; } = SortOrder.Ascending;

        public List<Activity> Sort(List<Activity> activities)
        {
            if (Order == SortOrder.Ascending)
            {
                return activities.OrderBy(a => a.OwnerId).ThenBy(a => a.StartDate).ToList();
            }
            else
            {
                return activities.OrderByDescending(a => a.OwnerId).ThenBy(a => a.StartDate).ToList();
            }
        }
    }
}