using SharedActivityManager.ViewModels;
using SharedActivityManager.Models;
using SharedActivityManager.Data;
using SharedActivityManager.Enums;
using SharedActivityManager.Services;

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
            System.Diagnostics.Debug.WriteLine("CategoriesPage: OnAppearing - forcing refresh");

            // 🔥 FORȚEAZĂ REFRESH COMPLET
            _viewModel.RefreshCommand.Execute(null);
        }

        private async void OnHardResetClicked(object sender, EventArgs e)
        {
            try
            {
                var confirm = await DisplayAlert("Hard Reset",
                    "This will force reload all data from database.\n\nContinue?",
                    "Yes", "No");

                if (!confirm) return;

                // Forțează reîncărcarea completă
                _viewModel.RefreshCommand.Execute(null);

                // Forțează reîncărcarea și în MainPage prin mesaj
                var messagingService = new MessagingService();
                messagingService.Send(new ActivitiesChangedMessage { Action = "HardReset" });

                await DisplayAlert("Success", "Data reloaded!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnDiagnoseClicked(object sender, EventArgs e)
        {
            try
            {
                var database = new ActivityDataBase();

                // 1. Verifică categoriile din baza de date
                var categories = await database.GetCategoriesAsync();
                var catInfo = "📁 CATEGORIES IN DATABASE:\n";
                foreach (var cat in categories)
                {
                    catInfo += $"ID: {cat.Id}, Name: {cat.Name}, ParentId: {cat.ParentCategoryId}\n";
                }

                // 2. Verifică activitățile și CategoryId-ul lor
                var activities = await database.GetActivitiesAsync();
                var actInfo = "\n📋 ACTIVITIES:\n";
                foreach (var act in activities)
                {
                    actInfo += $"Title: {act.Title}, Type: {act.TypeId}, CategoryId: {act.CategoryId}\n";
                }

                // 3. Verifică potrivirea
                var mismatch = "\n⚠️ ACTIVITIES WITH MISMATCHED CATEGORY:\n";
                var hasMismatch = false;
                foreach (var act in activities)
                {
                    if (!categories.Any(c => c.Id == act.CategoryId))
                    {
                        mismatch += $"• '{act.Title}' has CategoryId={act.CategoryId} but no category with this ID exists!\n";
                        hasMismatch = true;
                    }
                }

                if (!hasMismatch) mismatch += "None - all activities have valid categories!\n";

                var fullMessage = catInfo + actInfo + mismatch;

                // Limitează lungimea mesajului
                if (fullMessage.Length > 3000)
                    fullMessage = fullMessage.Substring(0, 3000) + "\n...(truncated)";

                await DisplayAlert("Diagnostic Report", fullMessage, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnFixCategoriesClicked(object sender, EventArgs e)
        {
            try
            {
                var confirm = await DisplayAlert("Fix Categories",
                    "This will reassign all activities to correct categories based on their TypeId.\n\nContinue?",
                    "Yes", "No");

                if (!confirm) return;

                var database = new ActivityDataBase();
                var activities = await database.GetActivitiesAsync();
                var categories = await database.GetCategoriesAsync();

                int fixedCount = 0;

                foreach (var activity in activities)
                {
                    // Determină numele categoriei corecte în funcție de tip
                    string expectedCategoryName = activity.TypeId switch
                    {
                        ActivityType.Work => "💼 Work",
                        ActivityType.Personal => "🏠 Personal",
                        ActivityType.Health => "💪 Health",
                        ActivityType.Study => "📚 Study",
                        _ => "Other"
                    };

                    // Găsește categoria corectă
                    var correctCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);

                    if (correctCategory != null && activity.CategoryId != correctCategory.Id)
                    {
                        activity.CategoryId = correctCategory.Id;
                        await database.SaveActivityAsync(activity);
                        fixedCount++;
                        System.Diagnostics.Debug.WriteLine($"Fixed: {activity.Title} → {expectedCategoryName}");
                    }
                }

                await DisplayAlert("Success", $"Fixed {fixedCount} activities!", "OK");

                // Reîncarcă pagina
                _viewModel.RefreshCommand.Execute(null);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}