#if WINDOWS
using CommunityToolkit.Maui.Views;
using SharedActivityManager.Models;
using SharedActivityManager.Abstracts.Platforms;

namespace SharedActivityManager.Services
{
    public class WindowsRingtoneService : IAudioService
    {
        private MediaElement _mediaElement;
        public bool IsPlaying { get; private set; }

        public WindowsRingtoneService()
        {
            _mediaElement = new MediaElement
            {
                ShouldAutoPlay = true,
                ShouldKeepScreenOn = true,
                Volume = 1.0
            };
        }

        public async Task<List<RingtoneProj>> GetAvailableRingtonesAsync()
        {
            var ringtones = new List<RingtoneProj>();

            try
            {
                // Tonuri default
                ringtones.AddRange(GetDefaultRingtones());

                // Tonuri din folderul aplicației
                var folder = Path.Combine(FileSystem.AppDataDirectory, "Ringtones");
                if (Directory.Exists(folder))
                {
                    foreach (var file in Directory.GetFiles(folder, "*.mp3"))
                    {
                        ringtones.Add(new RingtoneProj
                        {
                            Id = Path.GetFileName(file),
                            Title = Path.GetFileNameWithoutExtension(file),
                            FileName = Path.GetFileName(file),
                            FilePath = file,
                            IsSystem = false
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Windows: Error loading ringtones: {ex.Message}");
            }

            return await Task.FromResult(ringtones);
        }

        private List<RingtoneProj> GetDefaultRingtones()
        {
            return new List<RingtoneProj>
            {
                new RingtoneProj { Id = "default1", Title = "Default Alarm", FileName = "default_alarm.mp3", IsSystem = true },
                new RingtoneProj { Id = "default2", Title = "Digital Beep", FileName = "digital_beep.mp3", IsSystem = true },
                new RingtoneProj { Id = "default3", Title = "Gentle Wake", FileName = "gentle_wake.mp3", IsSystem = true }
            };
        }

        public async Task<bool> PlayRingtoneAsync(string ringtoneIdentifier)
        {
            try
            {
                await StopPlayingAsync();

                string filePath = null;

                if (ringtoneIdentifier.StartsWith("default"))
                {
                    var fileName = ringtoneIdentifier + ".mp3";
                    filePath = Path.Combine(FileSystem.AppDataDirectory, "Ringtones", fileName);
                }
                else
                {
                    filePath = Path.Combine(FileSystem.AppDataDirectory, "Ringtones", ringtoneIdentifier);
                }

                if (File.Exists(filePath))
                {
                    _mediaElement.Source = MediaSource.FromFile(filePath);
                    _mediaElement.Play();
                    IsPlaying = true;
                    System.Diagnostics.Debug.WriteLine($"Windows: Playing {filePath}");
                    return true;
                }

                System.Diagnostics.Debug.WriteLine($"Windows: File not found: {filePath}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Windows: Error playing: {ex.Message}");
                return false;
            }
        }

        public async Task StopPlayingAsync()
        {
            try
            {
                _mediaElement?.Stop();
                IsPlaying = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Windows: Error stopping: {ex.Message}");
            }
            await Task.CompletedTask;
        }

        public async Task SetVolumeAsync(float volume)
        {
            try
            {
                _mediaElement.Volume = volume;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Windows: Error setting volume: {ex.Message}");
            }
            await Task.CompletedTask;
        }

        public async Task<bool> ImportRingtoneAsync(string filePath)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);
                var destPath = Path.Combine(FileSystem.AppDataDirectory, "Ringtones", fileName);

                if (!Directory.Exists(Path.Combine(FileSystem.AppDataDirectory, "Ringtones")))
                {
                    Directory.CreateDirectory(Path.Combine(FileSystem.AppDataDirectory, "Ringtones"));
                }

                File.Copy(filePath, destPath, true);
                System.Diagnostics.Debug.WriteLine($"Windows: Imported {fileName}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Windows: Error importing: {ex.Message}");
                return false;
            }
        }
    }
}
#endif