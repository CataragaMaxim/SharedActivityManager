#if ANDROID
using System;
using System.Text.Json;
using System.Threading.Tasks;
using SharedActivityManager.Abstracts.Platforms;

namespace SharedActivityManager.Services
{
    public class AndroidSettingsService : ISettingsService
    {
        public async Task SaveSettingAsync<T>(string key, T value)
        {
            try
            {
                if (typeof(T) == typeof(string) || typeof(T).IsValueType)
                {
                    Preferences.Set(key, value.ToString());
                }
                else
                {
                    var json = JsonSerializer.Serialize(value);
                    Preferences.Set(key, json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving setting: {ex.Message}");
            }
            await Task.CompletedTask;
        }

        public async Task<T> LoadSettingAsync<T>(string key, T defaultValue = default)
        {
            try
            {
                if (!Preferences.ContainsKey(key))
                {
                    return defaultValue;
                }

                if (typeof(T) == typeof(string))
                {
                    return (T)(object)Preferences.Get(key, defaultValue?.ToString() ?? "");
                }
                else if (typeof(T) == typeof(int))
                {
                    return (T)(object)Preferences.Get(key, (int)(object)defaultValue);
                }
                else if (typeof(T) == typeof(bool))
                {
                    return (T)(object)Preferences.Get(key, (bool)(object)defaultValue);
                }
                else if (typeof(T) == typeof(double))
                {
                    return (T)(object)Preferences.Get(key, (double)(object)defaultValue);
                }
                else
                {
                    var json = Preferences.Get(key, "");
                    if (!string.IsNullOrEmpty(json))
                    {
                        return JsonSerializer.Deserialize<T>(json);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading setting: {ex.Message}");
            }
            return defaultValue;
        }

        public bool ContainsKey(string key)
        {
            return Preferences.ContainsKey(key);
        }

        public void RemoveSetting(string key)
        {
            Preferences.Remove(key);
        }

        public void ClearAllSettings()
        {
            Preferences.Clear();
        }
    }
}
#endif