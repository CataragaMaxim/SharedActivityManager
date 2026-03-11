// Data/DatabaseConnection.cs
using SharedActivityManager.Models;
using SQLite;

namespace SharedActivityManager.Data
{
    // 🔥 Clasa trebuie să fie 'sealed' pentru a preveni moștenirea
    public sealed class DatabaseConnection
    {
        // 🔥 Câmp static privat pentru instanța unică
        private static DatabaseConnection _instance;

        // 🔥 Obiect pentru lock (thread-safety)
        private static readonly object _lock = new object();

        // 🔥 Conexiunea la baza de date
        private readonly SQLiteAsyncConnection _connection;

        // 🔥 Constructorul PRIVAT - prevenim crearea de instanțe din exterior
        private DatabaseConnection()
        {
            // Inițializare conexiune DB
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "Activity.db");
            _connection = new SQLiteAsyncConnection(dbPath);

            // Creare tabel dacă nu există
            _connection.CreateTableAsync<Activity>().Wait();

            System.Diagnostics.Debug.WriteLine($"[Singleton] DatabaseConnection: Instance created at {DateTime.Now:HH:mm:ss.fff}");
            System.Diagnostics.Debug.WriteLine($"[Singleton] Database path: {dbPath}");
        }

        // 🔥 Metodă statică pentru obținerea instanței unice
        public static DatabaseConnection GetInstance()
        {
            // 🔥 Thread-safe Double-Check Locking
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DatabaseConnection();
                    }
                }
            }
            return _instance;
        }

        // 🔥 Proprietate pentru acces la conexiune
        public SQLiteAsyncConnection Connection => _connection;

        // 🔥 Metodă de business logic - exemplu
        public async Task<int> GetTotalActivitiesCountAsync()
        {
            try
            {
                var count = await _connection.Table<Activity>().CountAsync();
                return count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Singleton] Error getting count: {ex.Message}");
                return 0;
            }
        }

        // 🔥 Metodă pentru a verifica starea conexiunii
        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                // Încercăm o operație simplă pentru a verifica conexiunea
                await _connection.ExecuteScalarAsync<int>("SELECT 1");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}