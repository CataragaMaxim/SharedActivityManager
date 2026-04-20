using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedActivityManager.Data;
using SharedActivityManager.Models;
using SharedActivityManager.Services;

namespace SharedActivityManager.ViewModels
{
    public partial class CategoriesViewModel : ObservableObject
    {
        private readonly IActivityService _activityService;
        private readonly CategoryTreeBuilder _treeBuilder;
        private readonly CategoryExportService _exportService;
        private readonly IAlertService _alertService;

        // Proprietăți principale
        [ObservableProperty]
        private ActivityCategory _rootCategory;

        [ObservableProperty]
        private ObservableCollection<ActivityComponent> _displayCategories = new();

        [ObservableProperty]
        private ActivityComponent _selectedCategory;

        [ObservableProperty]
        private List<ActivityComponent> _searchResults;

        // Statistici
        [ObservableProperty]
        private int _totalActivities;

        [ObservableProperty]
        private int _completedActivities;

        [ObservableProperty]
        private double _completionPercentage;

        [ObservableProperty]
        private bool _isLoading;

        // Creare categorie
        [ObservableProperty]
        private bool _isCreatingCategory;

        [ObservableProperty]
        private string _newCategoryName;

        // Căutare
        [ObservableProperty]
        private string _searchText;

        // Stare expand
        private Dictionary<int, bool> _expandedState = new();

        public CategoriesViewModel()
        {
            var repository = new Repositories.ActivityRepository();
            _activityService = new ActivityService(repository);
            _treeBuilder = new CategoryTreeBuilder(_activityService);
            _exportService = new CategoryExportService(_activityService);
            _alertService = new AlertService();

            LoadCategories();
            LoadExpandedState();
        }

        private async void LoadCategories()
        {
            try
            {
                IsLoading = true;
                RootCategory = await _treeBuilder.BuildFromDatabaseAsync();

                // Afișează structura în debug
                _treeBuilder.DisplayTree(RootCategory);

                // Construiește lista de afișat (doar rădăcinile)
                _displayCategories.Clear();
                foreach (var child in RootCategory.GetChildren())
                {
                    _displayCategories.Add(child);
                }

                UpdateStatistics();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading categories: {ex.Message}");
                await _alertService.ShowAlertAsync("Error", $"Failed to load categories: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateStatistics()
        {
            if (RootCategory != null)
            {
                TotalActivities = RootCategory.GetTotalActivitiesCount();
                CompletedActivities = RootCategory.GetCompletedCount();
                CompletionPercentage = TotalActivities > 0
                    ? (double)CompletedActivities / TotalActivities * 100
                    : 0;
            }
        }

        // ========== COMENZI ==========

        [RelayCommand]
        private void SelectCategory(ActivityComponent category)
        {
            SelectedCategory = category;
            UpdateStatistics();
        }

        [RelayCommand]
        private async Task ViewCategoryDetails(ActivityComponent category)
        {
            if (category == null) return;

            var total = category.GetTotalActivitiesCount();
            var completed = category.GetCompletedCount();
            var percent = total > 0 ? (double)completed / total * 100 : 0;

            // 🔥 CORECTAT: IsComposite este proprietate, nu metodă
            var message = $"📁 {category.Name}\n" +
                         $"├─ Total activități: {total}\n" +
                         $"├─ Finalizate: {completed}\n" +
                         $"├─ Progres: {percent:F0}%\n" +
                         $"├─ Adâncime: {(category.IsComposite ? (category as ActivityCategory)?.GetMaxDepth() ?? 0 : 0)}\n" +
                         $"└─ Tip: {(category.IsComposite ? "Folder" : "Activitate")}";

            await _alertService.ShowAlertAsync("Detalii categorie", message);
        }

        [RelayCommand]
        private async Task Refresh()
        {
            LoadCategories();
        }

        // ========== CREARE CATEGORIE ==========

        [RelayCommand]
        private void ShowCreateCategoryDialog()
        {
            IsCreatingCategory = true;
        }

        [RelayCommand]
        private void CancelCreateCategory()
        {
            IsCreatingCategory = false;
            NewCategoryName = string.Empty;
        }

        [RelayCommand]
        private async Task CreateNewCategory()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName))
            {
                await _alertService.ShowAlertAsync("Error", "Category name is required");
                return;
            }

            try
            {
                int parentId = 0;
                if (SelectedCategory is ActivityCategory selectedCat)
                {
                    parentId = selectedCat.CategoryId;
                }

                await _treeBuilder.CreateCategoryAsync(NewCategoryName, parentId);
                NewCategoryName = string.Empty;
                IsCreatingCategory = false;
                LoadCategories();

                await _alertService.ShowAlertAsync("Success", "Category created!");
            }
            catch (Exception ex)
            {
                await _alertService.ShowAlertAsync("Error", $"Failed: {ex.Message}");
            }
        }

        // ========== ȘTERGERE CATEGORIE ==========

        [RelayCommand]
        private async Task DeleteCategory(ActivityComponent category)
        {
            // 🔥 CORECTAT: IsComposite este proprietate
            if (category == null || !category.IsComposite) return;

            var confirm = await _alertService.ShowConfirmationAsync("Confirm Delete",
                $"Delete '{category.Name}' and all its activities?");

            if (confirm && category is ActivityCategory cat)
            {
                await _treeBuilder.DeleteCategoryAsync(cat.CategoryId);
                LoadCategories();
            }
        }

        // ========== EXPAND/COLLAPSE ==========

        // În CategoriesViewModel.cs - verifică această metodă
        [RelayCommand]
        private void ToggleExpand(ActivityCategory category)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"ToggleExpand called for: {category?.Name}");

                if (category == null)
                {
                    System.Diagnostics.Debug.WriteLine("Category is null");
                    return;
                }

                if (_expandedState.ContainsKey(category.CategoryId))
                {
                    _expandedState[category.CategoryId] = !_expandedState[category.CategoryId];
                    System.Diagnostics.Debug.WriteLine($"Category {category.Name} toggled to: {_expandedState[category.CategoryId]}");
                }
                else
                {
                    _expandedState[category.CategoryId] = true;
                    System.Diagnostics.Debug.WriteLine($"Category {category.Name} set to expanded");
                }

                SaveExpandedState();
                RefreshDisplayTree();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ToggleExpand: {ex.Message}");
            }
        }

        private void RefreshDisplayTree()
        {
            _displayCategories.Clear();
            foreach (var child in RootCategory.GetChildren())
            {
                AddWithExpandedState(child, _displayCategories, 0);
            }
        }

        private void AddWithExpandedState(ActivityComponent component, ObservableCollection<ActivityComponent> collection, int level)
        {
            collection.Add(component);

            // 🔥 CORECTAT: IsComposite este proprietate
            if (component.IsComposite && _expandedState.ContainsKey(component.CategoryId) && _expandedState[component.CategoryId])
            {
                foreach (var child in component.GetChildren())
                {
                    AddWithExpandedState(child, collection, level + 1);
                }
            }
        }

        private void SaveExpandedState()
        {
            var expandedIds = _expandedState.Where(e => e.Value).Select(e => e.Key).ToList();
            var json = System.Text.Json.JsonSerializer.Serialize(expandedIds);
            Preferences.Set("ExpandedCategories", json);
        }

        private void LoadExpandedState()
        {
            var json = Preferences.Get("ExpandedCategories", "[]");
            var expandedIds = System.Text.Json.JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();

            _expandedState.Clear();
            foreach (var id in expandedIds)
            {
                _expandedState[id] = true;
            }
        }

        // ========== CĂUTARE ==========

        [RelayCommand]
        private async Task Search()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                SearchResults = null;
                return;
            }

            var results = new List<ActivityComponent>();
            var searchLower = SearchText.ToLower();

            SearchInTree(RootCategory, searchLower, results);
            SearchResults = results;

            await Task.CompletedTask;
        }

        private void SearchInTree(ActivityComponent component, string searchText, List<ActivityComponent> results)
        {
            if (component.Name.ToLower().Contains(searchText))
                results.Add(component);

            // 🔥 CORECTAT: IsComposite este proprietate
            if (component.IsComposite)
            {
                foreach (var child in component.GetChildren())
                {
                    SearchInTree(child, searchText, results);
                }
            }
        }

        // ========== EXPORT/IMPORT ==========

        [RelayCommand]
        private async Task Export()
        {
            try
            {
                var json = await _exportService.ExportToJsonAsync(RootCategory);

                // Salvează fișierul
                var fileName = $"categories_export_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
                await File.WriteAllTextAsync(filePath, json);

                // Share fișierul
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Export Categories",
                    File = new ShareFile(filePath)
                });

                await _alertService.ShowAlertAsync("Success", $"Exported to {fileName}");
            }
            catch (Exception ex)
            {
                await _alertService.ShowAlertAsync("Error", $"Export failed: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task Import()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select JSON file to import",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".json" } },
                        { DevicePlatform.Android, new[] { "application/json" } }
                    })
                });

                if (result != null)
                {
                    var json = await File.ReadAllTextAsync(result.FullPath);
                    var imported = await _exportService.ImportFromJsonAsync(json);

                    // Reîncarcă structura
                    LoadCategories();

                    await _alertService.ShowAlertAsync("Success", "Categories imported successfully!");
                }
            }
            catch (Exception ex)
            {
                await _alertService.ShowAlertAsync("Error", $"Import failed: {ex.Message}");
            }
        }

        // ========== OPERAȚII RECURSIVE ==========

        [RelayCommand]
        private async Task ShowIncompleteActivities()
        {
            if (RootCategory == null) return;

            var incomplete = RootCategory.GetIncompleteActivities();
            var message = $"📋 Incomplete Activities ({incomplete.Count}):\n";
            message += string.Join("\n", incomplete.Select(a => $"• {a.Title}"));

            await _alertService.ShowAlertAsync("Incomplete Activities", message);
        }

        [RelayCommand]
        private async Task ShowTreeDepth()
        {
            if (RootCategory == null) return;

            var depth = RootCategory.GetMaxDepth();
            await _alertService.ShowAlertAsync("Tree Info", $"Maximum tree depth: {depth}");
        }

        [RelayCommand]
        private async Task MarkAllAsCompleted()
        {
            var confirm = await _alertService.ShowConfirmationAsync("Confirm",
                "Mark ALL activities as completed?");

            if (confirm && RootCategory != null)
            {
                await RootCategory.ForEachAsync(async component =>
                {
                    if (component is ActivityLeaf leaf)
                    {
                        var activity = leaf.GetActivity();
                        if (!activity.IsCompleted)
                        {
                            activity.IsCompleted = true;
                            await _activityService.SaveActivityAsync(activity);
                        }
                    }
                });

                LoadCategories();
                await _alertService.ShowAlertAsync("Success", "All activities marked as completed!");
            }
        }
    }
}