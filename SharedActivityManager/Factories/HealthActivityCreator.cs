// Factories/HealthActivityCreator.cs
using System.Text;
using SharedActivityManager.Abstracts;
using SharedActivityManager.Enums;
using SharedActivityManager.Models;

namespace SharedActivityManager.Factories
{
    public class HealthActivityCreator : ActivityCreator
    {
        public override Activity CreateActivity()
        {
            return new Activity
            {
                TypeId = ActivityType.Health
            };
        }

        protected override void ConfigureSpecificProperties(Activity activity, Dictionary<string, object> additionalParams)
        {
            var sb = new StringBuilder(activity.Desc ?? "");

            if (additionalParams?.ContainsKey("HealthType") == true)
            {
                sb.Append($" [Type: {additionalParams["HealthType"]}]");
            }
            if (additionalParams?.ContainsKey("DurationMinutes") == true)
            {
                sb.Append($" [{additionalParams["DurationMinutes"]} min]");
            }

            activity.Desc = sb.ToString();
        }

        protected override ReminderType GetDefaultReminderType()
        {
            return ReminderType.Daily;
        }
    }
}