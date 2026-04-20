// ViewModels/MainViewModel.cs
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Builders;
using SharedActivityManager.Enums;
using SharedActivityManager.Models;
using SharedActivityManager.Services;
using SharedActivityManager.Repositories;

namespace SharedActivityManager.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        // ========== SERVICII ==========
        private readonly IActivityService _activityService;
        private readonly IAudioService _audioService;
        private readonly IAlarmService _alarmService;
        private readonly IAlertService _alertService;
        private readonly IMessagingService _messagingService;

        // ========== PROPRIETĂȚI PENTRU COLECȚII ==========
        [ObservableProperty]
        private ObservableCollection<Activity> activities = new();

        // ========== TOATE PROPRIETĂȚILE PENTRU FORMULAR ==========
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
            // Inițializare servicii
            _activityService = new ActivityService(new ActivityRepository());
            _alarmService = PlatformServiceLocator.AlarmService;
            _audioService = PlatformServiceLocator.AudioService;
            _alertService = new AlertService();
            _messagingService = new MessagingService();

            // Abonare la mesaje
            _messagingService.Subscribe<ActivitiesChangedMessage>(this, OnActivitiesChanged);

            // Încărcare date inițiale
            Task.Run(async () => await LoadActivitiesAsync()).Wait();

            // Încărcare setări
            LoadSavedRingtone();
            UpdateNextReminderPreview();

            // Restaurare alarme
            Task.Run(async () => await RestoreAlarmsAsync());
        }

        private async void OnActivitiesChanged(ActivitiesChangedMessage message)
        {
            System.Diagnostics.Debug.WriteLine($"ActivitiesChanged: {message.Action}");

            // Reîncarcă activitățile pe UI thread
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await LoadActivitiesAsync();
            });
        }

        // ========== PARTIAL METHODS ==========
        partial void OnSelectedStartDateChanged(DateTime value)
        {
            UpdateNextReminderPreview();
        }

        partial void OnSelectedStartTimeChanged(TimeSpan value)
        {
            UpdateNextReminderPreview();
        }

        partial void OnAlarmSetChanged(bool value)
        {
            UpdateNextReminderPreview();
        }

        partial void OnSelectedReminderTypeChanged(ReminderType value)
        {
            UpdateNextReminderPreview();
        }

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

        // ViewModels/MainViewModel.cs - adaugă această metodă dacă nu există
        [RelayCommand]
        public async Task LoadActivitiesAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MainViewModel: Loading activities...");
                var activitiesFromDb = await _activityService.GetActivitiesAsync();
                Activities = new ObservableCollection<Activity>(activitiesFromDb.OrderBy(a => a.StartDate));

                System.Diagnostics.Debug.WriteLine($"MainViewModel: Loaded {Activities.Count} activities");

                // Afișează activitățile în debug
                foreach (var a in Activities)
                {
                    System.Diagnostics.Debug.WriteLine($"- {a.Title} (ID: {a.Id})");
                }
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
                // 🔥 OBȚINE SAU CREEAZĂ CATEGORIA CORESPUNZĂTOARE
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
                    CategoryId = categoryId  // 🔥 SETEAZĂ CATEGORIA
                };

                await _activityService.SaveActivityWithAlarmAsync(newActivity, _alarmService);
                await LoadActivitiesAsync();
                ResetForm();

                await _alertService.ShowAlertAsync("Success", "Activity added successfully!");
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

            // 🔥 FOLOSEȘTE SERVICE-UL, NU _database
            var categories = await _activityService.GetCategoriesAsync();
            var existing = categories.FirstOrDefault(c => c.Name == categoryName);

            if (existing != null)
                return existing.Id;

            // Creează categorie nouă dacă nu există
            var newCategory = new Category
            {
                Name = categoryName,
                ParentCategoryId = 0,
                DisplayOrder = 1
            };

            await _activityService.SaveCategoryAsync(newCategory);
            return newCategory.Id;
        }

        public async Task SaveEditedActivity()
        {
            if (SelectedActivity == null) return;

            System.Diagnostics.Debug.WriteLine($"=== SaveEditedActivity START ===");
            System.Diagnostics.Debug.WriteLine($"Activity ID: {SelectedActivity.Id}");
            System.Diagnostics.Debug.WriteLine($"Activity Title: {SelectedActivity.Title}");
            System.Diagnostics.Debug.WriteLine($"IsPublic (form): {IsPublic}");
            System.Diagnostics.Debug.WriteLine($"Activity.IsPublic before: {SelectedActivity.IsPublic}");

            if (string.IsNullOrWhiteSpace(NewTaskTitle))
            {
                await _alertService.ShowAlertAsync("Validation", "Title is required");
                return;
            }

            try
            {
                // Anulează alarma veche
                await _alarmService.CancelAlarmAsync(SelectedActivity.Id);
                await Task.Delay(200);

                // Update the activity with form values
                SelectedActivity.Title = NewTaskTitle;
                SelectedActivity.Desc = NewTaskDesc ?? string.Empty;
                SelectedActivity.TypeId = SelectedActivityType;
                SelectedActivity.StartDate = SelectedStartDate.Date;
                SelectedActivity.StartTime = CombinedStartDateTime;
                SelectedActivity.AlarmSet = AlarmSet;
                SelectedActivity.ReminderType = SelectedReminderType;
                SelectedActivity.NextReminderDate = CalculateNextReminderDate();
                SelectedActivity.RingTone = SelectedRingTone ?? "Default";
                SelectedActivity.IsPublic = IsPublic;  // ← IMPORTANT!

                // Dacă devine publică, actualizăm SharedDate
                if (SelectedActivity.IsPublic && SelectedActivity.SharedDate == default)
                {
                    SelectedActivity.SharedDate = DateTime.Now;
                }

                System.Diagnostics.Debug.WriteLine($"Activity.IsPublic after: {SelectedActivity.IsPublic}");

                await _activityService.SaveActivityAsync(SelectedActivity);

                if (AlarmSet && !SelectedActivity.IsCompleted)
                {
                    await _alarmService.ScheduleAlarmAsync(SelectedActivity);
                }

                await LoadActivitiesAsync();
                await _alertService.ShowAlertAsync("Success", "Activity updated successfully!");

                System.Diagnostics.Debug.WriteLine($"=== SaveEditedActivity END ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in SaveEditedActivity: {ex.Message}");
                await _alertService.ShowAlertAsync("Error", $"Failed to update activity: {ex.Message}");
            }
        }

        public async Task AddNewActivity()
        {
            await AddActivity();
        }

        [RelayCommand]
        public async Task DeleteActivity(Activity activity)
        {
            if (activity == null) return;

            try
            {
                await _alarmService.CancelAlarmAsync(activity.Id);
                await _activityService.DeleteActivityAsync(activity);
                await LoadActivitiesAsync();
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
                activity.IsCompleted = !activity.IsCompleted;  // ← folosește IsCompleted

                if (activity.IsCompleted)
                {
                    await _alarmService.CancelAlarmAsync(activity.Id);
                }
                else if (activity.AlarmSet)
                {
                    await _alarmService.ScheduleAlarmAsync(activity);
                }

                await _activityService.SaveActivityAsync(activity);

                var index = Activities.IndexOf(activity);
                if (index >= 0)
                {
                    Activities[index] = activity;
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
            SelectedReminderType = activity.ReminderType;  // ← folosește ReminderType
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

            var factory = Factories.ActivityFactoryRegistry.GetCreator(activity.TypeId);

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

                // Poți adăuga și pentru Shopping și Work

                default:
                    // Deschide modalul de editare existent
                    var modal = new ActivityModal(this, activity);
                    await Application.Current.MainPage.Navigation.PushModalAsync(modal);
                    break;
            }
        }
    }
}