using SharedActivityManager.Data;
using SharedActivityManager.Models;

namespace SharedActivityManager.Models
{
    public class ActivityLeaf : ActivityComponent
    {
        private readonly Activity _activity;

        public ActivityLeaf(Activity activity)
        {
            _activity = activity;
        }

        public override string Id => _activity.Id.ToString();
        public override string Name => _activity.Title;
        public override string Description => _activity.Desc ?? string.Empty;
        public override int CategoryId => _activity.CategoryId;

        // 🔥 PROPRIETATE NOUĂ
        public override bool IsComposite => false;

        public override int GetTotalActivitiesCount() => 1;
        public override int GetCompletedCount() => _activity.IsCompleted ? 1 : 0;

        public override List<Activity> GetAllActivities()
        {
            return new List<Activity> { _activity };
        }

        public override async Task SaveAsync(ActivityDataBase database)
        {
            await database.SaveActivityAsync(_activity);
        }

        public Activity GetActivity() => _activity;
    }
}