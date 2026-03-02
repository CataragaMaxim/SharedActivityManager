using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Models;

namespace SharedActivityManager.Services
{
    internal class WindowsAudioServices : IAudioService
    {
        public bool IsPlaying => throw new NotImplementedException();

        public Task<List<Ringtone>> GetAvailableRingtonesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ImportRingtoneAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PlayRingtoneAsync(string ringtoneIdentifier)
        {
            throw new NotImplementedException();
        }

        public Task SetVolumeAsync(float volume)
        {
            throw new NotImplementedException();
        }

        public Task StopPlayingAsync()
        {
            throw new NotImplementedException();
        }
    }
}
