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
                TypeId = ActivityType.Work,
                SpecificDataJson = new WorkActivityData().Serialize()
            };
        }

        protected override void ConfigureSpecificProperties(Activity activity, Dictionary<string, object> additionalParams)
        {
            var workData = WorkActivityData.Deserialize(activity.SpecificDataJson);

            workData.Priority = GetParamValue(additionalParams, "Priority", "Medium");
            workData.ProjectName = GetParamValue(additionalParams, "ProjectName", "");
            workData.Deadline = GetParamValue(additionalParams, "Deadline", (DateTime?)null);
            workData.Assignee = GetParamValue(additionalParams, "Assignee", "");
            workData.EstimatedHours = GetParamValue(additionalParams, "EstimatedHours", 1);

            activity.SpecificDataJson = workData.Serialize();
        }

        protected override ReminderType GetDefaultReminderType() => ReminderType.Daily;

        // ========== METODE SPECIFICE WORK ==========

        public WorkActivityData GetWorkData(Activity activity)
        {
            return WorkActivityData.Deserialize(activity.SpecificDataJson);
        }

        public void SaveWorkData(Activity activity, WorkActivityData data)
        {
            activity.SpecificDataJson = data.Serialize();
        }

        public void SetPriority(Activity activity, string priority)
        {
            var data = GetWorkData(activity);
            data.Priority = priority;
            SaveWorkData(activity, data);
        }

        public void LogHours(Activity activity, int hours)
        {
            var data = GetWorkData(activity);
            data.LoggedHours += hours;

            if (data.LoggedHours >= data.EstimatedHours)
            {
                activity.IsCompleted = true;
            }

            SaveWorkData(activity, data);
        }

        public void AddTag(Activity activity, string tag)
        {
            var data = GetWorkData(activity);
            if (!data.Tags.Contains(tag))
                data.Tags.Add(tag);
            SaveWorkData(activity, data);
        }

        public void RemoveTag(Activity activity, string tag)
        {
            var data = GetWorkData(activity);
            data.Tags.Remove(tag);
            SaveWorkData(activity, data);
        }

        public bool IsOverdue(Activity activity)
        {
            return GetWorkData(activity).IsOverdue;
        }

        public double GetProgressPercentage(Activity activity)
        {
            return GetWorkData(activity).GetProgressPercentage();
        }

        public string GetPriorityColor(string priority)
        {
            return priority switch
            {
                "Critical" => "#D32F2F",
                "High" => "#F44336",
                "Medium" => "#FF9800",
                "Low" => "#4CAF50",
                _ => "#9E9E9E"
            };
        }

        public string GetPriorityIcon(string priority)
        {
            return priority switch
            {
                "Critical" => "🔴",
                "High" => "🟠",
                "Medium" => "🟡",
                "Low" => "🟢",
                _ => "⚪"
            };
        }
    }
}