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
using RingtoneModel = SharedActivityManager.Models.Ringtone;

namespace SharedActivityManager.Services
{
    public class AndroidAudioService : IAudioService
    {
        private MediaPlayer _mediaPlayer;
        public bool IsPlaying { get; private set; }

        public async Task<List<RingtoneModel>> GetAvailableRingtonesAsync()
        {
            var ringtones = new List<RingtoneModel>();

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

                    ringtones.Add(new RingtoneModel
                    {
                        Id = data,
                        Title = title,
                        FileName = Path.GetFileName(data),
                        FilePath = data,
                        IsSystem = true
                    });
                }
                cursor.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading ringtones: {ex.Message}");
            }

            return await Task.FromResult(ringtones);
        }

        public async Task<bool> PlayRingtoneAsync(string ringtoneIdentifier)
        {
            try
            {
                await StopPlayingAsync();

                if (ringtoneIdentifier != null && (ringtoneIdentifier.StartsWith("content://") || ringtoneIdentifier.StartsWith("file://")))
                {
                    _mediaPlayer = MediaPlayer.Create(AndroidApp.Context, AndroidUri.Parse(ringtoneIdentifier));
                }
                else
                {
                    var filePath = Path.Combine(FileSystem.AppDataDirectory, "Ringtones", ringtoneIdentifier);
                    if (File.Exists(filePath))
                    {
                        _mediaPlayer = new MediaPlayer();
                        _mediaPlayer.SetDataSource(filePath);
                        _mediaPlayer.Prepare();
                    }
                }

                if (_mediaPlayer != null)
                {
                    _mediaPlayer.Start();
                    IsPlaying = true;
                    return true;
                }

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
            _mediaPlayer?.Stop();
            _mediaPlayer?.Release();
            _mediaPlayer = null;
            IsPlaying = false;
            await Task.CompletedTask;
        }

        public async Task SetVolumeAsync(float volume)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.SetVolume(volume, volume);
            }
            await Task.CompletedTask;
        }

        public async Task<bool> ImportRingtoneAsync(string filePath)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);
                var destinationPath = Path.Combine(FileSystem.AppDataDirectory, "Ringtones", fileName);

                if (!Directory.Exists(Path.Combine(FileSystem.AppDataDirectory, "Ringtones")))
                {
                    Directory.CreateDirectory(Path.Combine(FileSystem.AppDataDirectory, "Ringtones"));
                }

                File.Copy(filePath, destinationPath, true);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error importing: {ex.Message}");
                return false;
            }
        }
    }
}
#endif