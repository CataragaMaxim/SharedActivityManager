#if ANDROID
using Android.Media;
using SharedActivityManager.Models;
using SharedActivityManager.Abstracts.Platforms;
using AndroidApp = Android.App.Application;

namespace SharedActivityManager.Platforms.Android.Services
{
    public class AndroidRingtoneService : IAudioService
    {
        private MediaPlayer _mediaPlayer;
        public bool IsPlaying { get; private set; }

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
                System.Diagnostics.Debug.WriteLine($"Android: Error loading ringtones: {ex.Message}");
            }

            return await Task.FromResult(ringtones);
        }

        private List<RingtoneProj> GetDefaultRingtones()
        {
            return new List<RingtoneProj>
            {
                new RingtoneProj {
                    Id = "default1",
                    Title = "Default Alarm",
                    FileName = "default_alarm.mp3",
                    IsSystem = true
                },
                new RingtoneProj {
                    Id = "default2",
                    Title = "Digital Beep",
                    FileName = "digital_beep.mp3",
                    IsSystem = true
                },
                new RingtoneProj {
                    Id = "default3",
                    Title = "Gentle Wake",
                    FileName = "gentle_wake.mp3",
                    IsSystem = true
                }
            };
        }

private async Task ExtractDefaultRingtones()
        {
            var ringtonesFolder = Path.Combine(FileSystem.AppDataDirectory, "Ringtones");
            if (!Directory.Exists(ringtonesFolder))
                Directory.CreateDirectory(ringtonesFolder);

            var defaultFiles = new[] { "default_alarm.mp3", "digital_beep.mp3", "gentle_wake.mp3" };

            foreach (var file in defaultFiles)
            {
                var destPath = Path.Combine(ringtonesFolder, file);
                if (!File.Exists(destPath))
                {
                    try
                    {
                        using var stream = await FileSystem.OpenAppPackageFileAsync(file);
                        using var fileStream = File.Create(destPath);
                        await stream.CopyToAsync(fileStream);
                        System.Diagnostics.Debug.WriteLine($"Extracted {file} to {destPath}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to extract {file}: {ex.Message}");
                    }
                }
            }
        }

        private async Task<string> GetDefaultRingtonePath(string ringtoneId)
        {
            try
            {
                string fileName;
                switch (ringtoneId)
                {
                    case "default1":
                        fileName = "default_alarm.mp3";
                        break;
                    case "default2":
                        fileName = "digital_beep.mp3";
                        break;
                    case "default3":
                        fileName = "gentle_wake.mp3";
                        break;
                    default:
                        fileName = "default_alarm.mp3";
                        break;
                }

                var destinationPath = Path.Combine(FileSystem.AppDataDirectory, "Ringtones", fileName);

                // Verifică dacă fișierul există deja
                if (File.Exists(destinationPath))
                {
                    System.Diagnostics.Debug.WriteLine($"Ringtone already exists: {destinationPath}");
                    return destinationPath;
                }

                // Asigură-te că folderul există
                var folder = Path.Combine(FileSystem.AppDataDirectory, "Ringtones");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                // În MAUI, resursele sunt în folderul Resources/Raw
                // Trebuie să le copiezi prima dată când rulează aplicația
                // Pentru test, poți copia manual un fișier în folder
                System.Diagnostics.Debug.WriteLine($"Ringtone not found: {destinationPath}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting default ringtone: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> PlayRingtoneAsync(string ringtoneIdentifier)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"AndroidAudioService: Attempting to play: {ringtoneIdentifier}");

                await StopPlayingAsync();

                string filePath = null;

                // Verifică dacă e un ID default (numeric sau string)
                if (ringtoneIdentifier.StartsWith("default") ||
                    (ringtoneIdentifier.Length == 1 && char.IsDigit(ringtoneIdentifier[0])))
                {
                    filePath = await GetDefaultRingtonePath(ringtoneIdentifier);
                    System.Diagnostics.Debug.WriteLine($"Default ringtone path: {filePath}");
                }
                else
                {
                    // Calea completă posibilă
                    var possiblePath = Path.Combine(FileSystem.AppDataDirectory, "Ringtones", ringtoneIdentifier);

                    if (File.Exists(possiblePath))
                    {
                        filePath = possiblePath;
                        System.Diagnostics.Debug.WriteLine($"Found exact file: {filePath}");
                    }
                    else
                    {
                        // Caută în folderul de ringtones
                        var ringtonesFolder = Path.Combine(FileSystem.AppDataDirectory, "Ringtones");
                        if (Directory.Exists(ringtonesFolder))
                        {
                            var files = Directory.GetFiles(ringtonesFolder, "*.mp3");

                            // Încearcă să găsească după numele fișierului (fără extensie)
                            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(ringtoneIdentifier);

                            foreach (var file in files)
                            {
                                string currentFileName = Path.GetFileName(file);
                                string currentFileNameWithoutExt = Path.GetFileNameWithoutExtension(file);

                                // Verifică dacă numele fișierului curent conține numele căutat
                                if (currentFileName.Contains(ringtoneIdentifier) ||
                                    currentFileNameWithoutExt.Contains(fileNameWithoutExt) ||
                                    ringtoneIdentifier.Contains(currentFileNameWithoutExt))
                                {
                                    filePath = file;
                                    System.Diagnostics.Debug.WriteLine($"Found matching file: {filePath}");
                                    break;
                                }
                            }
                        }
                    }
                }

                if (filePath != null && File.Exists(filePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Playing file: {filePath}");
                    _mediaPlayer = new MediaPlayer();
                    await _mediaPlayer.SetDataSourceAsync(filePath);
                    _mediaPlayer.Prepare();
                    _mediaPlayer.Start();
                    _mediaPlayer.Looping = true;
                    IsPlaying = true;
                    return true;
                }

                System.Diagnostics.Debug.WriteLine($"File not found for identifier: {ringtoneIdentifier}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing: {ex.Message}");
                return false;
            }
        }

        public async Task StopPlayingAsync()
        {
            try
            {
                _mediaPlayer?.Stop();
                _mediaPlayer?.Release();
                _mediaPlayer = null;
                IsPlaying = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Android: Error stopping: {ex.Message}");
            }
            await Task.CompletedTask;
        }

        public async Task SetVolumeAsync(float volume)
        {
            try
            {
                if (_mediaPlayer != null)
                {
                    _mediaPlayer.SetVolume(volume, volume);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Android: Error setting volume: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"Android: Imported {fileName}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Android: Error importing: {ex.Message}");
                return false;
            }
        }

        private async Task EnsureDefaultRingtonesExist()
        {
            try
            {
                var ringtonesFolder = Path.Combine(FileSystem.AppDataDirectory, "Ringtones");
                if (!Directory.Exists(ringtonesFolder))
                {
                    Directory.CreateDirectory(ringtonesFolder);
                }

                var defaultRingtones = new[] { "default_alarm.mp3", "digital_beep.mp3", "gentle_wake.mp3" };

                foreach (var ringtoneFile in defaultRingtones)
                {
                    var destinationPath = Path.Combine(ringtonesFolder, ringtoneFile);
                    if (!File.Exists(destinationPath))
                    {
                        // Aici poți copia din resursele aplicației dacă ai fișierele incluse
                        // De exemplu:
                        // using var stream = await FileSystem.OpenAppPackageFileAsync(ringtoneFile);
                        // using var fileStream = File.Create(destinationPath);
                        // await stream.CopyToAsync(fileStream);

                        System.Diagnostics.Debug.WriteLine($"Created default ringtone placeholder: {ringtoneFile}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ensuring default ringtones: {ex.Message}");
            }
        }
    }
}
#endif