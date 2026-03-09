using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SharedActivityManager.Enums;
using SharedActivityManager.Models;

namespace SharedActivityManager.Factories
{
    public class StudyActivityCreator : ActivityCreator
    {
        public override Activity CreateActivity()
        {
            return new Activity
            {
                TypeId = ActivityType.Study
            };
        }

        protected override void ConfigureSpecificProperties(Activity activity, Dictionary<string, object> additionalParams)
        {
            var sb = new StringBuilder(activity.Desc ?? "");

            if (additionalParams?.ContainsKey("Subject") == true)
            {
                sb.Append($" [Subject: {additionalParams["Subject"]}]");
            }
            if (additionalParams?.ContainsKey("Mode") == true)
            {
                sb.Append($" [Mode: {additionalParams["Mode"]}]");
            }

            activity.Desc = sb.ToString();
        }

        protected override ReminderType GetDefaultReminderType()
        {
            return ReminderType.Weekly;
        }
    }
}