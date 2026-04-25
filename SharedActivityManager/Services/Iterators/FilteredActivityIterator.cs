using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Iterators
{
    /// <summary>
    /// Iterator cu filtru - parcurge doar activitățile care îndeplinesc condiția
    /// </summary>
    public class FilteredActivityIterator : IActivityIterator
    {
        private readonly ActivityCollection _collection;
        private readonly Func<Activity, bool> _filter;
        private readonly List<int> _filteredIndices;
        private int _position;
        private Activity _current;

        public FilteredActivityIterator(ActivityCollection collection, Func<Activity, bool> filter)
        {
            _collection = collection;
            _filter = filter;
            _filteredIndices = new List<int>();
            _position = -1;

            // Pre-calculează indicii care îndeplinesc filtrul
            for (int i = 0; i < _collection.Count; i++)
            {
                var activity = _collection.GetAt(i);
                if (activity != null && _filter(activity))
                {
                    _filteredIndices.Add(i);
                }
            }
        }

        public int CurrentIndex => _position >= 0 && _position < _filteredIndices.Count
            ? _filteredIndices[_position]
            : -1;

        public Activity Current => _current;

        public int Count => _filteredIndices.Count;

        public bool MoveNext()
        {
            if (_position + 1 >= _filteredIndices.Count)
                return false;

            _position++;
            _current = _collection.GetAt(_filteredIndices[_position]);
            return true;
        }

        public bool MovePrevious()
        {
            if (_position - 1 < 0)
                return false;

            _position--;
            _current = _collection.GetAt(_filteredIndices[_position]);
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
            if (_filteredIndices.Count == 0)
                return false;

            _position = 0;
            _current = _collection.GetAt(_filteredIndices[0]);
            return true;
        }

        public bool MoveToLast()
        {
            if (_filteredIndices.Count == 0)
                return false;

            _position = _filteredIndices.Count - 1;
            _current = _collection.GetAt(_filteredIndices[_position]);
            return true;
        }
    }
}