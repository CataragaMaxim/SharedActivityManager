// MauiProgram.cs
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using SharedActivityManager.Repositories;
using SharedActivityManager.Services;
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.ViewModels;

namespace SharedActivityManager;

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

        // Înregistrare servicii existente
        builder.Services.AddSingleton<IActivityRepository, ActivityRepository>();
        builder.Services.AddSingleton<IActivityService, ActivityService>();
        builder.Services.AddSingleton<IAlertService, AlertService>();
        builder.Services.AddSingleton<IMessagingService, MessagingService>();

        // Înregistrare servicii platformă (singleton)
        builder.Services.AddSingleton<IAlarmService>(_ => PlatformServiceLocator.AlarmService);
        builder.Services.AddSingleton<IAudioService>(_ => PlatformServiceLocator.AudioService);
        builder.Services.AddSingleton<INotificationService>(_ => PlatformServiceLocator.NotificationService);

        // 🔥 Înregistrare FACADE
        builder.Services.AddSingleton<IActivityManagementFacade, ActivityManagementFacade>();

        // Înregistrare ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<SharedActivitiesViewModel>();
        builder.Services.AddTransient<CategoriesViewModel>();

        // Înregistrare pagini
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<SharedActivitiesPage>();
        builder.Services.AddTransient<ActivityModal>();
        builder.Services.AddTransient<CategoriesPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}