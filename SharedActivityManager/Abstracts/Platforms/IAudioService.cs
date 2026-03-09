using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedActivityManager.Models;

namespace SharedActivityManager.Abstracts.Platforms
{
    public interface IAudioService
    {
        Task<List<RingtoneProj>> GetAvailableRingtonesAsync();
        Task<bool> PlayRingtoneAsync(string ringtoneIdentifier);
        Task StopPlayingAsync();
        Task SetVolumeAsync(float volume);
        bool IsPlaying { get; }
        Task<bool> ImportRingtoneAsync(string filePath);
    }
}
