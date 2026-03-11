// ViewModels/SharedActivitiesViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedActivityManager.Models;
using SharedActivityManager.Services;
using System.Collections.ObjectModel;

namespace SharedActivityManager.ViewModels
{
    public partial class SharedActivitiesViewModel : ObservableObject
    {
        private readonly ActivityService _activityService;
        private readonly IAlertService _alertService;

        [ObservableProperty]
        private ObservableCollection<Activity> sharedActivities = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string currentUserId = "current_user"; // Simplificat pentru moment

        public SharedActivitiesViewModel()
        {
            _activityService = new ActivityService();
            _alertService = new AlertService(); // Sau PlatformServiceLocator.AlertService
            LoadSharedActivities();
        }

        [RelayCommand]
        private async Task LoadSharedActivities()
        {
            try
            {
                IsLoading = true;
                var allActivities = await _activityService.GetActivitiesAsync();

                // Filtrează activitățile publice care nu sunt ale utilizatorului curent
                var shared = allActivities
                    .Where(a => a is Activity act &&
                                act.IsPublic &&
                                act.OwnerId != CurrentUserId)
                    .Cast<Activity>()
                    .ToList();

                SharedActivities = new ObservableCollection<Activity>(shared);
            }
            catch (Exception ex)
            {
                await _alertService.ShowAlertAsync("Error", $"Failed to load shared activities: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task CopyToMyActivities(Activity activity)
        {
            try
            {
                if (activity == null) return;

                // 🔥 PROTOTYPE PATTERN IN ACTION: Deep Copy
                var myCopy = activity.DeepCopy();

                // Configurează copia pentru utilizatorul curent
                myCopy.OwnerId = CurrentUserId;
                myCopy.IsPublic = false; // Devine privată
                myCopy.AlarmSet = false; // Fără alarmă implicit
                myCopy.OriginalActivityId = activity.Id;

                await _activityService.SaveActivityAsync(myCopy);

                await _alertService.ShowAlertAsync("Success",
                    $"Activity '{activity.Title}' copied to your activities!");

                // Reîncarcă lista (opțional)
                await LoadSharedActivities();
            }
            catch (Exception ex)
            {
                await _alertService.ShowAlertAsync("Error", $"Failed to copy activity: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ViewDetails(Activity activity)
        {
            if (activity == null) return;

            // Navighează la pagina de detalii (poți implementa mai târziu)
            var details = activity.GetActivityDetails();
            await _alertService.ShowAlertAsync(activity.Title, details);
        }

        [RelayCommand]
        private async Task Refresh()
        {
            await LoadSharedActivities();
        }
    }
}