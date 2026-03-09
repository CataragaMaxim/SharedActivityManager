// Factories/PersonalActivityCreator.cs
using System.Text;
using SharedActivityManager.Abstracts;
using SharedActivityManager.Enums;
using SharedActivityManager.Models;

namespace SharedActivityManager.Factories
{
    public class PersonalActivityCreator : ActivityCreator
    {
        public override Activity CreateActivity()
        {
            return new Activity
            {
                TypeId = ActivityType.Personal
            };
        }

        protected override void ConfigureSpecificProperties(Activity activity, Dictionary<string, object> additionalParams)
        {
            var sb = new StringBuilder(activity.Desc ?? "");

            if (additionalParams?.ContainsKey("Location") == true)
            {
                sb.Append($" [Location: {additionalParams["Location"]}]");
            }
            if (additionalParams?.ContainsKey("Mood") == true)
            {
                sb.Append($" [Mood: {additionalParams["Mood"]}]");
            }

            activity.Desc = sb.ToString();
        }
    }
}