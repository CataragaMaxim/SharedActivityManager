// Services/RingtoneService.cs
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using SharedActivityManager.Models;

namespace SharedActivityManager.Services
{
    public class RingtoneService : IRingtoneService
    {
        private MediaElement _mediaElement;
        private readonly string _appRingtonesFolder;
        private readonly string _customMusicFolder = @"D:\music comp\Music"; // Folderul tău de muzică
        public bool IsPlaying { get; private set; }

        public RingtoneService()
        {
            _appRingtonesFolder = Path.Combine(FileSystem.AppDataDirectory, "Ringtones");

            if (!Directory.Exists(_appRingtonesFolder))
            {
                Directory.CreateDirectory(_appRingtonesFolder);
            }

            _mediaElement = new MediaElement
            {
                ShouldAutoPlay = true,
                ShouldKeepScreenOn = true,
                Volume = 1.0
            };

            _mediaElement.MediaEnded += OnMediaEnded;
            _mediaElement.MediaFailed += OnMediaFailed;
        }

        private void OnMediaEnded(object sender, EventArgs e)
        {
            IsPlaying = false;
        }

        private void OnMediaFailed(object sender, MediaFailedEventArgs e)
        {
            IsPlaying = false;
            System.Diagnostics.Debug.WriteLine($"Media failed: {e.ErrorMessage}");
        }

        public List<Ringtone> GetAvailableRingtones()
        {
            var ringtones = new List<Ringtone>();

            // Tonuri implicite
            ringtones.AddRange(GetDefaultRingtones());

            // Tonuri din folderul aplicației
            ringtones.AddRange(GetAppRingtones());

            // Tonuri din folderul tău personal de muzică
            ringtones.AddRange(GetCustomMusicFolderRingtones());

            return ringtones;
        }

        private List<Ringtone> GetDefaultRingtones()
        {
            return new List<Ringtone>
            {
                new Ringtone { Id = "default1", Title = "Default Alarm", FileName = "default_alarm.mp3", IsSystem = true },
                new Ringtone { Id = "default2", Title = "Digital Beep", FileName = "digital_beep.mp3", IsSystem = true },
                new Ringtone { Id = "default3", Title = "Gentle Wake", FileName = "gentle_wake.mp3", IsSystem = true }
            };
        }

        private List<Ringtone> GetAppRingtones()
        {
            var ringtones = new List<Ringtone>();

            try
            {
                if (Directory.Exists(_appRingtonesFolder))
                {
                    var files = Directory.GetFiles(_appRingtonesFolder, "*.mp3")
                                 .Concat(Directory.GetFiles(_appRingtonesFolder, "*.wav"))
                                 .Concat(Directory.GetFiles(_appRingtonesFolder, "*.m4a"))
                                 .ToList();

                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileName(file);
                        ringtones.Add(new Ringtone
                        {
                            Id = fileName,
                            Title = Path.GetFileNameWithoutExtension(file),
                            FileName = fileName,
                            FilePath = file,
                            IsSystem = false
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading app ringtones: {ex.Message}");
            }

            return ringtones;
        }

        // NOU: Metodă pentru a încărca muzica din folderul tău personal
        private List<Ringtone> GetCustomMusicFolderRingtones()
        {
            var ringtones = new List<Ringtone>();

            try
            {
                if (Directory.Exists(_customMusicFolder))
                {
                    System.Diagnostics.Debug.WriteLine($"Loading music from: {_customMusicFolder}");

                    // Caută toate fișierele audio
                    var files = Directory.GetFiles(_customMusicFolder, "*.mp3", SearchOption.AllDirectories)
                                 .Concat(Directory.GetFiles(_customMusicFolder, "*.wav", SearchOption.AllDirectories))
                                 .Concat(Directory.GetFiles(_customMusicFolder, "*.m4a", SearchOption.AllDirectories))
                                 .Concat(Directory.GetFiles(_customMusicFolder, "*.flac", SearchOption.AllDirectories))
                                 .ToList();

                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileName(file);
                        var directoryName = Path.GetFileName(Path.GetDirectoryName(file));

                        // Creează un titlu care include și folderul pentru organizare
                        string displayTitle = string.IsNullOrEmpty(directoryName)
                            ? Path.GetFileNameWithoutExtension(file)
                            : $"{directoryName} - {Path.GetFileNameWithoutExtension(file)}";

                        ringtones.Add(new Ringtone
                        {
                            Id = file, // Folosim calea completă ca ID
                            Title = displayTitle,
                            FileName = fileName,
                            FilePath = file, // Păstrăm calea completă
                            IsSystem = false
                        });

                        System.Diagnostics.Debug.WriteLine($"Found music: {displayTitle}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Custom music folder not found: {_customMusicFolder}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading custom music: {ex.Message}");
            }

            return ringtones;
        }

        // Modificat pentru a folosi calea completă
        public async Task<bool> PlayRingtoneAsync(string ringtoneIdentifier)
        {
            try
            {
                await StopPlayingAsync();

                // Verifică dacă e o cale completă
                if (File.Exists(ringtoneIdentifier))
                {
                    _mediaElement.Source = MediaSource.FromFile(ringtoneIdentifier);
                    _mediaElement.Play();
                    IsPlaying = true;
                    System.Diagnostics.Debug.WriteLine($"Playing from full path: {ringtoneIdentifier}");
                    return true;
                }

                // Dacă nu e cale completă, caută în foldere
                var filePath = Path.Combine(_appRingtonesFolder, ringtoneIdentifier);

                if (!File.Exists(filePath))
                {
                    // Caută în folderul de muzică
                    var musicFiles = Directory.GetFiles(_customMusicFolder, ringtoneIdentifier, SearchOption.AllDirectories);
                    if (musicFiles.Length > 0)
                    {
                        filePath = musicFiles[0];
                    }
                }

                if (File.Exists(filePath))
                {
                    _mediaElement.Source = MediaSource.FromFile(filePath);
                    _mediaElement.Play();
                    IsPlaying = true;
                    System.Diagnostics.Debug.WriteLine($"Playing from: {filePath}");
                    return true;
                }

                System.Diagnostics.Debug.WriteLine($"File not found: {ringtoneIdentifier}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing ringtone: {ex.Message}");
                return false;
            }
        }

        public async Task StopPlayingAsync()
        {
            try
            {
                _mediaElement?.Stop();
                IsPlaying = false;
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping ringtone: {ex.Message}");
            }
        }

        public async Task<bool> ImportRingtoneFromPickerAsync(FileResult file)
        {
            try
            {
                var destinationPath = Path.Combine(_appRingtonesFolder, file.FileName);

                using var stream = await file.OpenReadAsync();
                using var fileStream = File.Create(destinationPath);
                await stream.CopyToAsync(fileStream);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error importing ringtone: {ex.Message}");
                return false;
            }
        }

        public async Task SaveSelectedRingtoneAsync(string ringtoneId)
        {
            Preferences.Set("SelectedRingtone", ringtoneId);
            await Task.CompletedTask;
        }

        public string LoadSelectedRingtone()
        {
            return Preferences.Get("SelectedRingtone", "default1");
        }

        public async Task<bool> AddCustomRingtoneAsync(string filePath)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);
                var destinationPath = Path.Combine(_appRingtonesFolder, fileName);

                File.Copy(filePath, destinationPath, true);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding ringtone: {ex.Message}");
                return false;
            }
        }
    }
}