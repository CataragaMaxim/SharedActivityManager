using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Iterators
{
    /// <summary>
    /// Iterator implicit - parcurge activitățile în ordinea adăugării
    /// </summary>
    public class DefaultActivityIterator : IActivityIterator
    {
        private readonly ActivityCollection _collection;
        private int _position;
        private Activity _current;

        public DefaultActivityIterator(ActivityCollection collection)
        {
            _collection = collection;
            _position = -1;
            _current = null;
        }

        public int CurrentIndex => _position;

        public Activity Current => _current;

        public int Count => _collection.Count;

        public bool MoveNext()
        {
            if (_position + 1 >= _collection.Count)
                return false;

            _position++;
            _current = _collection.GetAt(_position);
            return true;
        }

        public bool MovePrevious()
        {
            if (_position - 1 < 0)
                return false;

            _position--;
            _current = _collection.GetAt(_position);
            return true;
        }

        public bool HasPrevious()
        {
            return _position > 0;
        }

        public void Reset()
        {
            _position = -1;
            _current = null;
        }

        public bool MoveToFirst()
        {
            if (_collection.Count == 0)
                return false;

            _position = 0;
            _current = _collection.GetAt(0);
            return true;
        }

        public bool MoveToLast()
        {
            if (_collection.Count == 0)
                return false;

            _position = _collection.Count - 1;
            _current = _collection.GetAt(_position);
            return true;
        }
    }
}