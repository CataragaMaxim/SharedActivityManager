namespace SharedActivityManager.Models.Decorators
{
    /// <summary>
    /// Componenta concretă - activitatea de bază fără extra funcționalități
    /// </summary>
    public class BaseActivityExtra : IActivityExtra
    {
        private readonly Activity _activity;

        public BaseActivityExtra(Activity activity)
        {
            _activity = activity;
        }

        public string Name => "Base Activity";
        public bool IsEnabled => true;

        public string GetDescription()
        {
            return $"{_activity.Title} - {_activity.TypeId}";
        }

        public int GetExtraCost()
        {
            return 0; // Activitatea de bază nu are cost extra
        }

        public string GetIcon()
        {
            return "📋";
        }

        public async Task ExecuteAsync(Activity activity)
        {
            // Acțiunea de bază - marcare ca completată
            activity.IsCompleted = true;
            await Task.CompletedTask;
        }
    }
}