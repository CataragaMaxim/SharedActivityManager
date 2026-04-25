using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Iterators
{
    /// <summary>
    /// Interfața Colecție - definește metodele pentru crearea iteratorilor
    /// </summary>
    public interface IActivityCollection
    {
        /// <summary>
        /// Creează un iterator implicit (în ordinea adăugării)
        /// </summary>
        IActivityIterator CreateIterator();

        /// <summary>
        /// Creează un iterator cu filtru
        /// </summary>
        IActivityIterator CreateFilteredIterator(Func<Activity, bool> filter);

        /// <summary>
        /// Creează un iterator cu paginare
        /// </summary>
        IActivityIterator CreatePagedIterator(int pageSize, int pageNumber = 0);

        /// <summary>
        /// Creează un iterator în ordine inversă
        /// </summary>
        IActivityIterator CreateReverseIterator();

        /// <summary>
        /// Creează un iterator sortat
        /// </summary>
        IActivityIterator CreateSortedIterator(IComparer<Activity> comparer);

        /// <summary>
        /// Adaugă o activitate în colecție
        /// </summary>
        void Add(Activity activity);

        /// <summary>
        /// Elimină o activitate din colecție
        /// </summary>
        bool Remove(Activity activity);

        /// <summary>
        /// Numărul total de activități
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Obține activitatea la un index specific
        /// </summary>
        Activity GetAt(int index);
    }
}