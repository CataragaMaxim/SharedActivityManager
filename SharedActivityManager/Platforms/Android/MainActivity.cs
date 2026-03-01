// Platforms/Android/MainActivity.cs
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace SharedActivityManager;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
    ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize |
    ConfigChanges.Density)]
[IntentFilter(new[] { "SHARED_ACTIVITY_MANAGER_ALARM" })]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Verifică dacă a fost pornită de o alarmă
        if (Intent?.Action == "SHARED_ACTIVITY_MANAGER_ALARM")
        {
            HandleAlarmIntent(Intent);
        }
    }

    protected override void OnNewIntent(Intent intent)
    {
        base.OnNewIntent(intent);

        if (intent?.Action == "SHARED_ACTIVITY_MANAGER_ALARM")
        {
            HandleAlarmIntent(intent);
        }
    }

    private void HandleAlarmIntent(Intent intent)
    {
        var activityId = intent.GetIntExtra("activity_id", -1);
        var activityTitle = intent.GetStringExtra("activity_title");
        var activityDesc = intent.GetStringExtra("activity_desc");
        var activityRingtone = intent.GetStringExtra("activity_ringtone");

        // Deschide pagina de alarmă
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var alarmPage = new AlarmNotificationPage(activityTitle, activityDesc, activityRingtone);
            await Microsoft.Maui.Controls.Application.Current?.MainPage.Navigation.PushModalAsync(alarmPage);
        });
    }
}