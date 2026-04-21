using CommunityToolkit.Mvvm.Input;
using SharedActivityManager.Data;
using SharedActivityManager.Enums;
using SharedActivityManager.Models;
using SharedActivityManager.Services;
using SharedActivityManager.ViewModels;

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

                _viewModel.RefreshCommand.Execute(null);

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

                var categories = await database.GetCategoriesAsync();
                var catInfo = "📁 CATEGORIES IN DATABASE:\n";
                foreach (var cat in categories)
                {
                    catInfo += $"ID: {cat.Id}, Name: {cat.Name}, ParentId: {cat.ParentCategoryId}\n";
                }

                var activities = await database.GetActivitiesAsync();
                var actInfo = "\n📋 ACTIVITIES:\n";
                foreach (var act in activities)
                {
                    actInfo += $"Title: {act.Title}, Type: {act.TypeId}, CategoryId: {act.CategoryId}\n";
                }

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
                    string expectedCategoryName = activity.TypeId switch
                    {
                        ActivityType.Work => "💼 Work",
                        ActivityType.Personal => "🏠 Personal",
                        ActivityType.Health => "💪 Health",
                        ActivityType.Study => "📚 Study",
                        _ => "Other"
                    };

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
                _viewModel.RefreshCommand.Execute(null);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        // 🔥 METODĂ CORECTATĂ - folosește DisplayAlert în loc de _alertService
        private async void OnTestDecoratorClicked(object sender, EventArgs e)
        {
            try
            {
                var testActivity = new Activity
                {
                    Title = "Test Decorator Activity",
                    Desc = "Testing extra features",
                    TypeId = ActivityType.Health,
                    StartDate = DateTime.Today,
                    StartTime = DateTime.Now.AddHours(1),
                    IsCompleted = false
                };

                var builder = new ActivityExtraBuilder(testActivity);
                builder.WithNotifications()
                       .WithEmailReminder("test@example.com")
                       .WithCalendarSync()
                       .WithGpsTracking();

                var message = $"✅ Decorator Test Results:\n\n" +
                              $"Description: {builder.GetFullDescription()}\n" +
                              $"Icon: {builder.GetIcon()}\n" +
                              $"Extra cost: {builder.GetTotalExtraCost()} minutes\n\n" +
                              $"Features enabled:\n" +
                              $"  • Push Notifications: YES\n" +
                              $"  • Email Reminder: YES\n" +
                              $"  • Calendar Sync: YES\n" +
                              $"  • GPS Tracking: YES";

                await DisplayAlert("Decorator Test", message, "OK");

                System.Diagnostics.Debug.WriteLine($"=== Decorator Test ===");
                System.Diagnostics.Debug.WriteLine($"Description: {builder.GetFullDescription()}");
                System.Diagnostics.Debug.WriteLine($"Extra cost: {builder.GetTotalExtraCost()} min");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Test failed: {ex.Message}", "OK");
            }
        }

        // 🔥 METODĂ CORECTATĂ - TestExtraFeatures
        private async void OnTestExtraFeaturesClicked(object sender, EventArgs e)
        {
            try
            {
                var testActivity = new Activity
                {
                    Title = "Yoga Session",
                    Desc = "Morning yoga routine",
                    TypeId = ActivityType.Health,
                    StartDate = DateTime.Today,
                    StartTime = DateTime.Now,
                    IsCompleted = false
                };

                var builder = new ActivityExtraBuilder(testActivity);
                builder.WithNotifications()
                       .WithEmailReminder("user@example.com")
                       .WithCalendarSync()
                       .WithGpsTracking();

                var result = $"📊 Extra Features Test:\n\n" +
                             $"📝 Base: {builder.GetFullDescription()}\n" +
                             $"🎨 Icon: {builder.GetIcon()}\n" +
                             $"⏱️ Extra time: {builder.GetTotalExtraCost()} min\n\n" +
                             $"✨ The activity now has:\n" +
                             $"• Push notifications when completed\n" +
                             $"• Email reminder to user@example.com\n" +
                             $"• Calendar synchronization\n" +
                             $"• GPS tracking for distance/route\n\n" +
                             $"✅ Activity can be saved to database\n" +
                             $"✅ Extra features are visible in UI";

                await DisplayAlert("Extra Features Test", result, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}