using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Memento
{
    /// <summary>
    /// Caretaker - gestionează salvarea și restaurarea memento-urilor
    /// </summary>
    public class ActivityMementoCaretaker
    {
        private readonly List<IActivityMemento> _mementos;
        private int _currentIndex;
        private readonly int _maxHistorySize;

        public event EventHandler<HistoryChangedEventArgs> HistoryChanged;

        public ActivityMementoCaretaker(int maxHistorySize = 50)
        {
            _mementos = new List<IActivityMemento>();
            _currentIndex = -1;
            _maxHistorySize = maxHistorySize;
        }

        /// <summary>
        /// Salvează o nouă stare (snapshot)
        /// </summary>
        public void SaveSnapshot(List<Activity> activities, string description)
        {
            // Elimină snapshot-urile viitoare (pierdem Redo history)
            if (_currentIndex < _mementos.Count - 1)
            {
                var removeCount = _mementos.Count - (_currentIndex + 1);
                _mementos.RemoveRange(_currentIndex + 1, removeCount);
            }

            // Creează noul snapshot
            var memento = new ActivityMemento(activities, description);
            _mementos.Add(memento);
            _currentIndex++;

            // Limitează istoricul
            while (_mementos.Count > _maxHistorySize)
            {
                _mementos.RemoveAt(0);
                _currentIndex--;
            }

            System.Diagnostics.Debug.WriteLine($"[Memento] Saved snapshot: {memento.Name}");
            OnHistoryChanged(new HistoryChangedEventArgs(true, false, _currentIndex, _mementos.Count));
        }

        /// <summary>
        /// Restaurează o stare anterioară (by index)
        /// </summary>
        public List<Activity> RestoreSnapshot(int index)
        {
            if (index < 0 || index >= _mementos.Count)
            {
                System.Diagnostics.Debug.WriteLine($"[Memento] Invalid snapshot index: {index}");
                return null;
            }

            var memento = _mementos[index];
            _currentIndex = index;

            System.Diagnostics.Debug.WriteLine($"[Memento] Restored snapshot: {memento.Name}");
            OnHistoryChanged(new HistoryChangedEventArgs(false, true, _currentIndex, _mementos.Count));

            return memento.GetActivities();
        }

        /// <summary>
        /// Mergi înapoi în istoric (Back in time)
        /// </summary>
        public List<Activity> GoBack()
        {
            if (!CanGoBack) return null;
            return RestoreSnapshot(_currentIndex - 1);
        }

        /// <summary>
        /// Mergi înainte în istoric (Forward in time)
        /// </summary>
        public List<Activity> GoForward()
        {
            if (!CanGoForward) return null;
            return RestoreSnapshot(_currentIndex + 1);
        }

        /// <summary>
        /// Verifică dacă se poate merge înapoi
        /// </summary>
        public bool CanGoBack => _currentIndex > 0;

        /// <summary>
        /// Verifică dacă se poate merge înainte
        /// </summary>
        public bool CanGoForward => _currentIndex < _mementos.Count - 1;

        /// <summary>
        /// Obține toate snapshot-urile pentru afișare
        /// </summary>
        public List<IActivityMemento> GetHistory()
        {
            return _mementos.ToList();
        }

        /// <summary>
        /// Obține snapshot-ul curent
        /// </summary>
        public IActivityMemento GetCurrentSnapshot()
        {
            if (_currentIndex >= 0 && _currentIndex < _mementos.Count)
                return _mementos[_currentIndex];
            return null;
        }

        /// <summary>
        /// Obține indexul curent
        /// </summary>
        public int CurrentIndex => _currentIndex;

        /// <summary>
        /// Obține numărul total de snapshot-uri
        /// </summary>
        public int TotalSnapshots => _mementos.Count;

        /// <summary>
        /// Curăță istoricul
        /// </summary>
        public void Clear()
        {
            _mementos.Clear();
            _currentIndex = -1;
            System.Diagnostics.Debug.WriteLine("[Memento] History cleared");
            OnHistoryChanged(new HistoryChangedEventArgs(false, false, _currentIndex, _mementos.Count));
        }

        private void OnHistoryChanged(HistoryChangedEventArgs e)
        {
            HistoryChanged?.Invoke(this, e);
        }
    }

    public class HistoryChangedEventArgs : EventArgs
    {
        public bool IsSave { get; }
        public bool IsRestore { get; }
        public int CurrentIndex { get; }
        public int TotalSnapshots { get; }

        public HistoryChangedEventArgs(bool isSave, bool isRestore, int currentIndex, int totalSnapshots)
        {
            IsSave = isSave;
            IsRestore = isRestore;
            CurrentIndex = currentIndex;
            TotalSnapshots = totalSnapshots;
        }
    }
}