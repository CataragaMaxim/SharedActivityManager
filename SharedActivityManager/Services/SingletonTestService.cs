// Services/SingletonTestService.cs (opțional, pentru debug)
using SharedActivityManager.Data;

namespace SharedActivityManager.Services
{
    public static class SingletonTestService
    {
        public static void TestDatabaseSingleton()
        {
            System.Diagnostics.Debug.WriteLine("\n=== TESTING DATABASE SINGLETON ===");

            // Obținem prima instanță
            var db1 = DatabaseConnection.GetInstance();
            System.Diagnostics.Debug.WriteLine($"Instance 1 created: {db1.GetHashCode()}");

            // Obținem a doua instanță
            var db2 = DatabaseConnection.GetInstance();
            System.Diagnostics.Debug.WriteLine($"Instance 2 created: {db2.GetHashCode()}");

            // Verificăm dacă sunt aceleași
            if (db1 == db2)
            {
                System.Diagnostics.Debug.WriteLine($"✅ SUCCESS: Both instances are the same!");
                System.Diagnostics.Debug.WriteLine($"   Connection objects are identical");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"❌ FAILED: Instances are different!");
            }

            // Test pentru versiunea Lazy
            System.Diagnostics.Debug.WriteLine("\n--- Testing Lazy Singleton ---");
            var lazy1 = DatabaseConnectionLazy.Instance;
            var lazy2 = DatabaseConnectionLazy.Instance;

            System.Diagnostics.Debug.WriteLine($"Lazy Instance 1: {lazy1.GetHashCode()}");
            System.Diagnostics.Debug.WriteLine($"Lazy Instance 2: {lazy2.GetHashCode()}");
            System.Diagnostics.Debug.WriteLine($"Lazy instances are same: {lazy1 == lazy2}");

            System.Diagnostics.Debug.WriteLine("=== END TEST ===\n");
        }
    }
}