// MainPage.xaml.cs
using SharedActivityManager.ViewModels;
using SharedActivityManager.Models;
using SharedActivityManager;

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

    private async void OnModalOpen(object sender, EventArgs e)
    {
        try
        {
            var activityModal = new ActivityModal(_viewModel);
            await Navigation.PushModalAsync(activityModal);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
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
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}