// MauiProgram.cs
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SharedActivityManager;
using SharedActivityManager.Views;
using SharedActivityManager.Repositories;
using SharedActivityManager.Services;
using SharedActivityManager.ViewModels;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Înregistrare servicii
        builder.Services.AddSingleton<IActivityRepository, ActivityRepository>();
        builder.Services.AddSingleton<IActivityService, ActivityService>();
        builder.Services.AddSingleton<IAlertService, AlertService>();
        builder.Services.AddSingleton<IMessagingService, MessagingService>();  // ← Adaugă asta

        // Înregistrare ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<SharedActivitiesViewModel>();

        // Înregistrare pagini
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<SharedActivitiesPage>();
        builder.Services.AddTransient<ActivityModal>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}