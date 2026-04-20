using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Adapters
{
    /// <summary>
    /// Target interface - interfața pe care o folosește clientul
    /// </summary>
    public interface IFileAdapter
    {
        /// <summary>
        /// Numele formatului suportat (ex: "CSV", "JSON", "XML")
        /// </summary>
        string FormatName { get; }

        /// <summary>
        /// Extensiile de fișier suportate (ex: ".csv", ".json", ".xml")
        /// </summary>
        string[] SupportedExtensions { get; }

        /// <summary>
        /// Importă activități dintr-un fișier
        /// </summary>
        Task<List<Activity>> ImportAsync(string filePath);

        /// <summary>
        /// Exportă activități într-un fișier
        /// </summary>
        Task ExportAsync(string filePath, List<Activity> activities);

        /// <summary>
        /// Verifică dacă fișierul este în formatul suportat
        /// </summary>
        bool CanHandle(string filePath);
    }
}