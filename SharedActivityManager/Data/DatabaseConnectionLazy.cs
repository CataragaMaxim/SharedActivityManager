// Data/DatabaseConnectionLazy.cs
using SharedActivityManager.Data;
using SharedActivityManager.Models;
using SQLite;

namespace SharedActivityManager.Data
{
    public sealed class DatabaseConnectionLazy
    {
        // 🔥 Folosim Lazy<T> pentru inițializare întârziată și thread-safe
        private static readonly Lazy<DatabaseConnectionLazy> _lazyInstance =
            new Lazy<DatabaseConnectionLazy>(() => new DatabaseConnectionLazy());

        private readonly SQLiteAsyncConnection _connection;

        private DatabaseConnectionLazy()
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "Activity.db");
            _connection = new SQLiteAsyncConnection(dbPath);
            _connection.CreateTableAsync<Activity>().Wait();

            System.Diagnostics.Debug.WriteLine($"[Singleton Lazy] DatabaseConnection created at {DateTime.Now:HH:mm:ss.fff}");
        }

        // 🔥 Proprietate statică pentru acces la instanță
        public static DatabaseConnectionLazy Instance => _lazyInstance.Value;

        public SQLiteAsyncConnection Connection => _connection;

        // Metode de business logic
        public async Task<int> GetTotalActivitiesCountAsync()
        {
            return await _connection.Table<Activity>().CountAsync();
        }

        public async Task DeleteAllActivitiesAsync()
        {
            await _connection.DeleteAllAsync<Activity>();
        }
    }
}