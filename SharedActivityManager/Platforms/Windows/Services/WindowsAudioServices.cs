#if WINDOWS
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Models;
using Windows.Storage;
using Windows.Media.Playback;
using Windows.Media.Core;

namespace SharedActivityManager.Platforms.Windows.Services
{
    public class WindowsAudioService : IAudioService
    {
        private MediaPlayer _mediaPlayer;
        public bool IsPlaying { get; private set; }

        public WindowsAudioService()
        {
            _mediaPlayer = new MediaPlayer();
        }

        public async Task<List<RingtoneProj>> GetAvailableRingtonesAsync()
        {
            var ringtones = new List<RingtoneProj>();

            try
            {
                // Tonuri default (le vom crea în folderul aplicației)
                ringtones.AddRange(GetDefaultRingtones());

                // Tonuri din folderul aplicației
                var ringtonesFolder = await GetRingtonesFolderAsync();

                if (Directory.Exists(ringtonesFolder))
                {
                    var files = Directory.GetFiles(ringtonesFolder, "*.*");
                    foreach (var file in files)
                    {
                        string extension = Path.GetExtension(file).ToLower();
                        if (extension == ".mp3" || extension == ".wav" || extension == ".m4a" || extension == ".aac")
                        {
                            string fileName = Path.GetFileName(file);

                            // Evită dublarea tonurilor default
                            if (fileName.StartsWith("default_"))
                                continue;

                            ringtones.Add(new RingtoneProj
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

                // Asigură-te că tonurile default există
                await EnsureDefaultRingtonesExistAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WindowsAudioService: Error loading ringtones: {ex.Message}");
            }

            return ringtones;
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

        private async Task<string> GetRingtonesFolderAsync()
        {
            string folderPath = Path.Combine(FileSystem.AppDataDirectory, "Ringtones");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return await Task.FromResult(folderPath);
        }

        private async Task EnsureDefaultRingtonesExistAsync()
        {
            try
            {
                var ringtonesFolder = await GetRingtonesFolderAsync();

                var defaultFiles = new[] { "default_alarm.mp3", "digital_beep.mp3", "gentle_wake.mp3" };

                foreach (var file in defaultFiles)
                {
                    var destPath = Path.Combine(ringtonesFolder, file);
                    if (!File.Exists(destPath))
                    {
                        // Creează un fișier gol ca placeholder
                        // În realitate, ar trebui să ai aceste fișiere în resurse
                        await File.WriteAllTextAsync(destPath, "Placeholder");
                        System.Diagnostics.Debug.WriteLine($"WindowsAudioService: Created placeholder for {file}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WindowsAudioService: Error ensuring default ringtones: {ex.Message}");
            }
        }

        public async Task<bool> PlayRingtoneAsync(string ringtoneIdentifier)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"WindowsAudioService: Attempting to play: {ringtoneIdentifier}");

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
                    // Calea completă
                    var ringtonesFolder = await GetRingtonesFolderAsync();

                    // Încearcă cu numele exact
                    var possiblePath = Path.Combine(ringtonesFolder, ringtoneIdentifier);

                    if (File.Exists(possiblePath))
                    {
                        filePath = possiblePath;
                        System.Diagnostics.Debug.WriteLine($"WindowsAudioService: Found exact file: {filePath}");
                    }
                    else
                    {
                        // Încearcă fără extensie
                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(ringtoneIdentifier);
                        var searchPattern = $"{fileNameWithoutExt}.*";

                        var files = Directory.GetFiles(ringtonesFolder, searchPattern);
                        if (files.Length > 0)
                        {
                            filePath = files[0];
                            System.Diagnostics.Debug.WriteLine($"WindowsAudioService: Found matching file: {filePath}");
                        }
                        else
                        {
                            // Caută orice fișier care conține numele
                            files = Directory.GetFiles(ringtonesFolder, "*.*");
                            foreach (var file in files)
                            {
                                if (file.Contains(fileNameWithoutExt) || fileNameWithoutExt.Contains(Path.GetFileNameWithoutExtension(file)))
                                {
                                    filePath = file;
                                    System.Diagnostics.Debug.WriteLine($"WindowsAudioService: Found partial match: {filePath}");
                                    break;
                                }
                            }
                        }
                    }
                }

                if (filePath != null && File.Exists(filePath))
                {
                    System.Diagnostics.Debug.WriteLine($"WindowsAudioService: Playing file: {filePath}");

                    var mediaSource = MediaSource.CreateFromUri(new Uri(filePath));
                    _mediaPlayer.Source = mediaSource;
                    _mediaPlayer.Play();

                    IsPlaying = true;
                    return true;
                }

                System.Diagnostics.Debug.WriteLine($"WindowsAudioService: File not found for identifier: {ringtoneIdentifier}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WindowsAudioService: Error playing: {ex.Message}");
                return false;
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

                var ringtonesFolder = await GetRingtonesFolderAsync();
                return Path.Combine(ringtonesFolder, fileName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WindowsAudioService: Error getting default ringtone: {ex.Message}");
                return null;
            }
        }

        public async Task StopPlayingAsync()
        {
            try
            {
                if (_mediaPlayer != null)
                {
                    _mediaPlayer.Pause();
                    _mediaPlayer.Source = null;
                }

                IsPlaying = false;
                System.Diagnostics.Debug.WriteLine($"WindowsAudioService: Stopped playing");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WindowsAudioService: Error stopping: {ex.Message}");
            }
            await Task.CompletedTask;
        }

        public async Task SetVolumeAsync(float volume)
        {
            try
            {
                if (_mediaPlayer != null)
                {
                    _mediaPlayer.Volume = volume;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WindowsAudioService: Error setting volume: {ex.Message}");
            }
            await Task.CompletedTask;
        }

        public async Task<bool> ImportRingtoneAsync(string filePath)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);
                var ringtonesFolder = await GetRingtonesFolderAsync();
                var destPath = Path.Combine(ringtonesFolder, fileName);

                File.Copy(filePath, destPath, true);
                System.Diagnostics.Debug.WriteLine($"WindowsAudioService: Imported {fileName}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WindowsAudioService: Error importing: {ex.Message}");
                return false;
            }
        }
    }
}
#endif