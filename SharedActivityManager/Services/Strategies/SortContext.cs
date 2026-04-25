using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Strategies
{
    /// <summary>
    /// Contextul - gestionează strategia curentă și aplică sortarea
    /// </summary>
    public class SortContext
    {
        private ISortStrategy _strategy;

        public SortContext()
        {
            // Strategia implicită - sortare după dată
            _strategy = new SortByDateStrategy();
        }

        public SortContext(ISortStrategy strategy)
        {
            _strategy = strategy;
        }

        /// <summary>
        /// Schimbă strategia de sortare la runtime
        /// </summary>
        public void SetStrategy(ISortStrategy strategy)
        {
            _strategy = strategy;
            System.Diagnostics.Debug.WriteLine($"[SortContext] Strategy changed to: {_strategy.Name}");
        }

        /// <summary>
        /// Obține strategia curentă
        /// </summary>
        public ISortStrategy GetCurrentStrategy()
        {
            return _strategy;
        }

        /// <summary>
        /// Schimbă ordinea de sortare (crescător/descrescător)
        /// </summary>
        public void SetSortOrder(SortOrder order)
        {
            _strategy.Order = order;
            System.Diagnostics.Debug.WriteLine($"[SortContext] Sort order changed to: {order}");
        }

        /// <summary>
        /// Aplică sortarea pe lista de activități
        /// </summary>
        public List<Activity> Sort(List<Activity> activities)
        {
            if (activities == null || !activities.Any())
                return activities ?? new List<Activity>();

            var sorted = _strategy.Sort(activities);
            System.Diagnostics.Debug.WriteLine($"[SortContext] Sorted {sorted.Count} activities using {_strategy.Name} (Order: {_strategy.Order})");
            return sorted;
        }

        /// <summary>
        /// Obține toate strategiile disponibile
        /// </summary>
        public static List<ISortStrategy> GetAllStrategies()
        {
            return new List<ISortStrategy>
            {
                new SortByDateStrategy(),
                new SortByTitleStrategy(),
                new SortByTypeStrategy(),
                new SortByPriorityStrategy(),
                new SortByProgressStrategy(),
                new SortByOwnerStrategy()
            };
        }
    }
}