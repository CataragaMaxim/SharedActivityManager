// Platforms/Windows/BackgroundTask.cs
#if WINDOWS
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using SharedActivityManager.Models;
using SharedActivityManager.Services;

namespace SharedActivityManager.Platforms.Windows
{
    public sealed class BackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            try
            {
                // Procesează notificarea
                var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
                if (details != null)
                {
                    string arguments = details.Argument;
                    var userInput = details.UserInput;

                    System.Diagnostics.Debug.WriteLine($"Background task triggered: {arguments}");

                    // Aici poți procesa acțiunile (Stop, Snooze)
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Background task error: {ex.Message}");
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}
#endif