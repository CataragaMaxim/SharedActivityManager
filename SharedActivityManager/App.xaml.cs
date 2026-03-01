using SharedActivityManager;

namespace SharedActivityManager;

public partial class App : Application
{
    public App()
    {
        try
        {
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in App constructor: {ex.Message}");
        }
    }
}