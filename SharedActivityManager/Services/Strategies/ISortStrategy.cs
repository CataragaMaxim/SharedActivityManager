using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Strategies
{
    /// <summary>
    /// Interfața Strategy - definește contractul pentru toate strategiile de sortare
    /// </summary>
    public interface ISortStrategy
    {
        /// <summary>
        /// Numele strategiei (pentru afișare în UI)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Descrierea strategiei
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Sortează lista de activități conform strategiei
        /// </summary>
        List<Activity> Sort(List<Activity> activities);

        /// <summary>
        /// Ordinea de sortare (crescător/descrescător)
        /// </summary>
        SortOrder Order { get; set; }
    }

    /// <summary>
    /// Ordinea de sortare
    /// </summary>
    public enum SortOrder
    {
        Ascending,
        Descending
    }
}