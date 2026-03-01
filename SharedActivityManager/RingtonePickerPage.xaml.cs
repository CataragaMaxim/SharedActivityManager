using CommunityToolkit.Maui.Views; // Pentru MediaElement
using Microsoft.Maui.Controls;     // Pentru ContentPage
using Microsoft.Maui.Storage;       // Pentru FileSystem
using SharedActivityManager.Models;
using SharedActivityManager.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SharedActivityManager;

public partial class RingtonePickerPage : ContentPage, INotifyPropertyChanged
{
    private IRingtoneService _ringtoneService;
    private List<Ringtone> _allRingtones;
    private ObservableCollection<Ringtone> _filteredRingtones;
    private Ringtone _selectedRingtone;
    private bool _isPlaying;
    private Action<Ringtone> _callback;

    public ObservableCollection<Ringtone> FilteredRingtones
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
            InitializeComponent(); // Acum ar trebui să funcționeze

            _ringtoneService = new RingtoneService();
            _allRingtones = new List<Ringtone>();
            _filteredRingtones = new ObservableCollection<Ringtone>();

            LoadRingtones();
            BindingContext = this;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in constructor: {ex.Message}");
        }
    }

    public void SetRingtoneSelectedCallback(Action<Ringtone> callback)
    {
        _callback = callback;
    }

    private void LoadRingtones()
    {
        _allRingtones = _ringtoneService.GetAvailableRingtones();
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
        var ringtone = button?.CommandParameter as Ringtone;

        if (ringtone != null)
        {
            try
            {
                // Oprește redarea curentă
                MediaPlayer.Stop();

                // Setează sursa audio
                var filePath = Path.Combine(FileSystem.AppDataDirectory, "Ringtones", ringtone.FileName);

                if (File.Exists(filePath))
                {
                    MediaPlayer.Source = MediaSource.FromFile(filePath);
                    MediaPlayer.Play();
                    IsPlaying = true;
                    _selectedRingtone = ringtone;

                    await DisplayAlert("Playing", $"Now playing: {ringtone.Title}", "OK");
                }
                else
                {
                    await DisplayAlert("Error", $"File not found: {ringtone.FileName}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to play: {ex.Message}", "OK");
            }
        }
    }

    private void OnStopRingtoneClicked(object sender, EventArgs e)
    {
        MediaPlayer.Stop();
        IsPlaying = false;
    }

    private async void OnSelectRingtoneClicked(object sender, EventArgs e)
    {
        if (_selectedRingtone != null)
        {
            await _ringtoneService.SaveSelectedRingtoneAsync(_selectedRingtone.Id);
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
        MediaPlayer.Stop();
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
                var success = await _ringtoneService.ImportRingtoneFromPickerAsync(result);

                if (success)
                {
                    await DisplayAlert("Success", "Ringtone imported successfully!", "OK");
                    LoadRingtones();
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

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        MediaPlayer.Stop();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}