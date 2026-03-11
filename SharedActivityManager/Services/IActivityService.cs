// Services/IActivityService.cs
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Models;

namespace SharedActivityManager.Services
{
    public interface IActivityService
    {
        Task<List<Activity>> GetActivitiesAsync();
        Task<Activity> GetActivityByIdAsync(int id);
        Task SaveActivityAsync(Activity activity);
        Task DeleteActivityAsync(Activity activity);
        Task SaveActivityWithAlarmAsync(Activity activity, IAlarmService alarmService);
        Task<List<Activity>> GetSharedActivitiesAsync(string currentUserId);
        Task<Activity> CopySharedActivityAsync(Activity sourceActivity, string newOwnerId);
    }
}