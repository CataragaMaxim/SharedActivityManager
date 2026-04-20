using SharedActivityManager.Enums;
using SharedActivityManager.Models;

namespace SharedActivityManager.Services
{
    public interface IActivityManagementFacade
    {
        // ========== OPERAȚII COMPLETE ==========

        /// <summary>
        /// Creează o activitate completă (salvare, alarmă, notificare)
        /// </summary>
        Task<Activity> CreateCompleteActivityAsync(
            string title,
            string description,
            ActivityType type,
            DateTime startTime,
            bool alarmSet = true,
            ReminderType reminderType = ReminderType.None,
            string ringTone = "Default Alarm",
            bool isPublic = false,
            Dictionary<string, object> additionalParams = null);

        /// <summary>
        /// Actualizează o activitate existentă
        /// </summary>
        Task UpdateCompleteActivityAsync(Activity activity);

        /// <summary>
        /// Șterge o activitate și toate datele asociate
        /// </summary>
        Task DeleteCompleteActivityAsync(Activity activity);

        /// <summary>
        /// Copiază o activitate (folosind Prototype Pattern)
        /// </summary>
        Task<Activity> CopyActivityAsync(Activity sourceActivity, string newOwnerId = null);

        // ========== OPERAȚII PENTRU STARE ==========

        /// <summary>
        /// Completează o activitate (marchează ca finalizată)
        /// </summary>
        Task CompleteActivityAsync(Activity activity);

        /// <summary>
        /// Reactivează o activitate finalizată
        /// </summary>
        Task ReactivateActivityAsync(Activity activity);

        // ========== OPERAȚII PENTRU ALARME ==========

        /// <summary>
        /// Programează sau reprogramează alarma pentru o activitate
        /// </summary>
        Task RescheduleAlarmAsync(Activity activity);

        /// <summary>
        /// Oprește alarma curentă
        /// </summary>
        Task StopCurrentAlarmAsync();

        // ========== OPERAȚII PENTRU PARTAJARE ==========

        /// <summary>
        /// Partajează o activitate cu alți utilizatori
        /// </summary>
        Task ShareActivityAsync(Activity activity, string targetUserId);

        /// <summary>
        /// Copiază o activitate partajată în colecția personală
        /// </summary>
        Task<Activity> CopySharedActivityAsync(Activity sharedActivity);

        // ========== OPERAȚII PENTRU STATISTICI ==========

        /// <summary>
        /// Obține statistici complete pentru utilizator
        /// </summary>
        Task<UserStatistics> GetUserStatisticsAsync();

        /// <summary>
        /// Obține activitățile pentru o anumită zi
        /// </summary>
        Task<List<Activity>> GetActivitiesForDateAsync(DateTime date);

        /// <summary>
        /// Obține activitățile pentru săptămâna curentă
        /// </summary>
        Task<List<Activity>> GetCurrentWeekActivitiesAsync();
    }

    // Clasă pentru statistici utilizator
    public class UserStatistics
    {
        public int TotalActivities { get; set; }
        public int CompletedActivities { get; set; }
        public int IncompleteActivities { get; set; }
        public int ActivitiesToday { get; set; }
        public int ActivitiesThisWeek { get; set; }
        public double CompletionPercentage { get; set; }
        public Dictionary<ActivityType, int> ActivitiesByType { get; set; } = new();
        public List<Activity> UpcomingActivities { get; set; } = new();
    }
}