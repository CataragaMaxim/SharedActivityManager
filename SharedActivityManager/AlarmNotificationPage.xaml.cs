using SharedActivityManager.Services;
using Microsoft.Maui.Controls;

namespace SharedActivityManager;

public partial class AlarmNotificationPage : ContentPage
{
    private string _ringtone;
    private IRingtoneService _ringtoneService;

    public AlarmNotificationPage(string title, string description, string ringtone = "Default")
    {
        InitializeComponent();
        _ringtone = ringtone;
        _ringtoneService = new RingtoneService();

        TitleLabel.Text = title;
        DescriptionLabel.Text = description ?? "Time for your activity!";

        PlayAlarmSound();
    }

    private async void PlayAlarmSound()
    {
        try
        {
            await _ringtoneService.PlayRingtoneAsync(_ringtone);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error playing sound: {ex.Message}");
        }
    }

    private async void OnStopClicked(object sender, EventArgs e)
    {
        try
        {
            await _ringtoneService.StopPlayingAsync();
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
            await _ringtoneService.StopPlayingAsync();
            await DisplayAlert("Snooze", "Snooze for 5 minutes", "OK");
            await Navigation.PopModalAsync();

            // Aici poți implementa logica de snooze
            // De exemplu, programează o nouă alarmă peste 5 minute
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error snoozing alarm: {ex.Message}");
            await Navigation.PopModalAsync();
        }
    }


}