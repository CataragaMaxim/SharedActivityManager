using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Adapters
{
    /// <summary>
    /// Facade care simplifică utilizarea adaptoarelor
    /// </summary>
    public class FileAdapterFacade
    {
        private readonly List<IFileAdapter> _adapters;

        public FileAdapterFacade()
        {
            _adapters = new List<IFileAdapter>
            {
                new CSVAdapter(),
                new JSONAdapter(),
                new XMLAdapter(),
                //new ExcelAdapter()
            };
        }

        /// <summary>
        /// Obține toate formatele suportate
        /// </summary>
        public List<string> GetSupportedFormats()
        {
            return _adapters.Select(a => a.FormatName).ToList();
        }

        /// <summary>
        /// Obține extensiile suportate pentru un format
        /// </summary>
        public string GetFilePickerFilter()
        {
            var filters = new List<string>();
            foreach (var adapter in _adapters)
            {
                filters.Add($"{adapter.FormatName}|*{string.Join(";*", adapter.SupportedExtensions)}");
            }
            filters.Add("All Files|*.*");
            return string.Join("|", filters);
        }

        /// <summary>
        /// Importă activități dintr-un fișier (detectează automat formatul)
        /// </summary>
        public async Task<List<Activity>> ImportFromFileAsync(string filePath)
        {
            var adapter = GetAdapterForFile(filePath);
            if (adapter == null)
                throw new NotSupportedException($"Format not supported for file: {filePath}");

            return await adapter.ImportAsync(filePath);
        }

        /// <summary>
        /// Exportă activități într-un fișier (folosește extensia pentru a determina formatul)
        /// </summary>
        public async Task ExportToFileAsync(string filePath, List<Activity> activities)
        {
            var adapter = GetAdapterForFile(filePath);
            if (adapter == null)
                throw new NotSupportedException($"Format not supported for file: {filePath}");

            await adapter.ExportAsync(filePath, activities);
        }

        /// <summary>
        /// Exportă activități într-un format specific
        /// </summary>
        public async Task ExportToFormatAsync(string filePath, List<Activity> activities, string formatName)
        {
            var adapter = _adapters.FirstOrDefault(a =>
                a.FormatName.Equals(formatName, StringComparison.OrdinalIgnoreCase));

            if (adapter == null)
                throw new NotSupportedException($"Format {formatName} not supported");

            // Asigură extensia corectă
            var extension = adapter.SupportedExtensions.FirstOrDefault() ?? ".txt";
            if (!filePath.EndsWith(extension))
                filePath += extension;

            await adapter.ExportAsync(filePath, activities);
        }

        private IFileAdapter GetAdapterForFile(string filePath)
        {
            return _adapters.FirstOrDefault(a => a.CanHandle(filePath));
        }

        /// <summary>
        /// Importă activități cu preview (pentru a vedea datele înainte de import)
        /// </summary>
        public async Task<(List<Activity> Activities, string Preview)> ImportWithPreviewAsync(string filePath)
        {
            var activities = await ImportFromFileAsync(filePath);

            var preview = $"Found {activities.Count} activities:\n";
            preview += string.Join("\n", activities.Take(5).Select(a => $"• {a.Title} ({a.TypeId})"));
            if (activities.Count > 5)
                preview += $"\n... and {activities.Count - 5} more";

            return (activities, preview);
        }
    }
}