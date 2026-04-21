// App.xaml.cs
using SharedActivityManager;
using SharedActivityManager.Services;

namespace SharedActivityManager;

public partial class App : Application
{
    public App()
    {
        try
        {
            InitializeComponent();

            SingletonTestService.TestDatabaseSingleton();

            MainPage = new AppShell();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in App constructor: {ex.Message}");
        }
    }
}