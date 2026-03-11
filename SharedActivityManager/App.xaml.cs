// App.xaml.cs
using SharedActivityManager;

namespace SharedActivityManager;

public partial class App : Application
{
    public App()
    {
        try
        {
            InitializeComponent();

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