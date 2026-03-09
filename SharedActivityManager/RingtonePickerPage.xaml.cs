using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SharedActivityManager.Models;
using SharedActivityManager.Abstracts.Platforms; // ← Adaugă asta
using SharedActivityManager.Services; // ← PlatformServiceLocator
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SharedActivityManager;

public partial class RingtonePickerPage : ContentPage, INotifyPropertyChanged
{
    private IAudioService _audioService; // ← Schimbat din IRingtoneService în IAudioService
    private List<RingtoneProj> _allRingtones;
    private ObservableCollection<RingtoneProj> _filteredRingtones;
    private RingtoneProj _selectedRingtone;
    private bool _isPlaying;
    private Action<RingtoneProj> _callback;

    public ObservableCollection<RingtoneProj> FilteredRingtones
    {
        get => _filteredRingtones;
        set
        {
            _filteredRingtones = value;
            OnPropertyChanged();
        }
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        set
        {
            _isPlaying = value;
            OnPropertyChanged();
        }
    }

    public RingtonePickerPage()
    {
        try
        {
            InitializeComponent();

            _audioService = PlatformServiceLocator.AudioService; // ← Folosește PlatformServiceLocator
            _allRingtones = new List<RingtoneProj>();
            _filteredRingtones = new ObservableCollection<RingtoneProj>();

            LoadRingtones();
            BindingContext = this;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in constructor: {ex.Message}");
        }
    }

    public void SetRingtoneSelectedCallback(Action<RingtoneProj> callback)
    {
        _callback = callback;
    }

    private async void LoadRingtones() // ← Schimbat în async void
    {
        var ringtones = await _audioService.GetAvailableRingtonesAsync(); // ← Folosește _audioService
        _allRingtones = ringtones.ToList();
        FilterRingtones(string.Empty);
    }

    private void FilterRingtones(string searchText)
    {
        FilteredRingtones.Clear();

        var filtered = string.IsNullOrWhiteSpace(searchText)
            ? _allRingtones
            : _allRingtones.Where(r => r.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase));

        foreach (var ringtone in filtered)
        {
            FilteredRingtones.Add(ringtone);
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        FilterRingtones(e.NewTextValue);
    }

    private async void OnPlayRingtoneClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var ringtone = button?.CommandParameter as RingtoneProj;

        if (ringtone != null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"RingtonePicker: Playing ringtone - Title: {ringtone.Title}, FileName: {ringtone.FileName}, DisplayName: {ringtone.DisplayName}");

                // Oprește redarea curentă
                await _audioService.StopPlayingAsync();

                // 🔥 FIX: Folosește Title sau FileName, nu DisplayName
                // Title nu conține emoji-ul, FileName conține numele real al fișierului
                string identifierToUse = ringtone.FileName;

                // Dacă FileName este gol, încearcă Title
                if (string.IsNullOrEmpty(identifierToUse))
                {
                    identifierToUse = ringtone.Title;
                }

                // Dacă nici Title nu e disponibil, folosește Id
                if (string.IsNullOrEmpty(identifierToUse))
                {
                    identifierToUse = ringtone.Id;
                }

                System.Diagnostics.Debug.WriteLine($"RingtonePicker: Using identifier: {identifierToUse}");

                var success = await _audioService.PlayRingtoneAsync(identifierToUse);

                if (success)
                {
                    IsPlaying = true;
                    _selectedRingtone = ringtone;
                    await DisplayAlert("Playing", $"Now playing: {ringtone.Title}", "OK");
                }
                else
                {
                    await DisplayAlert("Error", $"Could not play: {ringtone.Title}", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RingtonePicker: Error playing: {ex.Message}");
                await DisplayAlert("Error", $"Failed to play: {ex.Message}", "OK");
            }
        }
    }

    private async void OnStopRingtoneClicked(object sender, EventArgs e)
    {
        await _audioService.StopPlayingAsync(); // ← Folosește IAudioService
        IsPlaying = false;
    }

    private async void OnSelectRingtoneClicked(object sender, EventArgs e)
    {
        if (_selectedRingtone != null)
        {
            // Salvează în Preferences
            Preferences.Set("SelectedRingtone", _selectedRingtone.Id);
            _callback?.Invoke(_selectedRingtone);
            await Navigation.PopModalAsync();
        }
        else
        {
            await DisplayAlert("No Selection", "Please play a ringtone first", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await _audioService.StopPlayingAsync(); // ← Folosește IAudioService
        await Navigation.PopModalAsync();
    }

    private async void OnImportMusicClicked(object sender, EventArgs e)
    {
        try
        {
            var options = new PickOptions
            {
                PickerTitle = "Select a music file",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".mp3", ".wav", ".m4a" } },
                    { DevicePlatform.Android, new[] { "audio/mpeg", "audio/wav", "audio/mp4" } },
                    { DevicePlatform.iOS, new[] { "public.audio" } }
                })
            };

            var result = await FilePicker.Default.PickAsync(options);

            if (result != null)
            {
                // Import folosind IAudioService
                var success = await _audioService.ImportRingtoneAsync(result.FullPath);

                if (success)
                {
                    await DisplayAlert("Success", "Ringtone imported successfully!", "OK");
                    LoadRingtones(); // Reîncarcă lista
                }
                else
                {
                    await DisplayAlert("Error", "Failed to import ringtone", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to import: {ex.Message}", "OK");
        }
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await _audioService.StopPlayingAsync(); // ← Folosește IAudioService
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}