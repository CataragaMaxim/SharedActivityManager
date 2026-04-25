using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Iterators
{
    /// <summary>
    /// Colecție concretă de activități
    /// </summary>
    public class ActivityCollection : IActivityCollection
    {
        private readonly List<Activity> _activities;
        private readonly object _lock = new object();

        public ActivityCollection()
        {
            _activities = new List<Activity>();
        }

        public ActivityCollection(List<Activity> activities)
        {
            _activities = activities ?? new List<Activity>();
        }

        public int Count => _activities.Count;

        public void Add(Activity activity)
        {
            lock (_lock)
            {
                _activities.Add(activity);
            }
        }

        public bool Remove(Activity activity)
        {
            lock (_lock)
            {
                return _activities.Remove(activity);
            }
        }

        public Activity GetAt(int index)
        {
            lock (_lock)
            {
                if (index < 0 || index >= _activities.Count)
                    return null;
                return _activities[index];
            }
        }

        public IActivityIterator CreateIterator()
        {
            return new DefaultActivityIterator(this);
        }

        public IActivityIterator CreateFilteredIterator(Func<Activity, bool> filter)
        {
            return new FilteredActivityIterator(this, filter);
        }

        public IActivityIterator CreatePagedIterator(int pageSize, int pageNumber = 0)
        {
            return new PagedActivityIterator(this, pageSize, pageNumber);
        }

        public IActivityIterator CreateReverseIterator()
        {
            return new ReverseActivityIterator(this);
        }

        public IActivityIterator CreateSortedIterator(IComparer<Activity> comparer)
        {
            var sortedList = _activities.OrderBy(a => a, comparer).ToList();
            return new DefaultActivityIterator(new ActivityCollection(sortedList));
        }

        // Metodă pentru a obține lista internă (folosită de iteratori)
        internal List<Activity> GetInternalList()
        {
            return _activities;
        }
    }
}