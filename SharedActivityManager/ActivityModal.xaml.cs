// ActivityModal.xaml.cs
using SharedActivityManager.ViewModels;
using SharedActivityManager.Models;
using SharedActivityManager.Services;

namespace SharedActivityManager;

public partial class ActivityModal : ContentPage
{
    private MainViewModel _viewModel;
    private Activity _activityToEdit;

    // Constructor pentru activitate NOUĂ
    public ActivityModal(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _activityToEdit = null;

        BindingContext = _viewModel;

        _viewModel.IsEditMode = false;
        _viewModel.PageTitle = "Create New Activity";
        _viewModel.ResetForm();

        Title = "Create New Activity";
    }

    // Constructor pentru EDITARE activitate
    public ActivityModal(MainViewModel viewModel, Activity activityToEdit)
    {
        try
        {
            InitializeComponent();
            _viewModel = viewModel;
            _activityToEdit = activityToEdit;

            BindingContext = _viewModel;

            _viewModel.IsEditMode = true;
            _viewModel.PageTitle = "Edit Activity";
            _viewModel.SelectedActivity = activityToEdit;

            Title = "Edit Activity";
            LoadActivityData();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in ActivityModal constructor (edit): {ex.Message}");
        }
    }

    private void LoadActivityData()
    {
        if (_activityToEdit == null) return;

        _viewModel.NewTaskTitle = _activityToEdit.Title;
        _viewModel.NewTaskDesc = _activityToEdit.Desc;
        _viewModel.SelectedActivityType = _activityToEdit.TypeId;
        _viewModel.SelectedStartDate = _activityToEdit.StartDate.Date;
        _viewModel.SelectedStartTime = _activityToEdit.StartTime.TimeOfDay;
        _viewModel.AlarmSet = _activityToEdit.AlarmSet;
        _viewModel.SelectedReminderType = _activityToEdit.ReminderType;
        _viewModel.SelectedRingTone = _activityToEdit.RingTone ?? "Default Alarm";
        _viewModel.IsPublic = _activityToEdit.IsPublic;  // ← ACEASTA LIPSEA!

        System.Diagnostics.Debug.WriteLine($"LoadActivityData - IsPublic: {_activityToEdit.IsPublic}");
    }

    // ===== METODE PENTRU BUTOANE =====
    private async void OnCloseModal(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnSaveActivity(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"OnSaveActivity - IsEditMode: {_viewModel.IsEditMode}");

        try
        {
            if (_activityToEdit != null)
            {
                await _viewModel.SaveEditedActivity();
            }
            else
            {
                await _viewModel.AddNewActivity();
            }

            // 🔥 DECLANȘĂ EVENIMENTUL
            AppEvents.OnActivitiesChanged();

            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving: {ex.Message}");
            await DisplayAlert("Error", $"Failed to save: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteActivity(object sender, EventArgs e)
    {
        if (_activityToEdit == null) return;

        var confirm = await DisplayAlert(
            "Confirm Delete",
            $"Are you sure you want to delete '{_activityToEdit.Title}'?",
            "Yes", "No");

        if (confirm)
        {
            await _viewModel.DeleteActivity(_activityToEdit);
            await Navigation.PopModalAsync();
        }
    }

    private async void OnChooseRingtoneClicked(object sender, EventArgs e)
    {
        try
        {
            bool wasEditMode = _viewModel.IsEditMode;
            bool wasPublic = _viewModel.IsPublic;  // Salvăm starea

            var ringtonePicker = new RingtonePickerPage();

            ringtonePicker.SetRingtoneSelectedCallback((selectedRingtone) =>
            {
                _viewModel.IsEditMode = wasEditMode;
                _viewModel.IsPublic = wasPublic;  // Restaurăm starea
                _viewModel.SelectedRingTone = selectedRingtone.DisplayName;

                System.Diagnostics.Debug.WriteLine($"Ringtone selected. IsEditMode: {_viewModel.IsEditMode}, IsPublic: {_viewModel.IsPublic}");
            });

            await Navigation.PushModalAsync(ringtonePicker);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to open ringtone picker: {ex.Message}", "OK");
        }
    }
}