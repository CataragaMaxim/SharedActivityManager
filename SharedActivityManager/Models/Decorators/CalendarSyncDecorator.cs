namespace SharedActivityManager.Models.Decorators
{
    /// <summary>
    /// Decorator pentru sincronizare cu calendar
    /// </summary>
    public class CalendarSyncDecorator : ActivityDecorator
    {
        public CalendarSyncDecorator(IActivityExtra inner) : base(inner)
        {
        }

        public override string Name => "Calendar Sync";
        public override bool IsEnabled => true;

        public override string GetDescription()
        {
            return $"{_inner.GetDescription()} + 📅 Sync to Calendar";
        }

        public override int GetExtraCost()
        {
            return _inner.GetExtraCost() + 3; // 3 minute pentru sincronizare calendar
        }

        public override string GetIcon()
        {
            return "📅";
        }

        public override async Task ExecuteAsync(Activity activity)
        {
            await _inner.ExecuteAsync(activity);

            // Simulare sincronizare calendar
            System.Diagnostics.Debug.WriteLine($"📅 Syncing to calendar: '{activity.Title}' on {activity.StartDate:d}");
            await Task.CompletedTask;
        }
    }
}