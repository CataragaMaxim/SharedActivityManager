using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Iterators
{
    /// <summary>
    /// Interfața Iterator - definește metodele pentru parcurgerea colecției
    /// </summary>
    public interface IActivityIterator
    {
        /// <summary>
        /// Indexul curent în colecție
        /// </summary>
        int CurrentIndex { get; }

        /// <summary>
        /// Elementul curent
        /// </summary>
        Activity Current { get; }

        /// <summary>
        /// Resetează iteratorul la început
        /// </summary>
        void Reset();

        /// <summary>
        /// Se mută la următorul element
        /// </summary>
        bool MoveNext();

        /// <summary>
        /// Verifică dacă există un element anterior
        /// </summary>
        bool HasPrevious();

        /// <summary>
        /// Se mută la elementul anterior
        /// </summary>
        bool MovePrevious();

        /// <summary>
        /// Numărul total de elemente din colecție
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Se mută la primul element
        /// </summary>
        bool MoveToFirst();

        /// <summary>
        /// Se mută la ultimul element
        /// </summary>
        bool MoveToLast();
    }
}