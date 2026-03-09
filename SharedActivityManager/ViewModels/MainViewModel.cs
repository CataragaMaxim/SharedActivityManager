using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Data;
using SharedActivityManager.Enums;
using SharedActivityManager.Factories;
using SharedActivityManager.Models;
using SharedActivityManager.Services;

namespace SharedActivityManager.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ActivityDataBase _database;
        private readonly IAudioService _audioService;  // ← Folosim IAudioService
        private readonly IAlarmService _alarmService;

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
        private string pageTitle;

        [ObservableProperty]
        private Activity selectedActivity;

        public DateTime CombinedStartDateTime => SelectedStartDate.Add(SelectedStartTime);

        public ObservableCollection<ActivityType> ActivityTypeList { get; } =
            new ObservableCollection<ActivityType>(Enum.GetValues<ActivityType>());

        public ObservableCollection<ReminderType> ReminderTypeList { get; } =
            new ObservableCollection<ReminderType>(Enum.GetValues<ReminderType>());

        public ObservableCollection<string> RingToneList { get; } =
            new ObservableCollection<string>
            {
                "Default Alarm",
                "Digital Beep",
                "Gentle Wake",
                "Morning Bliss",
                "Classic Ring"
            };

        public MainViewModel()
        {
            _database = new ActivityDataBase();
            _alarmService = PlatformServiceLocator.AlarmService;
            _audioService = PlatformServiceLocator.AudioService;  // ← Folosim AudioService

            activities = new ObservableCollection<Activity>();

            // Încarcă activitățile
            Task.Run(async () => await LoadActivities()).Wait();

            LoadSavedRingtone();
            UpdateNextReminderPreview();

            // Restaurează alarmele
            Task.Run(async () => await RestoreAlarmsAsync());
        }



        // Restaurează alarmele la pornire
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

        // Încarcă ringtone-ul salvat
        private async void LoadSavedRingtone()
        {
            try
            {
                var savedRingtoneId = Preferences.Get("SelectedRingtone", "default1");
                var ringtones = await _audioService.GetAvailableRingtonesAsync();  // ← Folosim _audioService
                SelectedRingtoneObject = ringtones.FirstOrDefault(r => r.Id == savedRingtoneId);
                SelectedRingTone = SelectedRingtoneObject?.DisplayName ?? "Default Alarm";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading saved ringtone: {ex.Message}");
                SelectedRingTone = "Default Alarm";
            }
        }

        // Încărcare activități
        [RelayCommand]
        private async Task LoadActivities()
        {
            var activitiesFromDb = await _database.GetActivitiesAsync();
            Activities = new ObservableCollection<Activity>(activitiesFromDb.OrderBy(a => a.StartDate));
        }

        // Adăugare activitate nouă
        [RelayCommand]
        private async Task AddActivity()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle))
            {
                await App.Current.MainPage.DisplayAlert("Validation",
                    "Title is required", "OK");
                return;
            }

            try
            {
                var newActivity = new Activity
                {
                    Title = NewTaskTitle,
                    Desc = NewTaskDesc ?? string.Empty,
                    TypeId = SelectedActivityType,
                    StartDate = SelectedStartDate,
                    StartTime = CombinedStartDateTime,
                    AlarmSet = AlarmSet,
                    isCompleted = false,
                    ReminderTypeId = SelectedReminderType,
                    NextReminderDate = CalculateNextReminderDate(),
                    RingTone = SelectedRingTone ?? "Default"
                };

                await _database.SaveActivityAsync(newActivity);

                if (newActivity.AlarmSet)
                {
                    await _alarmService.ScheduleAlarmAsync(newActivity);
                }

                Activities.Add(newActivity);
                Activities = new ObservableCollection<Activity>(Activities.OrderBy(a => a.StartDate));
                ResetForm();

                await App.Current.MainPage.DisplayAlert("Success",
                    "Activity added successfully!", "OK");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error",
                    $"Failed to add activity: {ex.Message}", "OK");
            }
        }

        // Salvare activitate editată
        public async Task SaveEditedActivity()
        {
            if (SelectedActivity == null) return;

            System.Diagnostics.Debug.WriteLine($"=== SaveEditedActivity START ===");
            System.Diagnostics.Debug.WriteLine($"Activity ID: {SelectedActivity.Id}");
            System.Diagnostics.Debug.WriteLine($"Activity Title: {SelectedActivity.Title}");
            System.Diagnostics.Debug.WriteLine($"Current AlarmSet (form): {AlarmSet}");
            System.Diagnostics.Debug.WriteLine($"Current isCompleted (form): {SelectedActivity.isCompleted}");

            if (string.IsNullOrWhiteSpace(NewTaskTitle))
            {
                await App.Current.MainPage.DisplayAlert("Validation", "Title is required", "OK");
                return;
            }

            try
            {
                int activityId = SelectedActivity.Id;

                // 🔥 FIX: Anulează alarma și așteaptă confirmarea
                System.Diagnostics.Debug.WriteLine($"Cancelling old alarm for ID: {SelectedActivity.Id}");
                await _alarmService.CancelAlarmAsync(SelectedActivity.Id);

                // 🔥 FIX: O mică pauză pentru a permite anularea completă
                await Task.Delay(200);

                // Update the activity with form values
                SelectedActivity.Title = NewTaskTitle;
                SelectedActivity.Desc = NewTaskDesc ?? string.Empty;
                SelectedActivity.TypeId = SelectedActivityType;
                SelectedActivity.StartDate = SelectedStartDate.Date;
                SelectedActivity.StartTime = DateTime.Today.Add(SelectedStartTime);
                SelectedActivity.AlarmSet = AlarmSet;
                SelectedActivity.ReminderTypeId = SelectedReminderType;
                SelectedActivity.NextReminderDate = CalculateNextReminderDate();
                SelectedActivity.RingTone = SelectedRingTone ?? "Default";

                await _database.SaveActivityAsync(SelectedActivity);
                System.Diagnostics.Debug.WriteLine($"Activity saved to database");

                if (AlarmSet && !SelectedActivity.isCompleted)
                {
                    System.Diagnostics.Debug.WriteLine($"SCHEDULING NEW ALARM for activity {SelectedActivity.Id}");
                    await _alarmService.ScheduleAlarmAsync(SelectedActivity);
                    System.Diagnostics.Debug.WriteLine($"ScheduleAlarmAsync completed");
                }
                else
                {
                    if (!AlarmSet)
                        System.Diagnostics.Debug.WriteLine($"No new alarm scheduled - AlarmSet is false");
                    else if (SelectedActivity.isCompleted)
                        System.Diagnostics.Debug.WriteLine($"No new alarm scheduled - Activity is completed");
                }

                await LoadActivitiesAsync();
                await App.Current.MainPage.DisplayAlert("Success", "Activity updated successfully!", "OK");

                System.Diagnostics.Debug.WriteLine($"=== SaveEditedActivity END ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in SaveEditedActivity: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to update activity: {ex.Message}", "OK");
            }
        }

        // Adăugare activitate nouă (versiunea Task)
        private ActivityCreator GetActivityCreator()
        {
            return ActivityFactoryRegistry.GetCreator(SelectedActivityType);
        }

        public async Task AddNewActivity()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle))
            {
                await App.Current.MainPage.DisplayAlert("Validation", "Title is required", "OK");
                return;
            }

            try
            {
                var additionalParams = new Dictionary<string, object>();

                switch (SelectedActivityType)
                {
                    case ActivityType.Work:
                        additionalParams["Priority"] = "Medium";
                        additionalParams["ProjectName"] = "General";
                        break;
                    case ActivityType.Personal:
                        additionalParams["Location"] = "Home";
                        additionalParams["Mood"] = "Relaxed";
                        break;
                    case ActivityType.Health:
                        additionalParams["HealthType"] = "Gym";
                        additionalParams["DurationMinutes"] = 30;
                        break;
                    case ActivityType.Study:
                        additionalParams["Subject"] = "General";
                        additionalParams["Mode"] = "Reading";
                        break;
                }

                var creator = GetActivityCreator();
                var newActivity = await creator.CreateAndConfigureActivity(
                    NewTaskTitle,
                    NewTaskDesc ?? string.Empty,
                    CombinedStartDateTime,
                    additionalParams
                );

                newActivity.StartDate = SelectedStartDate.Date;
                newActivity.ReminderTypeId = SelectedReminderType;
                newActivity.NextReminderDate = CalculateNextReminderDate();
                newActivity.RingTone = SelectedRingTone ?? "Default Alarm";

                await _database.SaveActivityAsync(newActivity);

                if (newActivity.AlarmSet)
                {
                    await _alarmService.ScheduleAlarmAsync(newActivity);
                }

                Activities.Add(newActivity);
                Activities = new ObservableCollection<Activity>(Activities.OrderBy(a => a.StartDate));
                ResetForm();

                await App.Current.MainPage.DisplayAlert("Success", "Activity added successfully!", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding activity: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to add activity: {ex.Message}", "OK");
            }
        }

        // Încărcare activități (versiunea Task)
        public async Task LoadActivitiesAsync()
        {
            try
            {
                var activitiesFromDb = await _database.GetActivitiesAsync();
                Activities = new ObservableCollection<Activity>(
                    activitiesFromDb.OrderBy(a => a.StartDate));
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error",
                    $"Failed to load activities: {ex.Message}", "OK");
            }
        }

        // Ștergere activitate
        [RelayCommand]
        public async Task DeleteActivity(Activity activity)
        {
            if (activity == null) return;

            try
            {
                await _alarmService.CancelAlarmAsync(activity.Id);
                await _database.DeleteActivityAsync(activity);
                Activities.Remove(activity);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to delete activity: {ex.Message}", "OK");
            }
        }

        // Ștergere activitate curentă
        [RelayCommand]
        public async Task DeleteCurrentActivity()
        {
            if (SelectedActivity != null)
            {
                await DeleteActivity(SelectedActivity);
            }
        }

        // Salvare activitate (determină automat dacă e editare sau adăugare)
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

        // Toggle completare activitate
        [RelayCommand]
        private async Task ToggleComplete(Activity activity)  // ← Schimbat din void în Task
        {
            if (activity != null)
            {
                activity.isCompleted = !activity.isCompleted;

                if (activity.isCompleted)
                {
                    await _alarmService.CancelAlarmAsync(activity.Id);
                }
                else if (activity.AlarmSet)
                {
                    await _alarmService.ScheduleAlarmAsync(activity);
                }

                await _database.SaveActivityAsync(activity);

                var index = Activities.IndexOf(activity);
                if (index >= 0)
                {
                    Activities[index] = activity;
                }
            }
        }

        // Editare activitate
        [RelayCommand]
        private void EditActivity(Activity activity)
        {
            if (activity == null) return;
            SelectedActivity = activity;
        }

        // Resetare formular
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

            UpdateNextReminderPreview();
        }

        // Calculare dată următor reminder
        public DateTime CalculateNextReminderDate()
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

        // Update previzualizare reminder
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

        private void UpdateNextReminderPreview()
        {
            NextReminderDatePreview = CalculateNextReminderDate();
        }

        // Metodă pentru deschiderea ringtone picker
        [RelayCommand]
        private async Task OpenRingtonePicker()
        {
            await Task.CompletedTask;
        }

        public async Task SaveActivityToDatabase(Activity activity)
        {
            await _database.SaveActivityAsync(activity);
            await LoadActivities();
        }

        public async Task AddActivityToDatabase(Activity activity)
        {
            await _database.SaveActivityAsync(activity);
            await LoadActivities();
        }

        // Obține lista de ringtone-uri disponibile - FIXAT
        public async Task<List<RingtoneProj>> GetAvailableRingtonesAsync()
        {
            return await _audioService.GetAvailableRingtonesAsync();  // ← Folosește _audioService
        }
    }
}