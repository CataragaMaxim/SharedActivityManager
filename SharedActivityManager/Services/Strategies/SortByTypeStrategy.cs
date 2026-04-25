using SharedActivityManager.Models;
using SharedActivityManager.Enums;

namespace SharedActivityManager.Services.Strategies
{
    /// <summary>
    /// Sortează activitățile după tip (Work, Personal, Health, Study, Other)
    /// </summary>
    public class SortByTypeStrategy : ISortStrategy
    {
        public string Name => "Sort by Type";
        public string Description => "Sort activities by their type (Work, Personal, Health, Study, Other)";
        public SortOrder Order { get; set; } = SortOrder.Ascending;

        // Ordinea implicită a tipurilor
        private readonly Dictionary<ActivityType, int> _typeOrder = new()
        {
            { ActivityType.Work, 1 },
            { ActivityType.Personal, 2 },
            { ActivityType.Health, 3 },
            { ActivityType.Study, 4 },
            { ActivityType.Other, 5 }
        };

        public List<Activity> Sort(List<Activity> activities)
        {
            if (Order == SortOrder.Ascending)
            {
                return activities.OrderBy(a => _typeOrder[a.TypeId]).ToList();
            }
            else
            {
                return activities.OrderByDescending(a => _typeOrder[a.TypeId]).ToList();
            }
        }
    }
}