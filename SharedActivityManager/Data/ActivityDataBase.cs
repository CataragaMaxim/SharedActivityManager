// Data/ActivityDataBase.cs
using SharedActivityManager.Models;
using SQLite;

namespace SharedActivityManager.Data
{
    public class ActivityDataBase
    {
        private readonly SQLiteAsyncConnection _connection;

        public ActivityDataBase()
        {
            // 🔥 Obținem instanța unică a DatabaseConnection
            var dbConnection = DatabaseConnection.GetInstance();
            _connection = DatabaseConnectionLazy.Instance.Connection;

            System.Diagnostics.Debug.WriteLine($"[ActivityDataBase] Using singleton database connection");
        }

        public Task<List<Activity>> GetActivitiesAsync()
        {
            return _connection.Table<Activity>().ToListAsync();
        }

        public async Task<int> SaveActivityAsync(Activity activity)
        {
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