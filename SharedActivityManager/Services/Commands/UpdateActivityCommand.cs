using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Commands
{
    public class UpdateActivityCommand : ICommand
    {
        private readonly IActivityService _activityService;
        private readonly Activity _oldActivity;
        private readonly Activity _newActivity;
        private readonly int _activityId;

        public string Name => $"Update Activity: {_newActivity.Title}";

        public UpdateActivityCommand(IActivityService activityService, Activity oldActivity, Activity newActivity)
        {
            _activityService = activityService;

            // 🔥 PĂSTREAZĂ ID-UL ORIGINAL
            _activityId = newActivity.Id;

            // Creează copii DEEP cu același ID
            _oldActivity = DeepCopyWithId(oldActivity, _activityId);
            _newActivity = DeepCopyWithId(newActivity, _activityId);

            System.Diagnostics.Debug.WriteLine($"[UpdateCommand] Created - Activity ID: {_activityId}");
        }

        public async Task Execute()
        {
            System.Diagnostics.Debug.WriteLine($"[Command] Executing Update for ID: {_activityId}");

            // 🔥 ASIGURĂ-TE CĂ ID-UL ESTE CORECT
            _newActivity.Id = _activityId;
            await _activityService.SaveActivityAsync(_newActivity);
        }

        public async Task Undo()
        {
            System.Diagnostics.Debug.WriteLine($"[Command] Undo Update for ID: {_activityId}");

            // 🔥 REVINE LA STAREA VECHE CU ACELAȘI ID
            _oldActivity.Id = _activityId;
            await _activityService.SaveActivityAsync(_oldActivity);
        }

        public async Task Redo()
        {
            System.Diagnostics.Debug.WriteLine($"[Command] Redo Update for ID: {_activityId}");

            // 🔥 REAPLICĂ STAREA NOUĂ CU ACELAȘI ID
            _newActivity.Id = _activityId;
            await _activityService.SaveActivityAsync(_newActivity);
        }

        /// <summary>
        /// Creează o copie profundă PĂSTRÂND ID-UL
        /// </summary>
        private Activity DeepCopyWithId(Activity source, int id)
        {
            if (source == null) return null;

            var copy = new Activity
            {
                Id = id,  // 🔥 FOLOSEȘTE ID-UL PRIMIT, NU CEL DIN SURSĂ
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
                SpecificDataJson = source.SpecificDataJson,
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