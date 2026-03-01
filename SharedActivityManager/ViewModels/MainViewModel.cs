using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedActivityManager.Data;
using SharedActivityManager.Enums;
using SharedActivityManager.Models;
using SharedActivityManager.Services;

namespace SharedActivityManager.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ActivityDataBase _database;
        private readonly IAlarmService _alarmService;
        private readonly IRingtoneService _ringtoneService;

        [ObservableProperty]
        private ObservableCollection<Activity> activities;

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
        private string selectedRingTone = "Default";

        [ObservableProperty]
        private Ringtone selectedRingtoneObject;

        [ObservableProperty]
        private DateTime nextReminderDatePreview;

        [ObservableProperty]
        private bool isEditMode;

        [ObservableProperty]
        private string pageTitle;

        [ObservableProperty]
        private Activity selectedActivity;

        // Proprietate pentru data și ora combinată
        public DateTime CombinedStartDateTime => SelectedStartDate.Add(SelectedStartTime);

        // Liste pentru pickere
        public ObservableCollection<ActivityType> ActivityTypeList { get; } =
            new ObservableCollection<ActivityType>(Enum.GetValues<ActivityType>());

        public ObservableCollection<ReminderType> ReminderTypeList { get; } =
            new ObservableCollection<ReminderType>(Enum.GetValues<ReminderType>());

        public ObservableCollection<string> RingToneList { get; } =
            new ObservableCollection<string>
            {
                "Default",
                "Morning Alarm",
                "Digital Buzz",
                "Gentle Reminder",
                "Urgent",
                "Silent"
            };

        // Constructor
        public MainViewModel()
        {
            _database = new ActivityDataBase();
            _ringtoneService = new RingtoneService();
            _alarmService = new PlatformAlarmService();

            activities = new ObservableCollection<Activity>();
            LoadActivities();

            LoadSavedRingtone();
            UpdateNextReminderPreview();
        }


        private async void ScheduleExistingAlarms()
        {
            try
            {
                var alarmService = new PlatformAlarmService();
                await alarmService.RestoreAlarmsAsync(Activities.ToList());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring alarms: {ex.Message}");
            }
        }

        // Încarcă ringtone-ul salvat
        private void LoadSavedRingtone()
        {
            try
            {
                var savedRingtoneId = _ringtoneService.LoadSelectedRingtone();
                var ringtones = _ringtoneService.GetAvailableRingtones();
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
        public async Task LoadActivities()
        {
            var activities = await _database.GetActivitiesAsync();
            Activities = new ObservableCollection<Activity>(activities.OrderBy(a => a.StartDate));
        }

        // Adăugare activitate nouă
        [RelayCommand]
        private async void AddActivity()
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
                Activities.Add(newActivity);

                // Sortare după dată
                Activities = new ObservableCollection<Activity>(
                    Activities.OrderBy(a => a.StartDate));

                // Resetare formular
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

        // ===== METODE NOI ADĂUGATE =====

        // Salvare activitate editată
        public async Task SaveEditedActivity()
        {
            if (SelectedActivity == null) return;

            if (string.IsNullOrWhiteSpace(NewTaskTitle))
            {
                await App.Current.MainPage.DisplayAlert("Validation",
                    "Title is required", "OK");
                return;
            }

            try
            {
                SelectedActivity.Title = NewTaskTitle;
                SelectedActivity.Desc = NewTaskDesc ?? string.Empty;
                SelectedActivity.TypeId = SelectedActivityType;
                SelectedActivity.StartDate = SelectedStartDate;
                SelectedActivity.StartTime = CombinedStartDateTime;
                SelectedActivity.AlarmSet = AlarmSet;
                SelectedActivity.ReminderTypeId = SelectedReminderType;
                SelectedActivity.NextReminderDate = CalculateNextReminderDate();
                SelectedActivity.RingTone = SelectedRingTone ?? "Default";

                await _database.SaveActivityAsync(SelectedActivity);

                // Reîncărcăm lista
                await LoadActivitiesAsync();

                await App.Current.MainPage.DisplayAlert("Success",
                    "Activity updated successfully!", "OK");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error",
                    $"Failed to update activity: {ex.Message}", "OK");
            }
        }

        // Adăugare activitate nouă (versiunea Task pentru a putea fi așteptată)
        public async Task AddNewActivity()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle))
            {
                await App.Current.MainPage.DisplayAlert("Validation", "Title is required", "OK");
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
                    RingTone = SelectedRingTone ?? "Default Alarm"
                };

                await _database.SaveActivityAsync(newActivity);

                // Programează alarmă dacă e setată
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

        // ===== METODE EXISTENTE =====

        // Ștergere activitate
        [RelayCommand]
        public async Task DeleteActivity(Activity activity)
        {
            if (activity == null) return;

            try
            {
                // Anulează alarma înainte de ștergere
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
        private async void ToggleComplete(Activity activity)
        {
            if (activity != null)
            {
                activity.isCompleted = !activity.isCompleted;

                if (activity.isCompleted)
                {
                    // Dacă e completată, anulează alarma
                    await _alarmService.CancelAlarmAsync(activity.Id);
                }
                else if (activity.AlarmSet)
                {
                    // Dacă e reactivată, programează alarma
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

        // Obține lista de ringtone-uri disponibile
        public List<Ringtone> GetAvailableRingtones()
        {
            return _ringtoneService.GetAvailableRingtones();
        }

        public void ForceUpdateIsEditMode()
        {
            OnPropertyChanged(nameof(IsEditMode));
        }
    }
}