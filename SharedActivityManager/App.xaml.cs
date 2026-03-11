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

            MainPage = new NavigationPage(new MainPage())
            {
                BarBackgroundColor = Color.FromArgb("#2196F3"),
                BarTextColor = Colors.White
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in App constructor: {ex.Message}");
        }
    }
}