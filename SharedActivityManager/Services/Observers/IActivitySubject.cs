using SharedActivityManager.Models;

namespace SharedActivityManager.Services.Observers
{
    /// <summary>
    /// Interfața Subject - definește metodele pentru gestionarea observer-ilor
    /// </summary>
    public interface IActivitySubject
    {
        /// <summary>
        /// Adaugă un observer în listă
        /// </summary>
        void Attach(IActivityObserver observer);

        /// <summary>
        /// Elimină un observer din listă
        /// </summary>
        void Detach(IActivityObserver observer);

        /// <summary>
        /// Notifică toți observer-ii despre o schimbare
        /// </summary>
        Task NotifyObservers(string action, Activity activity = null, int activityCount = 0);
    }
}