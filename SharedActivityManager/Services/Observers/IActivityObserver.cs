using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Observers
{
    /// <summary>
    /// Interfața Observer - toate clasele care vor fi notificate trebuie să o implementeze
    /// </summary>
    public interface IActivityObserver
    {
        /// <summary>
        /// Metodă apelată când activitățile se schimbă
        /// </summary>
        /// <param name="action">Acțiunea executată (Added, Updated, Deleted, Imported, etc.)</param>
        /// <param name="activity">Activitatea afectată (opțional)</param>
        /// <param name="activityCount">Numărul de activități afectate (pentru Import/DeleteAll)</param>
        Task OnActivityChanged(string action, Activity activity = null, int activityCount = 0);
    }
}