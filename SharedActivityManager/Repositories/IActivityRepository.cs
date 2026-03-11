// Repositories/IActivityRepository.cs
using SharedActivityManager.Models;

namespace SharedActivityManager.Repositories
{
    public interface IActivityRepository
    {
        Task<List<Activity>> GetActivitiesAsync();
        Task<Activity> GetActivityByIdAsync(int id);
        Task<int> SaveActivityAsync(Activity activity);
        Task<int> DeleteActivityAsync(Activity activity);
        Task<List<Activity>> GetPublicActivitiesAsync(string excludeUserId);
    }
}