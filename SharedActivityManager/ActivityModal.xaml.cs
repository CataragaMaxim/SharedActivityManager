using SharedActivityManager.ViewModels;
using SharedActivityManager.Models;
using SharedActivityManager.Enums;

namespace SharedActivityManager;

public partial class ActivityModal : ContentPage
{
    private MainViewModel _viewModel;
    private Activity _activityToEdit;
    private bool _isEditMode;

    // Constructor pentru activitate NOUĂ
    public ActivityModal(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _activityToEdit = null;
        _isEditMode = false;

        BindingContext = _viewModel;

        _viewModel.IsEditMode = false;
        _viewModel.PageTitle = "Create New Activity";

        Title = "Create New Activity";
        _viewModel.ResetForm();
    }

    // Constructor pentru EDITARE activitate
    public ActivityModal(MainViewModel viewModel, Activity activityToEdit)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _activityToEdit = activityToEdit;
        _isEditMode = true;

        BindingContext = _viewModel;

        _viewModel.IsEditMode = true;
        _viewModel.PageTitle = "Edit Activity";
        _viewModel.SelectedActivity = activityToEdit;

        Title = "Edit Activity";
        LoadActivityData();
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

    // Salvare activitate
    private async void OnSaveActivity(object sender, EventArgs e)
    {
        if (_isEditMode)
        {
            await EditActivity();
        }
        else
        {
            await AddNewActivity();
        }
    }

    // Editare activitate existentă - FĂRĂ PARAMETRU
    private async Task EditActivity()
    {
        if (_activityToEdit == null) return;

        if (string.IsNullOrWhiteSpace(_viewModel.NewTaskTitle))
        {
            await DisplayAlert("Validation", "Title is required", "OK");
            return;
        }

        try
        {
            // Folosește _viewModel pentru toate proprietățile
            _activityToEdit.Title = _viewModel.NewTaskTitle;
            _activityToEdit.Desc = _viewModel.NewTaskDesc ?? string.Empty;
            _activityToEdit.TypeId = _viewModel.SelectedActivityType;
            _activityToEdit.StartDate = _viewModel.SelectedStartDate;
            _activityToEdit.StartTime = _viewModel.SelectedStartDate.Add(_viewModel.SelectedStartTime);
            _activityToEdit.AlarmSet = _viewModel.AlarmSet;
            _activityToEdit.ReminderTypeId = _viewModel.SelectedReminderType;
            _activityToEdit.RingTone = _viewModel.SelectedRingTone ?? "Default Alarm";

            // Salvează în baza de date prin ViewModel
            await _viewModel.SaveActivityToDatabase(_activityToEdit);

            await DisplayAlert("Success", "Activity updated successfully!", "OK");
            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to update activity: {ex.Message}", "OK");
        }
    }

    // Adăugare activitate nouă
    private async Task AddNewActivity()
    {
        if (string.IsNullOrWhiteSpace(_viewModel.NewTaskTitle))
        {
            await DisplayAlert("Validation", "Title is required", "OK");
            return;
        }

        try
        {
            var newActivity = new Activity
            {
                Title = _viewModel.NewTaskTitle,
                Desc = _viewModel.NewTaskDesc ?? string.Empty,
                TypeId = _viewModel.SelectedActivityType,
                StartDate = _viewModel.SelectedStartDate,
                StartTime = _viewModel.SelectedStartDate.Add(_viewModel.SelectedStartTime),
                AlarmSet = _viewModel.AlarmSet,
                isCompleted = false,
                ReminderTypeId = _viewModel.SelectedReminderType,
                RingTone = _viewModel.SelectedRingTone ?? "Default Alarm"
            };

            await _viewModel.AddActivityToDatabase(newActivity);

            await DisplayAlert("Success", "Activity added successfully!", "OK");
            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to add activity: {ex.Message}", "OK");
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

    // Alege ringtone
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

    // Închidere modal
    private async void OnCloseModal(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.IsEditMode = false;
        _viewModel.SelectedActivity = null;
    }
}