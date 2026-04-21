namespace SharedActivityManager.Models.Decorators
{
    /// <summary>
    /// Decorator pentru reminder pe email
    /// </summary>
    public class EmailReminderDecorator : ActivityDecorator
    {
        private readonly string _emailAddress;

        public EmailReminderDecorator(IActivityExtra inner, string emailAddress) : base(inner)
        {
            _emailAddress = emailAddress;
        }

        public override string Name => "Email Reminder";
        public override bool IsEnabled => !string.IsNullOrEmpty(_emailAddress);

        public override string GetDescription()
        {
            return $"{_inner.GetDescription()} + 📧 Email to {_emailAddress}";
        }

        public override int GetExtraCost()
        {
            return _inner.GetExtraCost() + 2; // 2 minute pentru configurare email
        }

        public override string GetIcon()
        {
            return "📧";
        }

        public override async Task ExecuteAsync(Activity activity)
        {
            await _inner.ExecuteAsync(activity);

            // Simulare trimitere email
            System.Diagnostics.Debug.WriteLine($"📧 Sending email to {_emailAddress}: Activity '{activity.Title}' completed!");
            await Task.CompletedTask;
        }
    }
}