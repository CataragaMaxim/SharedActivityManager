using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Strategies
{
    /// <summary>
    /// Sortează activitățile alfabetic după titlu
    /// </summary>
    public class SortByTitleStrategy : ISortStrategy
    {
        public string Name => "Sort by Title";
        public string Description => "Sort activities alphabetically by title";
        public SortOrder Order { get; set; } = SortOrder.Ascending;

        public List<Activity> Sort(List<Activity> activities)
        {
            if (Order == SortOrder.Ascending)
            {
                return activities.OrderBy(a => a.Title).ToList();
            }
            else
            {
                return activities.OrderByDescending(a => a.Title).ToList();
            }
        }
    }
}