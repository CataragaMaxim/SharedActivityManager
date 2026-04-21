namespace SharedActivityManager.Models.Decorators
{
    /// <summary>
    /// Decorator pentru atașamente
    /// </summary>
    public class AttachmentDecorator : ActivityDecorator
    {
        private readonly List<string> _attachments;

        public AttachmentDecorator(IActivityExtra inner) : base(inner)
        {
            _attachments = new List<string>();
        }

        public override string Name => "Attachments";
        public override bool IsEnabled => _attachments.Any();

        public void AddAttachment(string filePath)
        {
            _attachments.Add(filePath);
        }

        public List<string> GetAttachments() => _attachments;

        public override string GetDescription()
        {
            string attachmentInfo = _attachments.Any() ? $" ({_attachments.Count} files)" : "";
            return $"{_inner.GetDescription()} + 📎 Attachments{attachmentInfo}";
        }

        public override int GetExtraCost()
        {
            return _inner.GetExtraCost() + _attachments.Count; // 1 minut per atașament
        }

        public override string GetIcon()
        {
            return "📎";
        }

        public override async Task ExecuteAsync(Activity activity)
        {
            await _inner.ExecuteAsync(activity);

            // Simulare salvare atașamente
            foreach (var att in _attachments)
            {
                System.Diagnostics.Debug.WriteLine($"📎 Saving attachment: {att}");
            }
        }
    }
}