// Data/ActivityDataBase.cs - VERSIUNEA CORECTĂ
using SharedActivityManager.Abstracts;
using SharedActivityManager.Models;
using SQLite;

namespace SharedActivityManager.Data
{
    public class ActivityDataBase
    {
        private readonly SQLiteAsyncConnection _connection;

        public ActivityDataBase()
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "Activity.db");
            _connection = new SQLiteAsyncConnection(dbPath);
            _connection.CreateTableAsync<Activity>().Wait();
        }

        public Task<List<Activity>> GetActivitiesAsync()
        {
            return _connection.Table<Activity>().ToListAsync();
        }

        // 🔥 FIX: Pentru IActivity
        public async Task<int> SaveActivityAsync(IActivity activity)
        {
            if (activity is Activity act)
            {
                act.AdditionalDataJson = act.SerializeAdditionalData();

                if (act.Id == 0)
                    return await _connection.InsertAsync(act);  // ← era _database
                else
                    return await _connection.UpdateAsync(act);  // ← era _database
            }
            return 0;
        }

        // 🔥 FIX: Pentru Activity direct
        public async Task<int> SaveActivityAsync(Activity activity)
        {
            activity.AdditionalDataJson = activity.SerializeAdditionalData();

            if (activity.Id == 0)
                return await _connection.InsertAsync(activity);
            else
                return await _connection.UpdateAsync(activity);
        }

        public Task<int> DeleteActivityAsync(Activity activity)
        {
            return _connection.DeleteAsync(activity);
        }
    }
}