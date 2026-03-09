// Factories/WorkActivityCreator.cs
using System.Text;
using SharedActivityManager.Abstracts;
using SharedActivityManager.Enums;
using SharedActivityManager.Models;

namespace SharedActivityManager.Factories
{
    public class WorkActivityCreator : ActivityCreator
    {
        public override Activity CreateActivity()
        {
            return new Activity
            {
                TypeId = ActivityType.Work
            };
        }

        protected override void ConfigureSpecificProperties(Activity activity, Dictionary<string, object> additionalParams)
        {
            // Poți salva aceste date în Desc sau în câmpuri dedicate
            // De exemplu, poți adăuga în Desc informațiile suplimentare
            var sb = new StringBuilder(activity.Desc ?? "");

            if (additionalParams?.ContainsKey("Priority") == true)
            {
                sb.Append($" [Priority: {additionalParams["Priority"]}]");
            }
            if (additionalParams?.ContainsKey("ProjectName") == true)
            {
                sb.Append($" [Project: {additionalParams["ProjectName"]}]");
            }

            activity.Desc = sb.ToString();
        }

        protected override ReminderType GetDefaultReminderType()
        {
            return ReminderType.Daily; // Work activities need daily reminders
        }
    }
}