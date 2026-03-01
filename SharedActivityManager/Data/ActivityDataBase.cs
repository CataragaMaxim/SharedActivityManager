using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public Task<int> SaveActivityAsync(Activity activity)
        {
            if (activity.Id != 0)
            {
                return _connection.UpdateAsync(activity);
            }
            else
            {
                return _connection.InsertAsync(activity);
            }
        }

        public Task<int> DeleteActivityAsync(Activity activity)
        {
            return _connection.DeleteAsync(activity);
        }
    }
}
