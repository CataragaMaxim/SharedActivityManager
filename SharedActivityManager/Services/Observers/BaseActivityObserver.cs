using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Observers
{
    /// <summary>
    /// Clasă abstractă pentru observatori - implementare default pentru OnActivityChanged
    /// </summary>
    public abstract class BaseActivityObserver : IActivityObserver
    {
        public virtual async Task OnActivityChanged(string action, Activity activity = null, int activityCount = 0)
        {
            // Implementare default - poate fi suprascrisă
            System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] Received: {action}");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Metodă helper pentru a reîncărca datele pe UI thread
        /// </summary>
        protected async Task ReloadOnUIThread(Func<Task> reloadAction)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await reloadAction();
            });
        }
    }
}