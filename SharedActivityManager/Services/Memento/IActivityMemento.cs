using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Memento
{
    /// <summary>
    /// Interfața Memento - definește metodele pentru accesarea stării salvate
    /// </summary>
    public interface IActivityMemento
    {
        /// <summary>
        /// Numele/descrierea snapshot-ului
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Data și ora când a fost creat snapshot-ul
        /// </summary>
        DateTime Timestamp { get; }

        /// <summary>
        /// Lista activităților salvate
        /// </summary>
        List<Activity> GetActivities();

        /// <summary>
        /// Numărul de activități în snapshot
        /// </summary>
        int ActivityCount { get; }

        /// <summary>
        /// Descrierea snapshot-ului (ce s-a întâmplat)
        /// </summary>
        string Description { get; }
    }
}