using SharedActivityManager.Models;
using SharedActivityManager.Models.Decorators;

namespace SharedActivityManager.Services
{
    /// <summary>
    /// Builder pentru construirea activităților cu funcționalități extra
    /// </summary>
    public class ActivityExtraBuilder
    {
        private IActivityExtra _activityExtra;
        private readonly Activity _activity;

        public ActivityExtraBuilder(Activity activity)
        {
            _activity = activity;
            _activityExtra = new BaseActivityExtra(activity);
        }

        public ActivityExtraBuilder WithNotifications()
        {
            _activityExtra = new NotificationDecorator(_activityExtra);
            return this;
        }

        public ActivityExtraBuilder WithEmailReminder(string emailAddress)
        {
            if (!string.IsNullOrEmpty(emailAddress))
            {
                _activityExtra = new EmailReminderDecorator(_activityExtra, emailAddress);
            }
            return this;
        }

        public ActivityExtraBuilder WithCalendarSync()
        {
            _activityExtra = new CalendarSyncDecorator(_activityExtra);
            return this;
        }

        public ActivityExtraBuilder WithGpsTracking()
        {
            _activityExtra = new GpsTrackingDecorator(_activityExtra);
            return this;
        }

        public ActivityExtraBuilder WithAttachment(string filePath)
        {
            // Verifică dacă există deja AttachmentDecorator
            var existing = FindAttachmentDecorator(_activityExtra);
            if (existing != null)
            {
                existing.AddAttachment(filePath);
            }
            else
            {
                var attachmentDecorator = new AttachmentDecorator(_activityExtra);
                attachmentDecorator.AddAttachment(filePath);
                _activityExtra = attachmentDecorator;
            }
            return this;
        }

        // 🔥 METODĂ MODIFICATĂ - folosește proprietatea Inner
        private AttachmentDecorator FindAttachmentDecorator(IActivityExtra extra)
        {
            if (extra is ActivityDecorator activityDecorator)
                return activityDecorator.FindDecorator<AttachmentDecorator>();

            return null;
        }

        public IActivityExtra Build()
        {
            return _activityExtra;
        }

        public async Task ExecuteAsync()
        {
            await _activityExtra.ExecuteAsync(_activity);
        }

        public string GetFullDescription()
        {
            return _activityExtra.GetDescription();
        }

        public int GetTotalExtraCost()
        {
            return _activityExtra.GetExtraCost();
        }

        public string GetIcon()
        {
            return _activityExtra.GetIcon();
        }
    }
}