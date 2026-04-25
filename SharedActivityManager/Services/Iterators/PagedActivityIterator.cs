using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Iterators
{
    /// <summary>
    /// Iterator cu paginare - parcurge activitățile pe pagini
    /// </summary>
    public class PagedActivityIterator : IActivityIterator
    {
        private readonly ActivityCollection _collection;
        private readonly int _pageSize;
        private int _currentPage;
        private int _positionInPage;
        private Activity _current;
        private List<Activity> _currentPageItems;

        public PagedActivityIterator(ActivityCollection collection, int pageSize, int startPage = 0)
        {
            _collection = collection;
            _pageSize = pageSize;
            _currentPage = startPage;
            _positionInPage = -1;
            _currentPageItems = new List<Activity>();
            LoadCurrentPage();
        }

        public int CurrentIndex => (_currentPage * _pageSize) + _positionInPage;

        public Activity Current => _current;

        public int Count => _collection.Count;

        public int TotalPages => (int)Math.Ceiling((double)_collection.Count / _pageSize);

        public int CurrentPage => _currentPage;

        public bool HasNextPage => _currentPage + 1 < TotalPages;

        public bool HasPreviousPage => _currentPage > 0;

        private void LoadCurrentPage()
        {
            _currentPageItems.Clear();
            var start = _currentPage * _pageSize;
            var end = Math.Min(start + _pageSize, _collection.Count);

            System.Diagnostics.Debug.WriteLine($"[PagedIterator] Loading page {_currentPage}: items {start} to {end - 1} (total pages: {TotalPages})");

            for (int i = start; i < end; i++)
            {
                _currentPageItems.Add(_collection.GetAt(i));
            }
        }

        public bool MoveNext()
        {
            if (_positionInPage + 1 < _currentPageItems.Count)
            {
                _positionInPage++;
                _current = _currentPageItems[_positionInPage];
                return true;
            }

            return false;
        }

        public bool MovePrevious()
        {
            if (_positionInPage - 1 >= 0)
            {
                _positionInPage--;
                _current = _currentPageItems[_positionInPage];
                return true;
            }

            return false;
        }

        public bool HasPrevious()
        {
            return _positionInPage > 0;
        }

        public void Reset()
        {
            _positionInPage = -1;
            _current = null;
        }

        public bool MoveToFirst()
        {
            if (_currentPageItems.Count == 0)
                return false;

            _positionInPage = 0;
            _current = _currentPageItems[0];
            return true;
        }

        public bool MoveToLast()
        {
            if (_currentPageItems.Count == 0)
                return false;

            _positionInPage = _currentPageItems.Count - 1;
            _current = _currentPageItems[_positionInPage];
            return true;
        }

        public void GoToPage(int pageNumber)
        {
            System.Diagnostics.Debug.WriteLine($"[PagedIterator] GoToPage: from page {_currentPage} to page {pageNumber}, total pages: {TotalPages}");

            if (pageNumber >= 0 && pageNumber < TotalPages)
            {
                _currentPage = pageNumber;
                _positionInPage = -1;
                _current = null;
                LoadCurrentPage();

                System.Diagnostics.Debug.WriteLine($"[PagedIterator] Successfully moved to page {_currentPage}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[PagedIterator] GoToPage failed: page {pageNumber} out of range (0-{TotalPages - 1})");
            }
        }
    }
}