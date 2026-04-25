using SharedActivityManager.Models;
using SharedActivityManager.Enums;
using SharedActivityManager.Factories;

namespace SharedActivityManager.Services.Strategies
{
    /// <summary>
    /// Sortează activitățile după prioritate (doar pentru activități Work)
    /// Activitățile fără prioritate sunt plasate la sfârșit
    /// </summary>
    public class SortByPriorityStrategy : ISortStrategy
    {
        public string Name => "Sort by Priority";
        public string Description => "Sort work activities by priority (High > Medium > Low)";
        public SortOrder Order { get; set; } = SortOrder.Ascending;

        private readonly Dictionary<string, int> _priorityOrder = new()
        {
            { "Critical", 1 },
            { "High", 2 },
            { "Medium", 3 },
            { "Low", 4 },
            { "", 5 }  // Activități fără prioritate
        };

        public List<Activity> Sort(List<Activity> activities)
        {
            var workCreator = new WorkActivityCreator();

            if (Order == SortOrder.Ascending)
            {
                return activities.OrderBy(a =>
                {
                    if (a.TypeId == ActivityType.Work)
                    {
                        var data = workCreator.GetWorkData(a);
                        return _priorityOrder.GetValueOrDefault(data.Priority, 5);
                    }
                    return 5; // Activitățile non-Work la sfârșit
                }).ThenBy(a => a.StartDate).ToList();
            }
            else
            {
                return activities.OrderByDescending(a =>
                {
                    if (a.TypeId == ActivityType.Work)
                    {
                        var data = workCreator.GetWorkData(a);
                        return _priorityOrder.GetValueOrDefault(data.Priority, 5);
                    }
                    return 0;
                }).ThenBy(a => a.StartDate).ToList();
            }
        }
    }
}