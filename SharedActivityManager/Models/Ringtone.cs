// Models/Ringtone.cs
namespace SharedActivityManager.Models
{
    public class Ringtone
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public bool IsSystem { get; set; }

        public string DisplayName => IsSystem ? $"🔔 {Title}" : $"🎵 {Title}";
        public string SourceType => IsSystem ? "System Ringtone" : "Custom Sound";
    }
}