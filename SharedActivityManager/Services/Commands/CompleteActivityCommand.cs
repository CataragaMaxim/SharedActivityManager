using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Commands
{
    /// <summary>
    /// Comandă pentru marcarea unei activități ca completată/necompletată
    /// </summary>
    public class CompleteActivityCommand : ICommand
    {
        private readonly IActivityService _activityService;
        private readonly Activity _activity;
        private readonly bool _oldStatus;
        private readonly bool _newStatus;

        public string Name => $"{(_newStatus ? "Complete" : "Incomplete")}: {_activity.Title}";

        public CompleteActivityCommand(IActivityService activityService, Activity activity, bool newStatus)
        {
            _activityService = activityService;
            _activity = activity;
            _oldStatus = activity.IsCompleted;
            _newStatus = newStatus;
        }

        public async Task Execute()
        {
            System.Diagnostics.Debug.WriteLine($"[Command] Executing Complete: {_activity.Title} -> {_newStatus}");

            _activity.IsCompleted = _newStatus;
            await _activityService.SaveActivityAsync(_activity);
        }

        public async Task Undo()
        {
            System.Diagnostics.Debug.WriteLine($"[Command] Undo Complete: {_activity.Title} -> {_oldStatus}");

            _activity.IsCompleted = _oldStatus;
            await _activityService.SaveActivityAsync(_activity);
        }

        public async Task Redo()
        {
            System.Diagnostics.Debug.WriteLine($"[Command] Redo Complete: {_activity.Title} -> {_newStatus}");

            _activity.IsCompleted = _newStatus;
            await _activityService.SaveActivityAsync(_activity);
        }
    }
}