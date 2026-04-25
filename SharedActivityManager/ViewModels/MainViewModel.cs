// ViewModels/MainViewModel.cs
using System.Collections.ObjectModel;
using System.Net.Mail;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Builders;
using SharedActivityManager.Enums;
using SharedActivityManager.Models;
using SharedActivityManager.Repositories;
using SharedActivityManager.Services;
using SharedActivityManager.Services.Commands;
using SharedActivityManager.Services.Flyweight;
using SharedActivityManager.Services.Memento;
using SharedActivityManager.Services.Observers;
using SharedActivityManager.Services.Proxies;
using SharedActivityManager.Services.Strategies;

namespace SharedActivityManager.ViewModels
{
    public partial class MainViewModel : ObservableObject, IActivityObserver    
    {
        // ========== SERVICII ==========
        private readonly IActivityManagementFacade _activityFacade;
        private readonly IActivityService _activityService;
        private readonly IAudioService _audioService;
        private readonly IAlarmService _alarmService;
        private readonly IAlertService _alertService;
        //private readonly IMessagingService _messagingService;

        // ========== PROPRIETĂȚI ==========
        [ObservableProperty]
        private ObservableCollection<Activity> activities = new();

        [ObservableProperty]
        private string newTaskTitle;

        [ObservableProperty]
        private string newTaskDesc;

        [ObservableProperty]
        private ActivityType selectedActivityType = ActivityType.Other;

        [ObservableProperty]
        private DateTime selectedStartDate = DateTime.Today;

        [ObservableProperty]
        private TimeSpan selectedStartTime = DateTime.Now.TimeOfDay;

        [ObservableProperty]
        private bool alarmSet;

        [ObservableProperty]
        private ReminderType selectedReminderType = ReminderType.None;

        [ObservableProperty]
        private string selectedRingTone = "Default Alarm";

        [ObservableProperty]
        private RingtoneProj selectedRingtoneObject;

        [ObservableProperty]
        private DateTime nextReminderDatePreview;

        [ObservableProperty]
        private bool isEditMode;

        [ObservableProperty]
        private string pageTitle = "Create New Activity";

        [ObservableProperty]
        private bool isPublic;

        [ObservableProperty]
        private Activity selectedActivity;

        [ObservableProperty]
        private bool _enableNotifications = false;

        [ObservableProperty]
        private bool _enableEmailReminder = false;

        [ObservableProperty]
        private string _emailAddress = string.Empty;

        [ObservableProperty]
        private bool _enableCalendarSync = false;

        [ObservableProperty]
        private bool _enableGpsTracking = false;

        [ObservableProperty]
        private List<string> _attachments = new();

        [ObservableProperty]
        private string _extraFeaturesDescription = string.Empty;

        [ObservableProperty]
        private int _totalExtraCost = 0;

        private readonly SortContext _sortContext;

        [ObservableProperty]
        private List<ISortStrategy> _availableSortStrategies;

        [ObservableProperty]
        private ISortStrategy _selectedSortStrategy;

        [ObservableProperty]
        private SortOrder _selectedSortOrder = SortOrder.Ascending;

        [ObservableProperty]
        private string _currentSortInfo = "Sorted by Date (Ascending)";

        private readonly CommandInvoker _commandInvoker;

        [ObservableProperty]
        private bool _canUndo;

        [ObservableProperty]
        private bool _canRedo;

        [ObservableProperty]
        private string _undoCommandName;

        [ObservableProperty]
        private string _redoCommandName;

        private readonly ActivityMementoCaretaker _mementoCaretaker;

        [ObservableProperty]
        private bool _canGoBackInTime;

        [ObservableProperty]
        private bool _canGoForwardInTime;

        [ObservableProperty]
        private string _currentSnapshotInfo;

        [ObservableProperty]
        private List<IActivityMemento> _historySnapshots;

        [ObservableProperty]
        private bool _showHistoryBrowser;


        // ========== PROPRIETĂȚI CALCULATE ==========
        public DateTime CombinedStartDateTime => SelectedStartDate.Add(SelectedStartTime);

        // ========== PROPRIETĂȚI PENTRU LISTE ==========
        public ObservableCollection<ActivityType> ActivityTypeList { get; } =
            new ObservableCollection<ActivityType>(Enum.GetValues<ActivityType>().Cast<ActivityType>());

        public ObservableCollection<ReminderType> ReminderTypeList { get; } =
            new ObservableCollection<ReminderType>(Enum.GetValues<ReminderType>().Cast<ReminderType>());

        public ObservableCollection<string> RingToneList { get; } = new ObservableCollection<string>
        {
            "Default Alarm",
            "Digital Beep",
            "Gentle Wake",
            "Morning Bliss",
            "Classic Ring"
        };

        // ========== CONSTRUCTOR ==========
        public MainViewModel()
        {
            // Obține serviciile de bază
            var repository = new ActivityRepository();
            var alarmService = PlatformBridge.Instance.AlarmService;
            var audioService = PlatformBridge.Instance.AudioService;
            var notificationService = PlatformBridge.Instance.NotificationService;
            var alertService = new AlertService();
            var messagingService = new MessagingService();

            // 🔥 CREEAZĂ LANȚUL DE PROXY-URI
            // Cache -> Security -> Virtual -> Real

            // În MainViewModel constructor, înlocuiește:
            // _activityService = new RealActivityService(repository, alarmService);

            //// Cu:
            //var realService = new RealActivityService(repository, alarmService);
            //_activityService = new VirtualActivityServiceProxy(realService, 20);
            _activityService = ActivityServiceProxyFactory.CreateFullProxyChain(
                repository,
                alarmService,
                "current_user",
                enableVirtualProxy: true,    // 🔥 ACTIVAT (acum funcționează corect)
                enableSecurityProxy: true,   // 🔥 ACTIVAT
                enableCacheProxy: true,      // 🔥 ACTIVAT
                cacheDurationMinutes: 5
            );

            _commandInvoker = new CommandInvoker();
            _commandInvoker.CommandExecuted += OnCommandExecuted;

            _mementoCaretaker = new ActivityMementoCaretaker();
            _mementoCaretaker.HistoryChanged += OnHistoryChanged;
            _historySnapshots = new List<IActivityMemento>();

            TakeSnapshot("Application started");

            if (_activityService is IActivitySubject subject)
            {
                subject.Attach(this);
                System.Diagnostics.Debug.WriteLine("[MainViewModel] Attached as observer");
            }

            _sortContext = new SortContext();
            AvailableSortStrategies = SortContext.GetAllStrategies();
            SelectedSortStrategy = AvailableSortStrategies.FirstOrDefault(s => s is SortByDateStrategy);
            SelectedSortOrder = SortOrder.Ascending;

            _audioService = audioService;
            _alarmService = alarmService;
            _alertService = alertService;
            //_messagingService = messagingService;

            // Inițializare Facade cu serviciile corecte
            _activityFacade = new ActivityManagementFacade(
                _activityService, _alarmService, _audioService,
                notificationService, alertService, messagingService);

            // Abonare la mesaje
            //_messagingService.Subscribe<ActivitiesChangedMessage>(this, OnActivitiesChanged);

            // Încărcare date
            Task.Run(async () => await LoadActivitiesAsync()).Wait();
            LoadSavedRingtone();
            UpdateNextReminderPreview();
            Task.Run(async () => await RestoreAlarmsAsync());
        }

        private void OnHistoryChanged(object sender, HistoryChangedEventArgs e)
        {
            CanGoBackInTime = _mementoCaretaker.CanGoBack;
            CanGoForwardInTime = _mementoCaretaker.CanGoForward;

            var current = _mementoCaretaker.GetCurrentSnapshot();
            CurrentSnapshotInfo = current != null ? $"Current: {current.Name}" : "No history";

            // Actualizează lista pentru UI
            HistorySnapshots = _mementoCaretaker.GetHistory();

            System.Diagnostics.Debug.WriteLine($"[Memento] History changed - CanGoBack: {CanGoBackInTime}, CanGoForward: {CanGoForwardInTime}");
        }

        /// <summary>
        /// Salvează un snapshot al stării curente
        /// </summary>
        public void TakeSnapshot(string description)
        {
            _mementoCaretaker.SaveSnapshot(Activities.ToList(), description);
        }

        /// <summary>
        /// Comandă pentru a merge înapoi în istoric
        /// </summary>
        [RelayCommand]
        private async Task GoBackInTime()
        {
            System.Diagnostics.Debug.WriteLine("[Memento] Going back in time");

            var restoredActivities = _mementoCaretaker.GoBack();
            if (restoredActivities != null)
            {
                await RestoreActivities(restoredActivities);
            }
        }

        /// <summary>
        /// Comandă pentru a merge înainte în istoric
        /// </summary>
        [RelayCommand]
        private async Task GoForwardInTime()
        {
            System.Diagnostics.Debug.WriteLine("[Memento] Going forward in time");

            var restoredActivities = _mementoCaretaker.GoForward();
            if (restoredActivities != null)
            {
                await RestoreActivities(restoredActivities);
            }
        }

        /// <summary>
        /// Comandă pentru a restaura un snapshot specific
        /// </summary>
        [RelayCommand]
        private async Task RestoreSnapshot(IActivityMemento snapshot)
        {
            if (snapshot == null) return;

            System.Diagnostics.Debug.WriteLine($"[Memento] Restoring snapshot: {snapshot.Name}");

            // 🔥 FOLOSEȘTE GetHistory() ÎN LOC DE HistorySnapshots
            var history = _mementoCaretaker.GetHistory();
            var snapshotIndex = history.IndexOf(snapshot);

            var restoredActivities = _mementoCaretaker.RestoreSnapshot(snapshotIndex);
            if (restoredActivities != null)
            {
                await RestoreActivities(restoredActivities);
            }

            ShowHistoryBrowser = false;
        }

        /// <summary>
        /// Comandă pentru a arăta/ascunde History Browser
        /// </summary>
        [RelayCommand]
        private void ToggleHistoryBrowser()
        {
            ShowHistoryBrowser = !ShowHistoryBrowser;
            if (ShowHistoryBrowser)
            {
                HistorySnapshots = _mementoCaretaker.GetHistory();
            }
        }

        /// <summary>
        /// Comandă pentru a curăța istoricul
        /// </summary>
        [RelayCommand]
        private void ClearHistory()
        {
            _mementoCaretaker.Clear();
            TakeSnapshot("History cleared");
        }

        /// <summary>
        /// Restaurează lista de activități și salvează în baza de date
        /// </summary>
        private async Task RestoreActivities(List<Activity> restoredActivities)
        {
            // Curăță toate activitățile existente
            var currentActivities = await _activityService.GetActivitiesAsync();
            foreach (var activity in currentActivities)
            {
                await _activityService.DeleteActivityAsync(activity);
            }

            // Adaugă activitățile restaurate
            foreach (var activity in restoredActivities)
            {
                var newActivity = activity.DeepCopy();
                newActivity.Id = 0; // Reset ID pentru a fi inserate ca noi
                await _activityService.SaveActivityAsync(newActivity);
            }

            // Reîncarcă UI-ul
            await LoadActivitiesAsync();

            await _alertService.ShowAlertAsync("History", "State restored successfully!");
        }

        private void OnCommandExecuted(object sender, CommandExecutedEventArgs e)
        {
            // Actualizează UI-ul pentru Undo/Redo
            CanUndo = _commandInvoker.CanUndo;
            CanRedo = _commandInvoker.CanRedo;
            UndoCommandName = _commandInvoker.GetUndoCommandName();
            RedoCommandName = _commandInvoker.GetRedoCommandName();

            System.Diagnostics.Debug.WriteLine($"[ViewModel] UI Updated - CanUndo: {CanUndo}, CanRedo: {CanRedo}");
        }

        // 🔥 METODE PENTRU UNDO/REDO
        [RelayCommand]
        private async Task Undo()
        {
            System.Diagnostics.Debug.WriteLine("[ViewModel] Undo requested");
            await _commandInvoker.Undo();
            await LoadActivitiesAsync();
        }

        [RelayCommand]
        private async Task Redo()
        {
            System.Diagnostics.Debug.WriteLine("[ViewModel] Redo requested");
            await _commandInvoker.Redo();
            await LoadActivitiesAsync();
        }

        public async Task OnActivityChanged(string action, Activity activity = null, int activityCount = 0)
        {
            System.Diagnostics.Debug.WriteLine($"[MainViewModel] Observer received: {action}");

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                switch (action)
                {
                    case "Added":
                    case "Updated":
                    case "Deleted":
                    case "Copied":
                        await LoadActivitiesAsync();
                        break;

                    case "DeletedAll":
                    case "Imported":
                        await LoadActivitiesAsync();
                        if (activityCount > 0)
                        {
                            await _alertService.ShowAlertAsync("Success",
                                $"{activityCount} activities have been processed");
                        }
                        break;

                    case "CategoryChanged":
                        // Reîncarcă doar dacă e necesar
                        await LoadActivitiesAsync();
                        break;
                }
            });
        }

        [RelayCommand]
        private void ChangeSortStrategy(ISortStrategy strategy)
        {
            if (strategy == null) return;

            SelectedSortStrategy = strategy;
            _sortContext.SetStrategy(strategy);
            _sortContext.SetSortOrder(SelectedSortOrder);
            CurrentSortInfo = $"{strategy.Name} ({SelectedSortOrder})";

            // Reaplică sortarea pe activitățile curente
            ApplySorting();

            System.Diagnostics.Debug.WriteLine($"[MainViewModel] Sort strategy changed to: {strategy.Name}");
        }

        // 🔥 COMANDĂ PENTRU SCHIMBAREA ORDINII
        [RelayCommand]
        private void ToggleSortOrder()
        {
            SelectedSortOrder = SelectedSortOrder == SortOrder.Ascending
                ? SortOrder.Descending
                : SortOrder.Ascending;

            _sortContext.SetSortOrder(SelectedSortOrder);
            CurrentSortInfo = $"{SelectedSortStrategy?.Name} ({SelectedSortOrder})";

            // Reaplică sortarea
            ApplySorting();
        }

        // 🔥 METODĂ PENTRU A APLICA SORTAREA PE ACTIVITĂȚILE CURENTE
        // Adaugă verificări pentru a evita cicluri infinite
        private void ApplySorting()
        {
            if (Activities == null || !Activities.Any()) return;

            var currentList = Activities.ToList();
            var sortedList = _sortContext.Sort(currentList);

            // 🔥 FOLOSEȘTE Move ÎN LOC DE REASIGNARE PENTRU A MENȚINE REFERINȚA
            var index = 0;
            foreach (var item in sortedList)
            {
                if (Activities[index] != item)
                {
                    Activities.Move(Activities.IndexOf(item), index);
                }
                index++;
            }
        }

        public async Task LoadMoreActivitiesAsync()
        {
            if (_activityService is CachedActivityServiceProxy cached)
            {
                // Metodă pentru a încărca mai multe activități
            }
        }

        // Adaugă aceste metode pentru a salva preferințele utilizatorului

        private void SaveSortPreferences()
        {
            if (SelectedSortStrategy != null)
            {
                Preferences.Set("SortStrategy", SelectedSortStrategy.GetType().Name);
                Preferences.Set("SortOrder", (int)SelectedSortOrder);
            }
        }

        private void LoadSortPreferences()
        {
            var savedStrategy = Preferences.Get("SortStrategy", "SortByDateStrategy");
            var savedOrder = (SortOrder)Preferences.Get("SortOrder", (int)SortOrder.Ascending);

            SelectedSortStrategy = AvailableSortStrategies.FirstOrDefault(s => s.GetType().Name == savedStrategy)
                                   ?? new SortByDateStrategy();
            SelectedSortOrder = savedOrder;

            _sortContext.SetStrategy(SelectedSortStrategy);
            _sortContext.SetSortOrder(SelectedSortOrder);
            CurrentSortInfo = $"{SelectedSortStrategy.Name} ({SelectedSortOrder})";
        }

        // Apelează LoadSortPreferences() în constructor și SaveSortPreferences() la schimbare

        private async Task CreateActivityWithExtras()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle))
            {
                await _alertService.ShowAlertAsync("Validation", "Title is required");
                return;
            }

            try
            {
                int categoryId = await GetCategoryIdForActivityType(SelectedActivityType);

                var newActivity = new Activity
                {
                    Title = NewTaskTitle,
                    Desc = NewTaskDesc ?? string.Empty,
                    TypeId = SelectedActivityType,
                    StartDate = SelectedStartDate,
                    StartTime = CombinedStartDateTime,
                    AlarmSet = AlarmSet,
                    IsCompleted = false,
                    ReminderType = SelectedReminderType,
                    NextReminderDate = CalculateNextReminderDate(),
                    RingTone = SelectedRingTone ?? "Default",
                    IsPublic = IsPublic,
                    OwnerId = "current_user",
                    SharedDate = IsPublic ? DateTime.Now : default,
                    CategoryId = categoryId
                };

                // 🔥 CONSTRUIEȘTE ACTIVITATEA CU FUNCȚIONALITĂȚI EXTRA
                var builder = new ActivityExtraBuilder(newActivity);

                if (EnableNotifications)
                    builder.WithNotifications();

                if (EnableEmailReminder && !string.IsNullOrEmpty(EmailAddress))
                    builder.WithEmailReminder(EmailAddress);

                if (EnableCalendarSync)
                    builder.WithCalendarSync();

                if (EnableGpsTracking)
                    builder.WithGpsTracking();

                foreach (var attachment in Attachments)
                    builder.WithAttachment(attachment);

                // Salvează descrierea pentru afișare
                ExtraFeaturesDescription = builder.GetFullDescription();
                TotalExtraCost = builder.GetTotalExtraCost();

                // Execută activitatea (în cazul în care e marcată ca completată)
                // await builder.ExecuteAsync();

                await _activityFacade.CreateCompleteActivityAsync(
                    NewTaskTitle,
                    NewTaskDesc ?? string.Empty,
                    SelectedActivityType,
                    CombinedStartDateTime,
                    AlarmSet,
                    SelectedReminderType,
                    SelectedRingTone ?? "Default",
                    IsPublic,
                    null);

                await LoadActivitiesAsync();
                ResetForm();

                await _alertService.ShowAlertAsync("Success",
                    $"Activity created with extras!\n{ExtraFeaturesDescription}\nTotal extra time: {TotalExtraCost} min");
            }
            catch (Exception ex)
            {
                await _alertService.ShowAlertAsync("Error", $"Failed to add activity: {ex.Message}");
            }
        }


        private async void OnActivitiesChanged(ActivitiesChangedMessage message)
        {
            System.Diagnostics.Debug.WriteLine($"MainViewModel: ActivitiesChanged received - Action: {message.Action}");

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await LoadActivitiesAsync();
            });
        }

        // ========== PARTIAL METHODS ==========
        partial void OnSelectedStartDateChanged(DateTime value) => UpdateNextReminderPreview();
        partial void OnSelectedStartTimeChanged(TimeSpan value) => UpdateNextReminderPreview();
        partial void OnAlarmSetChanged(bool value) => UpdateNextReminderPreview();
        partial void OnSelectedReminderTypeChanged(ReminderType value) => UpdateNextReminderPreview();

        // ========== METODE PRIVATE ==========
        private async Task RestoreAlarmsAsync()
        {
            try
            {
                await _alarmService.RestoreAlarmsAsync(Activities.ToList());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring alarms: {ex.Message}");
            }
        }

        private async void LoadSavedRingtone()
        {
            try
            {
                var savedRingtoneId = Preferences.Get("SelectedRingtone", "default1");
                var ringtones = await _audioService.GetAvailableRingtonesAsync();
                SelectedRingtoneObject = ringtones.FirstOrDefault(r => r.Id == savedRingtoneId);
                SelectedRingTone = SelectedRingtoneObject?.DisplayName ?? "Default Alarm";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading saved ringtone: {ex.Message}");
                SelectedRingTone = "Default Alarm";
            }
        }

        private void UpdateNextReminderPreview()
        {
            NextReminderDatePreview = CalculateNextReminderDate();
        }

        private DateTime CalculateNextReminderDate()
        {
            var startDateTime = CombinedStartDateTime;

            if (!AlarmSet || SelectedReminderType == ReminderType.None)
                return startDateTime;

            return SelectedReminderType switch
            {
                ReminderType.Daily => startDateTime.AddDays(1),
                ReminderType.Weekly => startDateTime.AddDays(7),
                ReminderType.EveryTwoWeeks => startDateTime.AddDays(14),
                ReminderType.Monthly => startDateTime.AddMonths(1),
                ReminderType.Yearly => startDateTime.AddYears(1),
                _ => startDateTime
            };
        }

        // ========== COMENZI ==========
        [RelayCommand]
        private async Task NavigateToShared()
        {
            await Shell.Current.GoToAsync("//sharedactivities");
        }

        // În MainViewModel.LoadActivitiesAsync()
        [RelayCommand]
        public async Task LoadActivitiesAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MainViewModel: Loading activities...");
                var activitiesFromDb = await _activityService.GetActivitiesAsync();

                // 🔥 APLICĂ SORTAREA CURENTĂ
                var sortedActivities = _sortContext.Sort(activitiesFromDb);
                Activities = new ObservableCollection<Activity>(sortedActivities);

                System.Diagnostics.Debug.WriteLine($"MainViewModel: Loaded {Activities.Count} activities");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading activities: {ex.Message}");
                await _alertService.ShowAlertAsync("Error", $"Failed to load activities: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task AddActivity()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle))
            {
                await _alertService.ShowAlertAsync("Validation", "Title is required");
                return;
            }

            try
            {
                int categoryId = await GetCategoryIdForActivityType(SelectedActivityType);

                var newActivity = new Activity
                {
                    Title = NewTaskTitle,
                    Desc = NewTaskDesc ?? string.Empty,
                    TypeId = SelectedActivityType,
                    StartDate = SelectedStartDate,
                    StartTime = CombinedStartDateTime,
                    AlarmSet = AlarmSet,
                    IsCompleted = false,
                    ReminderType = SelectedReminderType,
                    NextReminderDate = CalculateNextReminderDate(),
                    RingTone = SelectedRingTone ?? "Default",
                    IsPublic = IsPublic,
                    OwnerId = "current_user",
                    SharedDate = IsPublic ? DateTime.Now : default,
                    CategoryId = categoryId
                };

                // 🔥 FOLOSEȘTE COMANDĂ ÎN LOC DE FACADE DIRECT
                var command = new CreateActivityCommand(_activityService, newActivity);
                await _commandInvoker.ExecuteCommand(command);


                await LoadActivitiesAsync();
                ResetForm();
                TakeSnapshot($"Added activity: {NewTaskTitle}");
            }
            catch (Exception ex)
            {
                await _alertService.ShowAlertAsync("Error", $"Failed to add activity: {ex.Message}");
            }
        }

        private async Task<int> GetCategoryIdForActivityType(ActivityType type)
        {
            string categoryName = type switch
            {
                ActivityType.Work => "💼 Work",
                ActivityType.Personal => "🏠 Personal",
                ActivityType.Health => "💪 Health",
                ActivityType.Study => "📚 Study",
                _ => "Other"
            };

            var categories = await _activityService.GetCategoriesAsync();
            var existing = categories.FirstOrDefault(c => c.Name == categoryName);

            if (existing != null)
            {
                System.Diagnostics.Debug.WriteLine($"Found category: {categoryName} with ID={existing.Id}");
                return existing.Id;
            }

            // Creează categorie nouă dacă nu există
            var newCategory = new Category
            {
                Name = categoryName,
                ParentCategoryId = 0,
                DisplayOrder = 1
            };

            await _activityService.SaveCategoryAsync(newCategory);
            System.Diagnostics.Debug.WriteLine($"Created new category: {categoryName} with ID={newCategory.Id}");
            return newCategory.Id;
        }
        public async Task SaveEditedActivity()
        {
            if (SelectedActivity == null) return;

            if (string.IsNullOrWhiteSpace(NewTaskTitle))
            {
                await _alertService.ShowAlertAsync("Validation", "Title is required");
                return;
            }

            try
            {
                var oldActivity = SelectedActivity.DeepCopy();

                SelectedActivity.Title = NewTaskTitle;
                SelectedActivity.Desc = NewTaskDesc ?? string.Empty;
                SelectedActivity.TypeId = SelectedActivityType;
                SelectedActivity.StartDate = SelectedStartDate.Date;
                SelectedActivity.StartTime = CombinedStartDateTime;
                SelectedActivity.AlarmSet = AlarmSet;
                SelectedActivity.ReminderType = SelectedReminderType;
                SelectedActivity.NextReminderDate = CalculateNextReminderDate();
                SelectedActivity.RingTone = SelectedRingTone ?? "Default";
                SelectedActivity.IsPublic = IsPublic;

                if (SelectedActivity.IsPublic && SelectedActivity.SharedDate == default)
                {
                    SelectedActivity.SharedDate = DateTime.Now;
                }

                var command = new UpdateActivityCommand(_activityService, oldActivity, SelectedActivity);
                await _commandInvoker.ExecuteCommand(command);

                await LoadActivitiesAsync();  // 🔥 Acest apel deja aplică sortarea

                IsEditMode = false;
                PageTitle = "Create New Activity";
                SelectedActivity = null;
                ResetForm();

                await _alertService.ShowAlertAsync("Success", "Activity updated successfully!");
                TakeSnapshot($"Updated activity: {SelectedActivity?.Title}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in SaveEditedActivity: {ex.Message}");
                await _alertService.ShowAlertAsync("Error", $"Failed to update activity: {ex.Message}");
            }
        }

        public async Task AddNewActivity() => await AddActivity();

        [RelayCommand]
        public async Task DeleteActivity(Activity activity)
        {
            if (activity == null) return;

            try
            {
                // 🔥 FOLOSEȘTE COMANDĂ
                var command = new DeleteActivityCommand(_activityService, activity);
                await _commandInvoker.ExecuteCommand(command);
                await LoadActivitiesAsync();
                TakeSnapshot($"Deleted activity: {activity.Title}");
            }
            catch (Exception ex)
            {
                await _alertService.ShowAlertAsync("Error", $"Failed to delete activity: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task DeleteCurrentActivity()
        {
            if (SelectedActivity != null)
            {
                await DeleteActivity(SelectedActivity);
            }
        }

        [RelayCommand]
        public async Task SaveActivity()
        {
            if (IsEditMode && SelectedActivity != null)
            {
                await SaveEditedActivity();
            }
            else
            {
                await AddNewActivity();
            }
        }

        [RelayCommand]
        private async Task ToggleComplete(Activity activity)
        {
            if (activity != null)
            {
                try
                {
                    // 🔥 FOLOSEȘTE COMANDĂ
                    var newStatus = !activity.IsCompleted;
                    var command = new CompleteActivityCommand(_activityService, activity, newStatus);
                    await _commandInvoker.ExecuteCommand(command);
                    await LoadActivitiesAsync();
                    TakeSnapshot($"{(activity.IsCompleted ? "Completed" : "Incompleted")}: {activity.Title}");
                }
                catch (Exception ex)
                {
                    await _alertService.ShowAlertAsync("Error", $"Failed to toggle completion: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private void EditActivity(Activity activity)
        {
            if (activity == null) return;

            NewTaskTitle = activity.Title;
            NewTaskDesc = activity.Desc;
            SelectedActivityType = activity.TypeId;
            SelectedStartDate = activity.StartDate.Date;
            SelectedStartTime = activity.StartTime.TimeOfDay;
            AlarmSet = activity.AlarmSet;
            SelectedReminderType = activity.ReminderType;
            SelectedRingTone = activity.RingTone ?? "Default Alarm";
            IsPublic = activity.IsPublic;

            SelectedActivity = activity;
            IsEditMode = true;
            PageTitle = "Edit Activity";
        }

        [RelayCommand]
        public void ResetForm()
        {
            NewTaskTitle = string.Empty;
            NewTaskDesc = string.Empty;
            SelectedActivityType = ActivityType.Other;
            SelectedStartDate = DateTime.Today;
            SelectedStartTime = DateTime.Now.TimeOfDay;
            AlarmSet = false;
            SelectedReminderType = ReminderType.None;
            SelectedRingTone = "Default Alarm";
            SelectedRingtoneObject = null;
            IsPublic = false;
            IsEditMode = false;
            PageTitle = "Create New Activity";
            SelectedActivity = null;

            UpdateNextReminderPreview();
        }

        // ========== METODE PUBLICE ==========
        public async Task SaveActivityToDatabase(Activity activity)
        {
            await _activityService.SaveActivityAsync(activity);
            await LoadActivitiesAsync();
        }

        public async Task AddActivityToDatabase(Activity activity)
        {
            await _activityService.SaveActivityAsync(activity);
            await LoadActivitiesAsync();
        }

        public async Task<List<RingtoneProj>> GetAvailableRingtonesAsync()
        {
            return await _audioService.GetAvailableRingtonesAsync();
        }

        [RelayCommand]
        private async Task OpenActivityDetails(Activity activity)
        {
            if (activity == null) return;

            switch (activity.TypeId)
            {
                case ActivityType.Health:
                    var sportPage = new SportActivityDetailPage(activity);
                    await Application.Current.MainPage.Navigation.PushAsync(sportPage);
                    break;

                case ActivityType.Study:
                    var studyPage = new StudyActivityDetailPage(activity);
                    await Application.Current.MainPage.Navigation.PushAsync(studyPage);
                    break;

                default:
                    var modal = new ActivityModal(this, activity);
                    await Application.Current.MainPage.Navigation.PushModalAsync(modal);
                    break;
            }
        }

        ~MainViewModel()
        {
            if (_activityService is IActivitySubject subject)
            {
                subject.Detach(this);
            }
        }
    }
}