using SharedActivityManager.ViewModels;
using SharedActivityManager.Models;
using SharedActivityManager.Enums;

namespace SharedActivityManager;

public partial class MainPage : ContentPage
{
    private MainViewModel _viewModel;

    public MainPage()
    {
        try
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            BindingContext = _viewModel;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in MainPage constructor: {ex.Message}");
        }
    }

    // 🔥 METODĂ MODIFICATĂ: Deschide meniu cu tipuri de activități
    private async void OnModalOpen(object sender, EventArgs e)
    {
        try
        {
            var action = await DisplayActionSheet("Select Activity Type", "Cancel", null,
                "💼 Work",
                "🏃 Sport",
                "📚 Study",
                "🛒 Shopping",
                "📝 Other");

            switch (action)
            {
                case "💼 Work":
                    _viewModel.SelectedActivityType = ActivityType.Work;
                    break;
                case "🏃 Sport":
                    _viewModel.SelectedActivityType = ActivityType.Health;
                    break;
                case "📚 Study":
                    _viewModel.SelectedActivityType = ActivityType.Study;
                    break;
                case "🛒 Shopping":
                    _viewModel.SelectedActivityType = ActivityType.Personal;
                    break;
                case "📝 Other":
                    _viewModel.SelectedActivityType = ActivityType.Other;
                    break;
                default:
                    return; // Cancel sau altceva
            }

            // Resetează formularul și deschide modal
            _viewModel.ResetForm();
            _viewModel.IsEditMode = false;
            _viewModel.PageTitle = $"Create New {action} Activity";

            var activityModal = new ActivityModal(_viewModel);
            await Navigation.PushModalAsync(activityModal);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening modal: {ex.Message}");
            await DisplayAlert("Error", $"Could not open create activity: {ex.Message}", "OK");
        }
    }

    // 🔥 METODELE EXISTENTE RĂMÂN
    private async void OnSharedActivitiesClicked(object sender, EventArgs e)
    {
        try
        {
            var sharedPage = new SharedActivitiesPage();
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

    private async void OnCategoriesClicked(object sender, EventArgs e)
    {
        try
        {
            var categoriesPage = new CategoriesPage();
            await Navigation.PushAsync(categoriesPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to categories: {ex.Message}");
            await DisplayAlert("Error", $"Could not open categories: {ex.Message}", "OK");
        }
    }

    // 🔥 METODE PENTRU BUTOANELE DIRECTE (opțional, dacă vrei să păstrezi ambele opțiuni)
    private async void OnCreateWorkClicked(object sender, EventArgs e)
    {
        _viewModel.SelectedActivityType = ActivityType.Work;
        _viewModel.ResetForm();
        _viewModel.IsEditMode = false;
        _viewModel.PageTitle = "Create New Work Activity";

        var activityModal = new ActivityModal(_viewModel);
        await Navigation.PushModalAsync(activityModal);
    }

    private async void OnCreateSportClicked(object sender, EventArgs e)
    {
        _viewModel.SelectedActivityType = ActivityType.Health;
        _viewModel.ResetForm();
        _viewModel.IsEditMode = false;
        _viewModel.PageTitle = "Create New Sport Activity";

        var activityModal = new ActivityModal(_viewModel);
        await Navigation.PushModalAsync(activityModal);
    }

    private async void OnCreateStudyClicked(object sender, EventArgs e)
    {
        _viewModel.SelectedActivityType = ActivityType.Study;
        _viewModel.ResetForm();
        _viewModel.IsEditMode = false;
        _viewModel.PageTitle = "Create New Study Activity";

        var activityModal = new ActivityModal(_viewModel);
        await Navigation.PushModalAsync(activityModal);
    }

    private async void OnCreateShoppingClicked(object sender, EventArgs e)
    {
        _viewModel.SelectedActivityType = ActivityType.Personal;
        _viewModel.ResetForm();
        _viewModel.IsEditMode = false;
        _viewModel.PageTitle = "Create New Shopping Activity";

        var activityModal = new ActivityModal(_viewModel);
        await Navigation.PushModalAsync(activityModal);
    }

    private async void OnActivitySelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (e.CurrentSelection.FirstOrDefault() is Activity selectedActivity)
            {
                await OpenActivityDetails(selectedActivity);
                ((CollectionView)sender).SelectedItem = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error selecting activity: {ex.Message}");
            await DisplayAlert("Error", $"Could not open activity: {ex.Message}", "OK");
        }
    }

    private async Task OpenActivityDetails(Activity activity)
    {
        switch (activity.TypeId)
        {
            case ActivityType.Health:
                var sportPage = new SportActivityDetailPage(activity);
                await Navigation.PushAsync(sportPage);
                break;

            case ActivityType.Study:
                var studyPage = new StudyActivityDetailPage(activity);
                await Navigation.PushAsync(studyPage);
                break;

            case ActivityType.Personal:
                // Pentru Shopping poți crea o pagină similară
                await Navigation.PushModalAsync(new ActivityModal(_viewModel, activity));
                break;

            case ActivityType.Work:
                await Navigation.PushModalAsync(new ActivityModal(_viewModel, activity));
                break;

            default:
                var modal = new ActivityModal(_viewModel, activity);
                await Navigation.PushModalAsync(modal);
                break;
        }
    }
}