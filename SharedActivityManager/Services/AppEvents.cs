// Services/AppEvents.cs
using System;

namespace SharedActivityManager.Services
{
    public static class AppEvents
    {
        // Eveniment care se declanșează când activitățile se schimbă
        public static event Action ActivitiesChanged;

        // Metodă pentru a declanșa evenimentul
        public static void OnActivitiesChanged()
        {
            System.Diagnostics.Debug.WriteLine("🔥 AppEvents: ActivitiesChanged triggered");

            // Verifică dacă există abonați
            if (ActivitiesChanged != null)
            {
                System.Diagnostics.Debug.WriteLine($"AppEvents: There are subscribers");
                ActivitiesChanged?.Invoke();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"AppEvents: No subscribers!");
            }
        }
    }
}