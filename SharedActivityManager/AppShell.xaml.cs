namespace SharedActivityManager
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("MainPage", typeof(MainPage));
            Routing.RegisterRoute("CategoriesPage", typeof(CategoriesPage));
            Routing.RegisterRoute("SharedActivitiesPage", typeof(SharedActivitiesPage));
            Routing.RegisterRoute("SettingsPage", typeof(SettingsPage));
        }
    }
}
