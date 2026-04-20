using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Adapters
{
    /// <summary>
    /// Clasă abstractă de bază pentru toți adaptorii
    /// </summary>
    public abstract class BaseFileAdapter : IFileAdapter
    {
        public abstract string FormatName { get; }
        public abstract string[] SupportedExtensions { get; }

        public abstract Task<List<Activity>> ImportAsync(string filePath);
        public abstract Task ExportAsync(string filePath, List<Activity> activities);

        public virtual bool CanHandle(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            return SupportedExtensions.Contains(extension);
        }

        // Metode helper pentru conversii
        protected string GetFileExtension(string filePath) => Path.GetExtension(filePath).ToLower();

        protected bool FileExists(string filePath) => File.Exists(filePath);
    }
}