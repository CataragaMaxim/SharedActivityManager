using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedActivityManager.Abstracts.Platforms
{
    public interface ISettingsService
    {
        Task SaveSettingAsync<T>(string key, T value);
        Task<T> LoadSettingAsync<T>(string key, T defaultValue = default);
        bool ContainsKey(string key);
        void RemoveSetting(string key);
        void ClearAllSettings();
    }
}
