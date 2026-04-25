using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Commands
{
    public class ResetAllCommand : ICommand
    {
        private readonly IActivityService _activityService;
        private readonly List<Activity> _savedActivities;
        private readonly List<Category> _savedCategories;

        public string Name => "Reset Everything";

        public ResetAllCommand(IActivityService activityService, List<Activity> activities, List<Category> categories)
        {
            _activityService = activityService;
            _savedActivities = DeepCopyActivities(activities);
            _savedCategories = DeepCopyCategories(categories);
        }

        public async Task Execute()
        {
            // Șterge toate activitățile
            var activities = await _activityService.GetActivitiesAsync();
            foreach (var activity in activities)
                await _activityService.DeleteActivityAsync(activity);

            // Șterge toate categoriile
            var categories = await _activityService.GetCategoriesAsync();
            foreach (var category in categories)
                await _activityService.DeleteCategoryAsync(category);

            // Recrează categoriile implicite
            await RecreateDefaultCategories();
        }

        public async Task Undo()
        {
            // Șterge tot ce s-a creat
            var activities = await _activityService.GetActivitiesAsync();
            foreach (var activity in activities)
                await _activityService.DeleteActivityAsync(activity);

            var categories = await _activityService.GetCategoriesAsync();
            foreach (var category in categories)
                await _activityService.DeleteCategoryAsync(category);

            // Restaurează categoriile
            foreach (var category in _savedCategories)
            {
                var newCategory = DeepCopyCategory(category);
                await _activityService.SaveCategoryAsync(newCategory);
            }

            // Restaurează activitățile
            foreach (var activity in _savedActivities)
            {
                var newActivity = activity.DeepCopy();
                newActivity.Id = 0;
                await _activityService.SaveActivityAsync(newActivity);
            }
        }

        public async Task Redo() => await Execute();

        private List<Activity> DeepCopyActivities(List<Activity> source)
            => source.Select(a => a.DeepCopy()).ToList();

        private List<Category> DeepCopyCategories(List<Category> source)
            => source.Select(c => DeepCopyCategory(c)).ToList();

        private Category DeepCopyCategory(Category source)
            => new Category { Name = source.Name, ParentCategoryId = source.ParentCategoryId, DisplayOrder = source.DisplayOrder };

        private async Task RecreateDefaultCategories()
        {
            var defaultCategories = new List<Category>
            {
                new Category { Id = 1, Name = "💼 Work", ParentCategoryId = 0, DisplayOrder = 1 },
                new Category { Id = 2, Name = "🏠 Personal", ParentCategoryId = 0, DisplayOrder = 2 },
                new Category { Id = 3, Name = "💪 Health", ParentCategoryId = 0, DisplayOrder = 3 },
                new Category { Id = 4, Name = "📚 Study", ParentCategoryId = 0, DisplayOrder = 4 }
            };

            foreach (var cat in defaultCategories)
            {
                await _activityService.SaveCategoryAsync(cat);
            }
        }
    }
}