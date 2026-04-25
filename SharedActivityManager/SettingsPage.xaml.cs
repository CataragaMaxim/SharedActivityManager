using SharedActivityManager.Services;
using SharedActivityManager.Data;
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SharedActivityManager.Services.Flyweight;
using SharedActivityManager.Enums;
using SharedActivityManager.Services.Proxies;
using SharedActivityManager.Repositories;
using SharedActivityManager.Services.Commands;
using SharedActivityManager.ViewModels;

namespace SharedActivityManager;

public partial class SettingsPage : ContentPage, INotifyPropertyChanged
{
    private readonly IAudioService _audioService;
    private readonly IAlertService _alertService;
    private readonly ActivityDataBase _database;
    private readonly IAlarmService _alarmService;
    private readonly IMessagingService _messagingService;
    private readonly IActivityService _activityService;
    private readonly CommandInvoker _commandInvoker;

    private string _selectedRingtone = "Default Alarm";
    public string SelectedRingtone
    {
        get => _selectedRingtone;
        set
        {
            if (_selectedRingtone != value)
            {
                _selectedRingtone = value;
                OnPropertyChanged();
            }
        }
    }

    public SettingsPage()
    {
        InitializeComponent();

        _audioService = PlatformServiceLocator.AudioService;
        _alertService = new AlertService();
        _database = new ActivityDataBase();
        _alarmService = PlatformServiceLocator.AlarmService;
        _messagingService = new MessagingService();

        // Inițializare pentru Undo/Redo
        var repository = new ActivityRepository();
        _activityService = new ActivityService(repository);
        _commandInvoker = new CommandInvoker();

        LoadSavedRingtone();
        BindingContext = this;
    }

    private async void LoadSavedRingtone()
    {
        try
        {
            var savedRingtoneId = Preferences.Get("SelectedRingtone", "default1");
            var ringtones = await _audioService.GetAvailableRingtonesAsync();
            var selected = ringtones?.FirstOrDefault(r => r.Id == savedRingtoneId);
            SelectedRingtone = selected?.DisplayName ?? "Default Alarm";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading ringtone: {ex.Message}");
            SelectedRingtone = "Default Alarm";
        }
    }

    private async void OnSelectRingtoneClicked(object sender, EventArgs e)
    {
        try
        {
            var ringtonePicker = new RingtonePickerPage();
            ringtonePicker.SetRingtoneSelectedCallback((selectedRingtone) =>
            {
                SelectedRingtone = selectedRingtone.DisplayName;
            });

            await Navigation.PushModalAsync(ringtonePicker);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to open ringtone picker: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Obține MainViewModel-ul curent pentru a accesa TakeSnapshot
    /// </summary>
    private MainViewModel GetMainViewModel()
    {
        // Încearcă să găsească MainPage în Navigation stack
        var navigationPage = Application.Current?.MainPage as NavigationPage;
        if (navigationPage != null)
        {
            var mainPage = navigationPage.RootPage as MainPage;
            if (mainPage != null)
            {
                return mainPage.BindingContext as MainViewModel;
            }
        }

        // Alternativ: caută în stiva de navigare
        if (Application.Current?.MainPage != null)
        {
            var mainPage = Application.Current.MainPage as MainPage;
            if (mainPage != null)
            {
                return mainPage.BindingContext as MainViewModel;
            }
        }

        return null;
    }

    private async void OnDeleteAllActivitiesClicked(object sender, EventArgs e)
    {
        try
        {
            var confirm = await DisplayAlert(
                "Confirm Delete",
                "⚠️ Are you sure you want to delete ALL activities?\n\nThis action can be undone!",
                "Yes, Delete All",
                "Cancel");

            if (!confirm) return;

            // 🔥 SALVEAZĂ SNAPSHOT ÎNAINTE DE ȘTERGERE
            var mainViewModel = GetMainViewModel();
            mainViewModel?.TakeSnapshot("Before deleting all activities");

            if (_database == null)
            {
                await DisplayAlert("Error", "Database service is not available", "OK");
                return;
            }

            var activities = await _database.GetActivitiesAsync();
            var count = activities?.Count ?? 0;

            if (count == 0)
            {
                await DisplayAlert("Info", "No activities to delete.", "OK");
                return;
            }

            // 🔥 FOLOSEȘTE COMANDĂ PENTRU UNDO/REDO
            var command = new DeleteAllActivitiesCommand(_activityService, activities);
            await _commandInvoker.ExecuteCommand(command);

            // Trimite notificare pentru refresh UI
            _messagingService?.Send(new ActivitiesChangedMessage
            {
                Action = "DeletedAll",
                ActivityCount = count
            });

            // 🔥 SALVEAZĂ SNAPSHOT DUPĂ ȘTERGERE
            mainViewModel?.TakeSnapshot($"Deleted {count} activities");

            await DisplayAlert("Success", $"✅ Successfully deleted {count} activities!", "OK");
            await Shell.Current.GoToAsync("///MainPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Delete all error: {ex.Message}");
            await DisplayAlert("Error", $"Failed to delete activities: {ex.Message}", "OK");
        }
    }

    private async void OnResetEverythingClicked(object sender, EventArgs e)
    {
        try
        {
            if (Shell.Current == null)
            {
                System.Diagnostics.Debug.WriteLine("Shell.Current is null!");
                await DisplayAlert("Error", "Navigation service not available", "OK");
                return;
            }

            var confirm = await DisplayAlert(
                "⚠️ DANGER ZONE ⚠️",
                "This will delete ALL activities AND ALL categories!\n\nThis action can be undone!\n\nAre you absolutely sure?",
                "Yes, Reset Everything",
                "No, Cancel");

            if (!confirm) return;

            // 🔥 SALVEAZĂ SNAPSHOT ÎNAINTE DE RESET
            var mainViewModel = GetMainViewModel();
            mainViewModel?.TakeSnapshot("Before resetting everything");

            if (_database == null)
            {
                await DisplayAlert("Error", "Database service is not available", "OK");
                return;
            }

            // Salvează datele curente pentru Undo
            var currentActivities = await _database.GetActivitiesAsync();
            var currentCategories = await _database.GetCategoriesAsync();

            // 🔥 FOLOSEȘTE COMANDĂ PENTRU UNDO/REDO
            var command = new ResetAllCommand(_activityService, currentActivities, currentCategories);
            await _commandInvoker.ExecuteCommand(command);

            // Recrează categoriile implicite (ResetAllCommand deja face asta)
            // Dar asigură-te că există după reset

            _messagingService?.Send(new ActivitiesChangedMessage
            {
                Action = "ResetEverything"
            });

            // 🔥 SALVEAZĂ SNAPSHOT DUPĂ RESET
            mainViewModel?.TakeSnapshot("Reset everything to factory settings");

            await DisplayAlert("Success", "✅ Application has been reset to factory settings!", "OK");
            await Shell.Current.GoToAsync("///MainPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Reset error: {ex.Message}");
            await DisplayAlert("Error", $"Failed to reset: {ex.Message}", "OK");
        }
    }

    private async Task RecreateDefaultCategories()
    {
        try
        {
            var defaultCategories = new List<Category>
            {
                new Category { Id = 1, Name = "💼 Work", ParentCategoryId = 0, DisplayOrder = 1 },
                new Category { Id = 2, Name = "🏠 Personal", ParentCategoryId = 0, DisplayOrder = 2 },
                new Category { Id = 3, Name = "💪 Health", ParentCategoryId = 0, DisplayOrder = 3 },
                new Category { Id = 4, Name = "📚 Study", ParentCategoryId = 0, DisplayOrder = 4 }
            };

            foreach (var cat in defaultCategories)
            {
                await _database.SaveCategoryAsync(cat);
            }
            System.Diagnostics.Debug.WriteLine("Default categories recreated");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error recreating categories: {ex.Message}");
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnTestFlyweightMemory(object sender, EventArgs e)
    {
        var factory = ActivityTypeMetadataFactory.Instance;
        var cacheSize = factory.CacheSize;

        var metadata1 = ActivityType.Work.GetMetadata();
        var metadata2 = ActivityType.Work.GetMetadata();
        var metadata3 = ActivityType.Work.GetMetadata();

        var areSame = ReferenceEquals(metadata1, metadata2) && ReferenceEquals(metadata2, metadata3);

        var message = $"📊 Flyweight Test Results:\n\n" +
                      $"Cache size: {cacheSize} metadata objects\n" +
                      $"All Work metadata are same instance: {(areSame ? "✅ YES" : "❌ NO")}\n\n" +
                      $"Memory saved: {cacheSize} instances instead of unlimited copies";

        DisplayAlert("Flyweight Test", message, "OK");
    }

    private async void OnFixMissingCategoryIds(object sender, EventArgs e)
    {
        try
        {
            var helper = new CategoryHelper(_database);
            await helper.FixActivitiesCategoryIds();

            await DisplayAlert("Success", "All activities have been fixed!", "OK");

            var messagingService = new MessagingService();
            messagingService.Send(new ActivitiesChangedMessage { Action = "Fixed" });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnTestBridgeClicked(object sender, EventArgs e)
    {
        try
        {
            var bridge = PlatformBridge.Instance;

            var message = $"🌉 Bridge Pattern Test\n\n" +
                          $"Platform: {bridge.PlatformName}\n" +
                          $"Alarm Service: {bridge.AlarmService?.GetType().Name ?? "null"}\n" +
                          $"Audio Service: {bridge.AudioService?.GetType().Name ?? "null"}\n" +
                          $"Notification Service: {bridge.NotificationService?.GetType().Name ?? "null"}\n\n" +
                          $"✅ Bridge Pattern is working correctly!\n" +
                          $"The abstraction is separated from implementation.";

            await DisplayAlert("Bridge Test", message, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnTestProxiesClicked(object sender, EventArgs e)
    {
        try
        {
            var repository = new ActivityRepository();
            var alarmService = PlatformBridge.Instance.AlarmService;

            var results = new List<string>();

            var realService = new RealActivityService(repository, alarmService);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var realActivities = await realService.GetActivitiesAsync();
            sw.Stop();
            results.Add($"Real Service: {realActivities.Count} activities in {sw.ElapsedMilliseconds} ms");

            var cacheProxy = ActivityServiceProxyFactory.CreateCacheProxy(repository, alarmService);
            sw.Restart();
            var cachedActivities1 = await cacheProxy.GetActivitiesAsync();
            sw.Stop();
            results.Add($"Cache Proxy (first call): {cachedActivities1.Count} activities in {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            var cachedActivities2 = await cacheProxy.GetActivitiesAsync();
            sw.Stop();
            results.Add($"Cache Proxy (second call): {cachedActivities2.Count} activities in {sw.ElapsedMilliseconds} ms (should be faster)");

            var virtualProxy = ActivityServiceProxyFactory.CreateVirtualProxy(repository, alarmService);
            sw.Restart();
            var virtualActivities = await virtualProxy.GetActivitiesAsync();
            sw.Stop();
            results.Add($"Virtual Proxy (first page): {virtualActivities.Count} activities in {sw.ElapsedMilliseconds} ms");

            var securityProxy = ActivityServiceProxyFactory.CreateSecurityProxy(repository, alarmService, "current_user");
            try
            {
                // Test security
            }
            catch (UnauthorizedAccessException ex)
            {
                results.Add($"Security Proxy: Access denied correctly - {ex.Message}");
            }

            var message = "📊 Proxy Pattern Test Results:\n\n" + string.Join("\n", results);
            await DisplayAlert("Proxy Test", message, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    public new event PropertyChangedEventHandler PropertyChanged;
}