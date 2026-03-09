using SharedActivityManager.Models;

namespace SharedActivityManager.Services
{
    public interface IRingtoneService
    {
        List<RingtoneProj> GetAvailableRingtones();
        Task<bool> PlayRingtoneAsync(string ringtoneFileName);
        Task StopPlayingAsync(); // Asigură-te că această metodă există
        Task SaveSelectedRingtoneAsync(string ringtoneId);
        string LoadSelectedRingtone();
        bool IsPlaying { get; }
        Task<bool> AddCustomRingtoneAsync(string filePath);
        Task<bool> ImportRingtoneFromPickerAsync(FileResult file);
    }
}