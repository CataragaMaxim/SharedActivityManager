using SharedActivityManager.ViewModels;
using SharedActivityManager.Models;

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

    // Metoda pentru deschiderea modalului de creare activitate nouă
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

    // Metoda pentru selectarea unei activități (editare)
    private async void OnActivitySelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (e.CurrentSelection.FirstOrDefault() is Activity selectedActivity)
            {
                var activityModal = new ActivityModal(_viewModel, selectedActivity);
                await Navigation.PushModalAsync(activityModal);

                // Deselectează
                ((CollectionView)sender).SelectedItem = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error selecting activity: {ex.Message}");
            await DisplayAlert("Error", $"Could not open activity: {ex.Message}", "OK");
        }
    }
}