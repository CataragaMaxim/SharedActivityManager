using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Enums;
using SharedActivityManager.Factories;
using SharedActivityManager.Models;

namespace SharedActivityManager.Services
{
    public class ActivityManagementFacade : IActivityManagementFacade
    {
        // Subsystem-uri (servicii)
        private readonly IActivityService _activityService;
        private readonly IAlarmService _alarmService;
        private readonly IAudioService _audioService;
        private readonly INotificationService _notificationService;
        private readonly IAlertService _alertService;
        private readonly IMessagingService _messagingService;

        // 🔥 NU MAI AVEM NEVOIE DE _factoryRegistry - e static!

        public ActivityManagementFacade(
            IActivityService activityService,
            IAlarmService alarmService,
            IAudioService audioService,
            INotificationService notificationService,
            IAlertService alertService,
            IMessagingService messagingService)
        {
            _activityService = activityService;
            _alarmService = alarmService;
            _audioService = audioService;
            _notificationService = notificationService;
            _alertService = alertService;
            _messagingService = messagingService;
            // 🔥 Șterge linia: _factoryRegistry = new ActivityFactoryRegistry();
        }

        // ========== IMPLEMENTARE METODE ==========

        /// <summary>
        /// Creează o activitate completă
        /// </summary>
        public async Task<Activity> CreateCompleteActivityAsync(
            string title,
            string description,
            ActivityType type,
            DateTime startTime,
            bool alarmSet = true,
            ReminderType reminderType = ReminderType.None,
            string ringTone = "Default Alarm",
            bool isPublic = false,
            Dictionary<string, object> additionalParams = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== Facade: Creating activity '{title}' of type {type} ===");

                // 🔥 Folosește direct clasa statică, fără instanță
                var creator = ActivityFactoryRegistry.GetCreator(type);
                var activity = await creator.CreateAndConfigureActivity(title, description, startTime, additionalParams);

                // 2. Configurează proprietățile comune
                activity.AlarmSet = alarmSet;
                activity.ReminderType = reminderType;
                activity.RingTone = ringTone;
                activity.IsPublic = isPublic;
                activity.OwnerId = "current_user"; // TODO: din sesiune
                activity.SharedDate = isPublic ? DateTime.Now : default;

                // 3. Salvează în baza de date
                await _activityService.SaveActivityAsync(activity);
                System.Diagnostics.Debug.WriteLine($"Activity saved with ID: {activity.Id}");

                // 4. Programează alarma dacă este cazul
                if (alarmSet && !activity.IsCompleted)
                {
                    await _alarmService.ScheduleAlarmAsync(activity);
                    System.Diagnostics.Debug.WriteLine($"Alarm scheduled for {startTime}");
                }

                // 5. Trimite notificare de succes
                await _notificationService.ShowNotificationAsync(
                    new AppNotification
                    {
                        Title = "Activity Created",
                        Content = $"✅ '{title}' has been created successfully!",
                        Priority = AppNotificationPriority.Normal
                    });

                // 6. Notifică alte părți ale aplicației
                _messagingService.Send(new ActivitiesChangedMessage
                {
                    Action = "Added",
                    Activity = activity
                });

                System.Diagnostics.Debug.WriteLine($"=== Facade: Activity created successfully ===");
                return activity;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Facade: Error creating activity - {ex.Message}");
                await _alertService.ShowAlertAsync("Error", $"Failed to create activity: {ex.Message}");
                throw;
            }
        }

        // Restul metodelor rămân la fel...
        // (UpdateCompleteActivityAsync, DeleteCompleteActivityAsync, CopyActivityAsync,
        //  CompleteActivityAsync, ReactivateActivityAsync, RescheduleAlarmAsync,
        //  StopCurrentAlarmAsync, ShareActivityAsync, CopySharedActivityAsync,
        //  GetUserStatisticsAsync, GetActivitiesForDateAsync, GetCurrentWeekActivitiesAsync)

    /// <summary>
    /// Actualizează o activitate existentă
    /// </summary>
    public async Task UpdateCompleteActivityAsync(Activity activity)
    {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== Facade: Updating activity ID {activity.Id} ===");

                // 1. Anulează alarma veche
                await _alarmService.CancelAlarmAsync(activity.Id);

                // 2. Salvează modificările
                await _activityService.SaveActivityAsync(activity);

                // 3. Reprogramează alarma dacă este necesar
                if (activity.AlarmSet && !activity.IsCompleted)
                {
                    await _alarmService.ScheduleAlarmAsync(activity);
                }

                // 4. Trimite notificare
                await _notificationService.ShowNotificationAsync(
                    new AppNotification
                    {
                        Title = "Activity Updated",
                        Content = $"✏️ '{activity.Title}' has been updated!",
                        Priority = AppNotificationPriority.Low
                    });

                // 5. Notifică aplicația
                _messagingService.Send(new ActivitiesChangedMessage
                {
                    Action = "Updated",
                    Activity = activity
                });

                System.Diagnostics.Debug.WriteLine($"=== Facade: Activity updated successfully ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Facade: Error updating activity - {ex.Message}");
                await _alertService.ShowAlertAsync("Error", $"Failed to update activity: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Șterge o activitate
        /// </summary>
        public async Task DeleteCompleteActivityAsync(Activity activity)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== Facade: Deleting activity ID {activity.Id} ===");

                // 1. Anulează alarma
                await _alarmService.CancelAlarmAsync(activity.Id);

                // 2. Șterge din baza de date
                await _activityService.DeleteActivityAsync(activity);

                // 3. Trimite notificare
                await _notificationService.ShowNotificationAsync(
                    new AppNotification
                    {
                        Title = "Activity Deleted",
                        Content = $"🗑️ '{activity.Title}' has been deleted!",
                        Priority = AppNotificationPriority.Low
                    });

                // 4. Notifică aplicația
                _messagingService.Send(new ActivitiesChangedMessage
                {
                    Action = "Deleted",
                    Activity = activity
                });

                System.Diagnostics.Debug.WriteLine($"=== Facade: Activity deleted successfully ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Facade: Error deleting activity - {ex.Message}");
                await _alertService.ShowAlertAsync("Error", $"Failed to delete activity: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Copiază o activitate (Prototype Pattern)
        /// </summary>
        public async Task<Activity> CopyActivityAsync(Activity sourceActivity, string newOwnerId = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== Facade: Copying activity '{sourceActivity.Title}' ===");

                // 1. Folosește Prototype Pattern pentru deep copy
                var copy = sourceActivity.DeepCopy();

                // 2. Configurează copia
                copy.OwnerId = newOwnerId ?? "current_user";
                copy.IsPublic = false;
                copy.AlarmSet = false;
                copy.OriginalActivityId = sourceActivity.Id;
                copy.Title = $"{sourceActivity.Title} (Copy)";

                // 3. Salvează copia
                await _activityService.SaveActivityAsync(copy);

                // 4. Notifică aplicația
                _messagingService.Send(new ActivitiesChangedMessage
                {
                    Action = "Copied",
                    Activity = copy
                });

                System.Diagnostics.Debug.WriteLine($"=== Facade: Activity copied with ID {copy.Id} ===");
                return copy;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Facade: Error copying activity - {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Completează o activitate
        /// </summary>
        public async Task CompleteActivityAsync(Activity activity)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== Facade: Completing activity '{activity.Title}' ===");

                // 1. Marchează ca finalizată
                activity.IsCompleted = true;

                // 2. Anulează alarma
                await _alarmService.CancelAlarmAsync(activity.Id);

                // 3. Salvează modificarea
                await _activityService.SaveActivityAsync(activity);

                // 4. Oprește orice redare audio
                await _audioService.StopPlayingAsync();

                // 5. Trimite notificare de felicitare
                await _notificationService.ShowNotificationAsync(
                    new AppNotification
                    {
                        Title = "Congratulations! 🎉",
                        Content = $"You completed '{activity.Title}'! Great job!",
                        Priority = AppNotificationPriority.High
                    });

                // 6. Notifică aplicația
                _messagingService.Send(new ActivitiesChangedMessage
                {
                    Action = "Completed",
                    Activity = activity
                });

                System.Diagnostics.Debug.WriteLine($"=== Facade: Activity completed successfully ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Facade: Error completing activity - {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Reactivează o activitate finalizată
        /// </summary>
        public async Task ReactivateActivityAsync(Activity activity)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== Facade: Reactivating activity '{activity.Title}' ===");

                activity.IsCompleted = false;
                await _activityService.SaveActivityAsync(activity);

                if (activity.AlarmSet)
                {
                    await _alarmService.ScheduleAlarmAsync(activity);
                }

                _messagingService.Send(new ActivitiesChangedMessage
                {
                    Action = "Reactivated",
                    Activity = activity
                });

                System.Diagnostics.Debug.WriteLine($"=== Facade: Activity reactivated ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Facade: Error reactivating activity - {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Reprogramează alarma
        /// </summary>
        public async Task RescheduleAlarmAsync(Activity activity)
        {
            try
            {
                await _alarmService.CancelAlarmAsync(activity.Id);
                if (activity.AlarmSet && !activity.IsCompleted)
                {
                    await _alarmService.ScheduleAlarmAsync(activity);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Facade: Error rescheduling alarm - {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Oprește alarma curentă
        /// </summary>
        public async Task StopCurrentAlarmAsync()
        {
            await _alarmService.StopCurrentAlarmAsync();
        }

        /// <summary>
        /// Partajează o activitate
        /// </summary>
        public async Task ShareActivityAsync(Activity activity, string targetUserId)
        {
            try
            {
                activity.IsPublic = true;
                activity.SharedDate = DateTime.Now;
                await _activityService.SaveActivityAsync(activity);

                await _notificationService.ShowNotificationAsync(
                    new AppNotification
                    {
                        Title = "Activity Shared",
                        Content = $"📤 '{activity.Title}' is now shared with others!",
                        Priority = AppNotificationPriority.Normal
                    });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Facade: Error sharing activity - {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Copiază o activitate partajată
        /// </summary>
        public async Task<Activity> CopySharedActivityAsync(Activity sharedActivity)
        {
            return await CopyActivityAsync(sharedActivity);
        }

        /// <summary>
        /// Obține statistici utilizator
        /// </summary>
        public async Task<UserStatistics> GetUserStatisticsAsync()
        {
            var allActivities = await _activityService.GetActivitiesAsync();
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);

            var stats = new UserStatistics
            {
                TotalActivities = allActivities.Count,
                CompletedActivities = allActivities.Count(a => a.IsCompleted),
                IncompleteActivities = allActivities.Count(a => !a.IsCompleted),
                ActivitiesToday = allActivities.Count(a => a.StartDate.Date == today),
                ActivitiesThisWeek = allActivities.Count(a => a.StartDate >= startOfWeek),
                UpcomingActivities = allActivities.Where(a => !a.IsCompleted && a.StartDate >= today)
                                       .OrderBy(a => a.StartDate)
                                       .Take(5)
                                       .ToList()
            };

            stats.CompletionPercentage = stats.TotalActivities > 0
                ? (double)stats.CompletedActivities / stats.TotalActivities * 100
                : 0;

            foreach (var type in Enum.GetValues<ActivityType>())
            {
                stats.ActivitiesByType[type] = allActivities.Count(a => a.TypeId == type);
            }

            return stats;
        }

        /// <summary>
        /// Obține activitățile pentru o zi
        /// </summary>
        public async Task<List<Activity>> GetActivitiesForDateAsync(DateTime date)
        {
            var allActivities = await _activityService.GetActivitiesAsync();
            return allActivities.Where(a => a.StartDate.Date == date.Date).ToList();
        }

        /// <summary>
        /// Obține activitățile pentru săptămâna curentă
        /// </summary>
        public async Task<List<Activity>> GetCurrentWeekActivitiesAsync()
        {
            var allActivities = await _activityService.GetActivitiesAsync();
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            return allActivities.Where(a => a.StartDate >= startOfWeek && a.StartDate < endOfWeek)
                               .OrderBy(a => a.StartDate)
                               .ToList();
        }
    }
}