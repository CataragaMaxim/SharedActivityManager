using SharedActivityManager.Enums;
using SharedActivityManager.Models;

namespace SharedActivityManager.Factories
{
    public class SportActivityCreator : ActivityCreator
    {
        public override Activity CreateActivity()
        {
            return new Activity
            {
                TypeId = ActivityType.Health,
                SpecificDataJson = new SportActivityData().Serialize()
            };
        }

        protected override void ConfigureSpecificProperties(Activity activity, Dictionary<string, object> additionalParams)
        {
            var sportData = SportActivityData.Deserialize(activity.SpecificDataJson);

            sportData.TimerDurationSeconds = GetParamValue(additionalParams, "DurationSeconds", 1800);
            sportData.WorkoutType = GetParamValue(additionalParams, "WorkoutType", "General");
            sportData.Repetitions = GetParamValue(additionalParams, "Repetitions", 0);
            sportData.Sets = GetParamValue(additionalParams, "Sets", 0);

            activity.SpecificDataJson = sportData.Serialize();
        }

        protected override ReminderType GetDefaultReminderType() => ReminderType.Daily;

        // ========== METODE SPECIFICE SPORT ==========

        public SportActivityData GetSportData(Activity activity)
        {
            return SportActivityData.Deserialize(activity.SpecificDataJson);
        }

        public void SaveSportData(Activity activity, SportActivityData data)
        {
            activity.SpecificDataJson = data.Serialize();
        }

        public void StartTimer(Activity activity)
        {
            var data = GetSportData(activity);
            data.IsTimerRunning = true;
            SaveSportData(activity, data);
        }

        public void PauseTimer(Activity activity)
        {
            var data = GetSportData(activity);
            data.IsTimerRunning = false;
            SaveSportData(activity, data);
        }

        public void StopTimer(Activity activity)
        {
            var data = GetSportData(activity);
            data.IsTimerRunning = false;
            data.TimerElapsedSeconds = 0;
            SaveSportData(activity, data);
        }

        public async Task TickTimerAsync(Activity activity)
        {
            var data = GetSportData(activity);

            if (data.IsTimerRunning && data.TimerElapsedSeconds < data.TimerDurationSeconds)
            {
                data.TimerElapsedSeconds++;

                // Calculează calorii aproximative
                data.CaloriesBurned = (int)(data.TimerElapsedSeconds / 60.0 * 8); // ~8 calorii pe minut

                SaveSportData(activity, data);

                if (data.TimerElapsedSeconds >= data.TimerDurationSeconds)
                {
                    activity.IsCompleted = true;
                }
            }

            await Task.CompletedTask;
        }

        public void AddRepetition(Activity activity)
        {
            var data = GetSportData(activity);
            data.Repetitions++;
            SaveSportData(activity, data);
        }

        public void AddDistance(Activity activity, double km)
        {
            var data = GetSportData(activity);
            data.DistanceKm += km;
            SaveSportData(activity, data);
        }

        public TimeSpan GetRemainingTime(Activity activity)
        {
            return GetSportData(activity).GetRemainingTime();
        }

        public double GetProgressPercentage(Activity activity)
        {
            return GetSportData(activity).GetProgressPercentage();
        }

        public string GetFormattedTime(Activity activity)
        {
            var remaining = GetRemainingTime(activity);
            return $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }
    }
}