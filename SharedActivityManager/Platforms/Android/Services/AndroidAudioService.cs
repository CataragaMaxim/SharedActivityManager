#if ANDROID
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Media;
using Android.Provider;
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Models;
using AndroidApp = Android.App.Application;
using AndroidUri = Android.Net.Uri;

namespace SharedActivityManager.Platforms.Android.Services
{
    public class AndroidAudioService : IAudioService
    {
        private MediaPlayer _mediaPlayer;
        public bool IsPlaying { get; private set; }

        // Obține toate tonurile disponibile (sistem + aplicație)
        public async Task<List<RingtoneProj>> GetAvailableRingtonesAsync()
        {
            var ringtones = new List<RingtoneProj>();

            try
            {
                // 1. Tonuri de sistem (folosind RingtoneManager)
                var systemRingtones = await GetSystemRingtonesAsync();
                ringtones.AddRange(systemRingtones);

                // 2. Tonuri default ale aplicației (cele hardcodate)
                var defaultAppRingtones = GetDefaultAppRingtones();
                ringtones.AddRange(defaultAppRingtones);

                // 3. Tonuri din folderul aplicației (importate de utilizator)
                var importedRingtones = await GetImportedRingtonesAsync();
                ringtones.AddRange(importedRingtones);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AndroidAudioService: Error loading ringtones: {ex.Message}");
            }

            return ringtones;
        }

        private async Task<List<RingtoneProj>> GetSystemRingtonesAsync()
        {
            var ringtones = new List<RingtoneProj>();

            try
            {
                var ringtoneManager = new RingtoneManager(AndroidApp.Context);
                ringtoneManager.SetType(RingtoneType.Alarm);
                var cursor = ringtoneManager.Cursor;

                int titleColumnIndex = cursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Title);
                int dataColumnIndex = cursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Data);

                while (cursor.MoveToNext())
                {
                    var title = cursor.GetString(titleColumnIndex);
                    var data = cursor.GetString(dataColumnIndex);

                    ringtones.Add(new RingtoneProj
                    {
                        Id = data,
                        Title = title,
                        FileName = Path.GetFileName(data),
                        FilePath = data,
                        IsSystem = true,
                    });
                }
                cursor.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AndroidAudioService: Error loading system ringtones: {ex.Message}");
            }

            return ringtones;
        }

        private List<RingtoneProj> GetDefaultAppRingtones()
        {
            var ringtones = new List<RingtoneProj>
            {
                new RingtoneProj {
                    Id = "default1",
                    Title = "Default Alarm",
                    FileName = "default_alarm.mp3",
                    IsSystem = false
                },
                new RingtoneProj {
                    Id = "default2",
                    Title = "Digital Beep",
                    FileName = "digital_beep.mp3",
                    IsSystem = false
                },
                new RingtoneProj {
                    Id = "default3",
                    Title = "Gentle Wake",
                    FileName = "gentle_wake.mp3",
                    IsSystem = false
                }
            };

            // Asigură-te că fișierele default există
            Task.Run(async () => await EnsureDefaultRingtonesExist());

            return ringtones;
        }

        private async Task<List<RingtoneProj>> GetImportedRingtonesAsync()
        {
            var ringtones = new List<RingtoneProj>();

            try
            {
                var folder = Path.Combine(FileSystem.AppDataDirectory, "Ringtones");
                if (Directory.Exists(folder))
                {
                    foreach (var file in Directory.GetFiles(folder, "*.mp3"))
                    {
                        // Ignoră fișierele default (ca să nu apară dublat)
                        string fileName = Path.GetFileName(file);
                        if (fileName == "default_alarm.mp3" ||
                            fileName == "digital_beep.mp3" ||
                            fileName == "gentle_wake.mp3")
                        {
                            continue;
                        }

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
                System.Diagnostics.Debug.WriteLine($"AndroidAudioService: Error loading imported ringtones: {ex.Message}");
            }

            return ringtones;
        }

        public async Task<bool> PlayRingtoneAsync(string ringtoneIdentifier)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"AndroidAudioService: Attempting to play: {ringtoneIdentifier}");

                await StopPlayingAsync();

                string filePath = null;

                // Verifică dacă e ton default
                if (ringtoneIdentifier.StartsWith("default") ||
                    ringtoneIdentifier == "default1" ||
                    ringtoneIdentifier == "default2" ||
                    ringtoneIdentifier == "default3")
                {
                    filePath = await GetDefaultRingtonePath(ringtoneIdentifier);
                }
                else
                {
                    var ringtonesFolder = Path.Combine(FileSystem.AppDataDirectory, "Ringtones");

                    // Caută fișierul indiferent de extensie
                    var possiblePath = Path.Combine(ringtonesFolder, ringtoneIdentifier);

                    if (File.Exists(possiblePath))
                    {
                        filePath = possiblePath;
                    }
                    else
                    {
                        // Caută după nume fără extensie
                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(ringtoneIdentifier);
                        var files = Directory.GetFiles(ringtonesFolder, "*.*");

                        foreach (var file in files)
                        {
                            string currentFileName = Path.GetFileName(file);
                            string currentNameWithoutExt = Path.GetFileNameWithoutExtension(file);

                            if (currentFileName.Equals(ringtoneIdentifier, StringComparison.OrdinalIgnoreCase) ||
                                currentNameWithoutExt.Equals(fileNameWithoutExt, StringComparison.OrdinalIgnoreCase) ||
                                currentNameWithoutExt.Contains(fileNameWithoutExt))
                            {
                                filePath = file;
                                break;
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

        private async Task<string> FindRingtoneFile(string identifier)
        {
            // Calea completă posibilă
            var possiblePath = Path.Combine(FileSystem.AppDataDirectory, "Ringtones", identifier);
            if (File.Exists(possiblePath))
            {
                return possiblePath;
            }

            // Caută în folderul de ringtones
            var ringtonesFolder = Path.Combine(FileSystem.AppDataDirectory, "Ringtones");
            if (Directory.Exists(ringtonesFolder))
            {
                var files = Directory.GetFiles(ringtonesFolder, "*.mp3");

                // Încearcă să găsească după numele fișierului (fără extensie)
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(identifier);

                foreach (var file in files)
                {
                    string currentFileName = Path.GetFileName(file);
                    string currentFileNameWithoutExt = Path.GetFileNameWithoutExtension(file);

                    // Verifică diferite potriviri
                    if (currentFileName.Equals(identifier, StringComparison.OrdinalIgnoreCase) ||
                        currentFileNameWithoutExt.Equals(identifier, StringComparison.OrdinalIgnoreCase) ||
                        currentFileName.Contains(identifier, StringComparison.OrdinalIgnoreCase) ||
                        currentFileNameWithoutExt.Contains(fileNameWithoutExt, StringComparison.OrdinalIgnoreCase) ||
                        identifier.Contains(currentFileNameWithoutExt, StringComparison.OrdinalIgnoreCase))
                    {
                        return file;
                    }
                }
            }

            return null;
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
                    return destinationPath;
                }

                // Dacă nu există, încearcă să-l extragă din resurse
                await ExtractDefaultRingtone(fileName, destinationPath);

                return File.Exists(destinationPath) ? destinationPath : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AndroidAudioService: Error getting default ringtone: {ex.Message}");
                return null;
            }
        }

        private async Task ExtractDefaultRingtone(string fileName, string destinationPath)
        {
            try
            {
                var folder = Path.Combine(FileSystem.AppDataDirectory, "Ringtones");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                // Încearcă să extragă din resursele aplicației
                using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
                using var fileStream = File.Create(destinationPath);
                await stream.CopyToAsync(fileStream);
                System.Diagnostics.Debug.WriteLine($"Extracted {fileName} to {destinationPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to extract {fileName}: {ex.Message}");
            }
        }

        private async Task EnsureDefaultRingtonesExist()
        {
            var ringtonesFolder = Path.Combine(FileSystem.AppDataDirectory, "Ringtones");
            if (!Directory.Exists(ringtonesFolder))
            {
                Directory.CreateDirectory(ringtonesFolder);
            }

            var defaultFiles = new[] { "default_alarm.mp3", "digital_beep.mp3", "gentle_wake.mp3" };

            foreach (var file in defaultFiles)
            {
                var destPath = Path.Combine(ringtonesFolder, file);
                if (!File.Exists(destPath))
                {
                    await ExtractDefaultRingtone(file, destPath);
                }
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
                System.Diagnostics.Debug.WriteLine($"AndroidAudioService: Error stopping: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"AndroidAudioService: Error setting volume: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"AndroidAudioService: Imported {fileName}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AndroidAudioService: Error importing: {ex.Message}");
                return false;
            }
        }
    }
}
#endif