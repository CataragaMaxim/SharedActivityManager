using SharedActivityManager.ViewModels;
using SharedActivityManager.Models;

namespace SharedActivityManager;

public partial class ActivityModal : ContentPage
{
    private MainViewModel _viewModel;
    private Activity _activityToEdit;

    // Constructor pentru activitate NOUĂ
    public ActivityModal(MainViewModel viewModel)
    {
        try
        {
            InitializeComponent();
            _viewModel = viewModel;
            _activityToEdit = null;

            BindingContext = _viewModel;

            Title = "Create New Activity";
            _viewModel.ResetForm();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in ActivityModal constructor (new): {ex.Message}");
        }
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
        _viewModel.SelectedReminderType = _activityToEdit.ReminderTypeId;
        _viewModel.SelectedRingTone = _activityToEdit.RingTone ?? "Default Alarm";
    }

    // ===== METODE PENTRU BUTOANE =====

    // Închidere modal
    private async void OnCloseModal(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    // Salvare activitate
    private async void OnSaveActivity(object sender, EventArgs e)
    {
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
            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save: {ex.Message}", "OK");
        }
    }

    // Ștergere activitate
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

    // ===== METODA PENTRU RINGTONE PICKER (ASTA LIPSEA) =====
    private async void OnChooseRingtoneClicked(object sender, EventArgs e)
    {
        try
        {
            bool wasEditMode = _viewModel.IsEditMode;

            var ringtonePicker = new RingtonePickerPage();

            ringtonePicker.SetRingtoneSelectedCallback((selectedRingtone) =>
            {
                _viewModel.IsEditMode = wasEditMode;
                _viewModel.SelectedRingTone = selectedRingtone.DisplayName;

                System.Diagnostics.Debug.WriteLine($"Ringtone selected. IsEditMode: {_viewModel.IsEditMode}");
            });

            await Navigation.PushModalAsync(ringtonePicker);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to open ringtone picker: {ex.Message}", "OK");
        }
    }
}