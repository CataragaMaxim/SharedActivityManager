using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedActivityManager.Services;

namespace SharedActivityManager.Abstracts.Platforms
{
    public interface IPlatformFactory
    {
        IAlarmService CreateAlarmService();
        INotificationService CreateNotificationService();
        IAudioService CreateAudioService();
        ISettingsService CreateSettingsService();
        string GetPlatformName();
    }
}
