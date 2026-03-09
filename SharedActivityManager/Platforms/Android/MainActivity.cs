using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using SharedActivityManager.Abstracts.Platforms; // ← Adaugă asta
using static SharedActivityManager.Platforms.Android.Services.AndroidAlarmService;

namespace SharedActivityManager;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
    ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize |
    ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Verifică permisiunile pentru alarme exacte
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
        {
            var alarmManager = (AlarmManager)GetSystemService(AlarmService);
            if (!alarmManager.CanScheduleExactAlarms())
            {
                var intent = new Intent(Settings.ActionRequestScheduleExactAlarm);
                intent.SetData(Android.Net.Uri.Parse("package:" + PackageName));
                StartActivity(intent);
            }
        }

        // 🔥 IMPORTANT: Procesează intent-ul care a deschis activitatea
        HandleIntent(Intent);
    }

    protected override void OnNewIntent(Intent intent)
    {
        base.OnNewIntent(intent);
        // 🔥 IMPORTANT: Procesează intent-ul nou (când activitatea era deja deschisă)
        HandleIntent(intent);
    }

    private void HandleIntent(Intent intent)
    {
        try
        {
            if (intent != null && intent.Extras != null)
            {
                // Verifică dacă intent-ul vine de la o notificare de alarmă
                if (intent.Extras.ContainsKey("show_alarm") && intent.GetBooleanExtra("show_alarm", false))
                {
                    System.Diagnostics.Debug.WriteLine("MainActivity: Handling alarm notification intent");

                    // Extrage datele alarmei
                    var activityId = intent.GetIntExtra("activity_id", -1);
                    var activityTitle = intent.GetStringExtra("activity_title");
                    var activityDesc = intent.GetStringExtra("activity_desc");
                    var activityRingtone = intent.GetStringExtra("activity_ringtone");

                    System.Diagnostics.Debug.WriteLine($"MainActivity: Alarm data - ID: {activityId}, Title: {activityTitle}");

                    // Rulează pe UI thread-ul principal
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await ShowAlarmPage(activityId, activityTitle, activityDesc, activityRingtone);
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MainActivity: Error handling intent: {ex.Message}");
        }
    }

    private async Task ShowAlarmPage(int activityId, string title, string desc, string ringtone)
    {
        try
        {
            // Verifică dacă există deja o pagină de alarmă deschisă
            var currentPage = Microsoft.Maui.Controls.Application.Current?.MainPage;
            if (currentPage != null)
            {
                // Verifică dacă pagina curentă este deja AlarmNotificationPage
                if (currentPage is AlarmNotificationPage)
                {
                    System.Diagnostics.Debug.WriteLine("Alarm page already open");
                    return;
                }

                // Verifică dacă există în stiva de modale
                var modalPage = currentPage.Navigation.ModalStack
                    .FirstOrDefault(p => p is AlarmNotificationPage);

                if (modalPage != null)
                {
                    System.Diagnostics.Debug.WriteLine("Alarm modal already open");
                    return;
                }
            }

            // Creează pagina de alarmă
            var alarmPage = new AlarmNotificationPage(title, desc, ringtone);

            // 🔥 Setează binding context pentru a putea opri alarma
            alarmPage.BindingContext = new
            {
                ActivityId = activityId,
                Title = title,
                Description = desc,
                Ringtone = ringtone
            };

            // Afișează pagina
            if (Microsoft.Maui.Controls.Application.Current?.MainPage != null)
            {
                await Microsoft.Maui.Controls.Application.Current.MainPage.Navigation.PushModalAsync(alarmPage);
            }
            else
            {
                Microsoft.Maui.Controls.Application.Current.MainPage = alarmPage;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error showing alarm page: {ex.Message}");
        }
    }
}