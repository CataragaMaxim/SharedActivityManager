using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Commands
{
    /// <summary>
    /// Comandă pentru ștergerea unei activități
    /// </summary>
    public class DeleteActivityCommand : ICommand
    {
        private readonly IActivityService _activityService;
        private readonly Activity _deletedActivity;
        private bool _isDeleted;

        public string Name => $"Delete Activity: {_deletedActivity?.Title ?? "Unknown"}";

        public DeleteActivityCommand(IActivityService activityService, Activity activity)
        {
            _activityService = activityService;
            _deletedActivity = DeepCopy(activity);
            _isDeleted = false;
        }

        public async Task Execute()
        {
            System.Diagnostics.Debug.WriteLine($"[Command] Executing Delete: {_deletedActivity.Title}");

            if (!_isDeleted)
            {
                await _activityService.DeleteActivityAsync(_deletedActivity);
                _isDeleted = true;
            }
        }

        public async Task Undo()
        {
            System.Diagnostics.Debug.WriteLine($"[Command] Undo Delete: {_deletedActivity.Title}");

            if (_isDeleted)
            {
                // Re-creează activitatea ștearsă
                var restoredActivity = DeepCopy(_deletedActivity);
                restoredActivity.Id = 0; // Reset ID pentru a crea una nouă
                await _activityService.SaveActivityAsync(restoredActivity);
                _isDeleted = false;
            }
        }

        public async Task Redo()
        {
            System.Diagnostics.Debug.WriteLine($"[Command] Redo Delete: {_deletedActivity.Title}");

            if (!_isDeleted)
            {
                await _activityService.DeleteActivityAsync(_deletedActivity);
                _isDeleted = true;
            }
        }

        private Activity DeepCopy(Activity source)
        {
            if (source == null) return null;

            var copy = new Activity
            {
                Id = source.Id,
                Title = source.Title,
                Desc = source.Desc,
                TypeId = source.TypeId,
                StartDate = source.StartDate,
                StartTime = source.StartTime,
                AlarmSet = source.AlarmSet,
                IsCompleted = source.IsCompleted,
                ReminderType = source.ReminderType,
                NextReminderDate = source.NextReminderDate,
                RingTone = source.RingTone,
                IsPublic = source.IsPublic,
                OwnerId = source.OwnerId,
                SharedDate = source.SharedDate,
                OriginalActivityId = source.OriginalActivityId,
                CategoryId = source.CategoryId,
                SpecificDataJson = source.SpecificDataJson, // 🔥 PĂSTREAZĂ JSON-UL
                VideoUrl = source.VideoUrl,
                VideoProgress = source.VideoProgress,
                Notes = source.Notes,
                TimerDurationSeconds = source.TimerDurationSeconds,
                TimerElapsedSeconds = source.TimerElapsedSeconds,
                IsTimerRunning = source.IsTimerRunning,
                ShoppingItemsJson = source.ShoppingItemsJson,
                Budget = source.Budget,
                Store = source.Store,
                Priority = source.Priority,
                ProjectName = source.ProjectName,
                Deadline = source.Deadline
            };

            return copy;
        }
    }
}