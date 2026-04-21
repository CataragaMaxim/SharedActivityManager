// Views/SharedActivitiesPage.xaml.cs
using SharedActivityManager.Services;
using SharedActivityManager.ViewModels;

namespace SharedActivityManager
{
    public partial class SharedActivitiesPage : ContentPage
    {
        private SharedActivitiesViewModel _viewModel;

        public SharedActivitiesPage()
        {
            try
            {
                InitializeComponent();
                System.Diagnostics.Debug.WriteLine("SharedActivitiesPage initialized");

                _viewModel = new SharedActivitiesViewModel();
                BindingContext = _viewModel;

                System.Diagnostics.Debug.WriteLine("BindingContext set");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in constructor: {ex.Message}");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("SharedActivitiesPage OnAppearing");

            if (_viewModel != null)
            {
                await _viewModel.LoadSharedActivities();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("_viewModel is NULL!");
            }
        }
    }
}