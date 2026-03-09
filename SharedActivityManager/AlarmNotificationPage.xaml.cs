using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Services;

namespace SharedActivityManager;

public partial class AlarmNotificationPage : ContentPage
{
    private string _ringtone;
    private IAudioService _audioService;
    private IAlarmService _alarmService; // ← Adaugă pentru a opri alarma

    public AlarmNotificationPage(string title, string description, string ringtone = "Default")
    {
        InitializeComponent();
        _ringtone = ringtone;
        _audioService = PlatformServiceLocator.AudioService;
        _alarmService = PlatformServiceLocator.AlarmService;

        TitleLabel.Text = title;
        DescriptionLabel.Text = description ?? "Time for your activity!";
        TimeLabel.Text = DateTime.Now.ToString("HH:mm:ss");

        // Rulează sunetul după ce pagina este încărcată
        this.Loaded += async (s, e) => await PlayAlarmSound();
    }

    private async Task PlayAlarmSound()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"AlarmNotificationPage: Playing sound {_ringtone}");
            await _audioService.PlayRingtoneAsync(_ringtone);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error playing sound: {ex.Message}");
        }
    }

    // 🔥 MODIFICAT: Acum oprește și alarma, nu doar sunetul
    private async void OnStopClicked(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("AlarmNotificationPage: Stopping alarm");

            // Folosește PlatformServiceLocator pentru a obține serviciul corect
            var alarmService = PlatformServiceLocator.AlarmService;
            await alarmService.StopCurrentAlarmAsync();

            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error stopping alarm: {ex.Message}");
            await Navigation.PopModalAsync();
        }
    }

    private async void OnSnoozeClicked(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("AlarmNotificationPage: Snoozing alarm");

            // Oprește sunetul
            await _audioService.StopPlayingAsync();

            // Aici poți implementa logica de snooze
            // De exemplu, reprogramează alarma peste 5 minute
            await Task.Delay(300000); // 5 minute în milisecunde

            await DisplayAlert("Snooze", "Snooze time is over!", "OK");

            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error snoozing alarm: {ex.Message}");
            await Navigation.PopModalAsync();
        }
    }

    // 🔥 Asigură-te că sunetul se oprește când pagina este închisă
    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        try
        {
            // Oprește sunetul când pagina este închisă
            if (_audioService.IsPlaying)
            {
                await _audioService.StopPlayingAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error stopping sound on disappearing: {ex.Message}");
        }
    }
}