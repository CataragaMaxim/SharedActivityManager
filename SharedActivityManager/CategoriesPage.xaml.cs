// CategoriesPage.xaml.cs
using SharedActivityManager.ViewModels;
using SharedActivityManager.Models;

namespace SharedActivityManager
{
    public partial class CategoriesPage : ContentPage
    {
        private CategoriesViewModel _viewModel;

        public CategoriesPage()
        {
            InitializeComponent();
            _viewModel = new CategoriesViewModel();
            BindingContext = _viewModel;
        }

        private void OnCategorySelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ActivityComponent selectedCategory)
            {
                _viewModel.SelectCategoryCommand.Execute(selectedCategory);
            }

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }
        }

        // 🔥 METODĂ NOUĂ PENTRU BUTONUL EXPAND
        private async void OnExpandButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button?.BindingContext is ActivityCategory category)
                {
                    System.Diagnostics.Debug.WriteLine($"Expand button clicked for: {category.Name}");
                    _viewModel.ToggleExpandCommand.Execute(category);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnExpandButtonClicked: {ex.Message}");
                await DisplayAlert("Error", $"Failed to expand: {ex.Message}", "OK");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.RefreshCommand.Execute(null);
        }
    }
}