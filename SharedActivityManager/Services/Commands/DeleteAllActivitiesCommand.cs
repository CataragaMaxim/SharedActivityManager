using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Commands
{
    public class DeleteAllActivitiesCommand : ICommand
    {
        private readonly IActivityService _activityService;
        private readonly List<Activity> _deletedActivities;

        public string Name => "Delete All Activities";

        public DeleteAllActivitiesCommand(IActivityService activityService, List<Activity> activities)
        {
            _activityService = activityService;
            _deletedActivities = DeepCopyList(activities);
        }

        public async Task Execute()
        {
            var activities = await _activityService.GetActivitiesAsync();
            foreach (var activity in activities)
            {
                await _activityService.DeleteActivityAsync(activity);
            }
        }

        public async Task Undo()
        {
            foreach (var activity in _deletedActivities)
            {
                var newActivity = activity.DeepCopy();
                newActivity.Id = 0;
                await _activityService.SaveActivityAsync(newActivity);
            }
        }

        public async Task Redo() => await Execute();

        private List<Activity> DeepCopyList(List<Activity> source)
        {
            return source.Select(a => a.DeepCopy()).ToList();
        }
    }
}