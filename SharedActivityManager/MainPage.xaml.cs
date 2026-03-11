// MainPage.xaml.cs
using SharedActivityManager.ViewModels;
using SharedActivityManager.Models;
using SharedActivityManager.Views;
using SharedActivityManager.Services;

namespace SharedActivityManager;

public partial class MainPage : ContentPage
{
    private MainViewModel _viewModel;

    public MainPage()
    {
        try
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("MainPage: Constructor called");

            _viewModel = new MainViewModel();
            BindingContext = _viewModel;

            // 🔥 Abonare la evenimentul global
            AppEvents.ActivitiesChanged += OnActivitiesChanged;
            System.Diagnostics.Debug.WriteLine("MainPage: Subscribed to AppEvents");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in MainPage constructor: {ex.Message}");
        }
    }

    private async void OnActivitiesChanged()
    {
        System.Diagnostics.Debug.WriteLine("🔥🔥🔥 MainPage: ActivitiesChanged event received!");

        // Asigură-te că rulează pe UI thread
        if (_viewModel != null)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                System.Diagnostics.Debug.WriteLine("MainPage: Reloading activities...");
                await _viewModel.LoadActivitiesAsync();
                System.Diagnostics.Debug.WriteLine("MainPage: Activities reloaded");
            });
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("MainPage: _viewModel is NULL!");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine("MainPage: OnAppearing");

        // Asigură-te că evenimentul este încă abonat
        AppEvents.ActivitiesChanged += OnActivitiesChanged;

        // Reîncărcăm activitățile la fiecare afișare
        Task.Run(async () =>
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await _viewModel.LoadActivitiesAsync();
            });
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        System.Diagnostics.Debug.WriteLine("MainPage: OnDisappearing");

        // NU dezabonați aici - păstrați abonamentul
        // AppEvents.ActivitiesChanged -= OnActivitiesChanged;
    }

    // Restul metodelor rămân la fel
    private async void OnModalOpen(object sender, EventArgs e)
    {
        try
        {
            var activityModal = new ActivityModal(_viewModel);
            await Navigation.PushModalAsync(activityModal);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening modal: {ex.Message}");
            await DisplayAlert("Error", $"Could not open create activity: {ex.Message}", "OK");
        }
    }

    private async void OnActivitySelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (e.CurrentSelection.FirstOrDefault() is Activity selectedActivity)
            {
                var activityModal = new ActivityModal(_viewModel, selectedActivity);
                await Navigation.PushModalAsync(activityModal);
                ((CollectionView)sender).SelectedItem = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error selecting activity: {ex.Message}");
            await DisplayAlert("Error", $"Could not open activity: {ex.Message}", "OK");
        }
    }

    private async void OnSharedActivitiesClicked(object sender, EventArgs e)
    {
        try
        {
            var sharedPage = new Views.SharedActivitiesPage();
            await Navigation.PushAsync(sharedPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to shared: {ex.Message}");
            await DisplayAlert("Error", $"Could not open shared activities: {ex.Message}", "OK");
        }
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        try
        {
            var settingsPage = new SettingsPage();
            await Navigation.PushAsync(settingsPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to settings: {ex.Message}");
            await DisplayAlert("Error", $"Could not open settings: {ex.Message}", "OK");
        }
    }
}