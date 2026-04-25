// ViewModels/SharedActivitiesViewModel.cs
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedActivityManager.Models;
using SharedActivityManager.Services;
using SharedActivityManager.Repositories;
using SharedActivityManager.Services.Observers;

namespace SharedActivityManager.ViewModels
{
    public partial class SharedActivitiesViewModel : ObservableObject, IActivityObserver
    {
        private readonly IActivityService _activityService;
        private readonly IAlertService _alertService;
        private readonly IMessagingService _messagingService;

        [ObservableProperty]
        private ObservableCollection<Activity> sharedActivities = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string currentUserId = "current_user";

        public SharedActivitiesViewModel()
        {
            _activityService = new ActivityService(new Repositories.ActivityRepository());
            _alertService = new AlertService();

            // 🔥 ATTACH OBSERVER LA SERVICE
            if (_activityService is IActivitySubject subject)
            {
                subject.Attach(this);
                System.Diagnostics.Debug.WriteLine("[SharedActivitiesViewModel] Attached as observer");
            }
        }

        public async Task OnActivityChanged(string action, Activity activity = null, int activityCount = 0)
        {
            System.Diagnostics.Debug.WriteLine($"[SharedActivitiesViewModel] Observer received: {action}");

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (action == "Copied" || action == "Added" || action == "Imported")
                {
                    await LoadSharedActivities();
                }
            });
        }

        ~SharedActivitiesViewModel()
        {
            if (_activityService is IActivitySubject subject)
            {
                subject.Detach(this);
            }
        }

        [RelayCommand]
        public async Task LoadSharedActivities()
        {
            try
            {
                IsLoading = true;

                System.Diagnostics.Debug.WriteLine("=== LoadSharedActivities START ===");

                var allActivities = await _activityService.GetActivitiesAsync();
                System.Diagnostics.Debug.WriteLine($"Total activities in DB: {allActivities.Count}");

                // Pentru testare - afișăm toate activitățile publice
                var shared = allActivities
                    .Where(a => a.IsPublic)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Public activities found: {shared.Count}");

                foreach (var act in shared)
                {
                    System.Diagnostics.Debug.WriteLine($"- ID: {act.Id}, Title: '{act.Title}', Desc: '{act.Desc}', Owner: {act.OwnerId}, Type: {act.TypeId}");
                }

                SharedActivities = new ObservableCollection<Activity>(shared);

                System.Diagnostics.Debug.WriteLine("=== LoadSharedActivities END ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading shared activities: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
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
                System.Diagnostics.Debug.WriteLine("=== CopyToMyActivities START ===");

                if (activity == null) return;

                // Deep Copy
                var myCopy = activity.DeepCopy();
                myCopy.OwnerId = CurrentUserId;
                myCopy.IsPublic = false;
                myCopy.AlarmSet = false;
                myCopy.OriginalActivityId = activity.Id;

                await _activityService.SaveActivityAsync(myCopy);

                System.Diagnostics.Debug.WriteLine($"Copy saved with ID: {myCopy.Id}");

                // 🔥 DECLANȘĂ EVENIMENTUL - MainPage se va reîncărca
                AppEvents.OnActivitiesChanged();

                await _alertService.ShowAlertAsync("Success",
                    $"Activity '{activity.Title}' copied to your activities!");

                await LoadSharedActivities();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error copying activity: {ex.Message}");
                await _alertService.ShowAlertAsync("Error", $"Failed to copy activity: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ViewDetails(Activity activity)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== ViewDetails START ===");

                if (activity == null)
                {
                    System.Diagnostics.Debug.WriteLine("Activity is NULL!");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Viewing details for: {activity.Title}");

                var details = $"Title: {activity.Title}\n" +
                             $"Description: {activity.Desc}\n" +
                             $"Type: {activity.TypeId}\n" +
                             $"Date: {activity.StartDate:d}\n" +
                             $"Time: {activity.StartTime:t}\n" +
                             $"Owner: {activity.OwnerId}\n" +
                             $"Public: {activity.IsPublic}\n" +
                             $"ID: {activity.Id}";

                await _alertService.ShowAlertAsync(activity.Title, details);

                System.Diagnostics.Debug.WriteLine("=== ViewDetails END ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error viewing details: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            System.Diagnostics.Debug.WriteLine("Refresh command executed");
            await LoadSharedActivities();
        }
    }
}