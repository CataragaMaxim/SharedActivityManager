using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Commands
{
    /// <summary>
    /// Comandă pentru crearea unei activități noi
    /// </summary>
    public class CreateActivityCommand : ICommand
    {
        private readonly IActivityService _activityService;
        private readonly Activity _activity;
        private int _savedId;

        public string Name => $"Create Activity: {_activity.Title}";

        public CreateActivityCommand(IActivityService activityService, Activity activity)
        {
            _activityService = activityService;
            _activity = activity;
            _savedId = 0;
        }

        public async Task Execute()
        {
            System.Diagnostics.Debug.WriteLine($"[Command] Executing Create: {_activity.Title}");

            // Salvează activitatea
            await _activityService.SaveActivityAsync(_activity);
            _savedId = _activity.Id;

            System.Diagnostics.Debug.WriteLine($"[Command] Activity created with ID: {_savedId}");
        }

        public async Task Undo()
        {
            System.Diagnostics.Debug.WriteLine($"[Command] Undo Create: {_activity.Title}");

            // Șterge activitatea creată
            if (_savedId > 0)
            {
                _activity.Id = _savedId;
                await _activityService.DeleteActivityAsync(_activity);
            }
        }

        public async Task Redo()
        {
            System.Diagnostics.Debug.WriteLine($"[Command] Redo Create: {_activity.Title}");

            // Re-creează activitatea
            if (_savedId > 0)
            {
                _activity.Id = 0; // Reset ID pentru a crea una nouă
                await _activityService.SaveActivityAsync(_activity);
                _savedId = _activity.Id;
            }
            else
            {
                await Execute();
            }
        }
    }
}