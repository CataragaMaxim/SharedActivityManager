using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedActivityManager.Abstracts.Platforms;

namespace SharedActivityManager.Services
{
    internal class WindowsSettingsService : ISettingsService
    {
        public void ClearAllSettings()
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public Task<T> LoadSettingAsync<T>(string key, T defaultValue = default)
        {
            throw new NotImplementedException();
        }

        public void RemoveSetting(string key)
        {
            throw new NotImplementedException();
        }

        public Task SaveSettingAsync<T>(string key, T value)
        {
            throw new NotImplementedException();
        }
    }
}
