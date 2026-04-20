using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SharedActivityManager.Models
{
    public class SportActivityData
    {
        public int TimerDurationSeconds { get; set; } = 1800; // 30 minute default
        public int TimerElapsedSeconds { get; set; } = 0;
        public bool IsTimerRunning { get; set; } = false;
        public int CaloriesBurned { get; set; } = 0;
        public double DistanceKm { get; set; } = 0;
        public int Repetitions { get; set; } = 0;
        public int Sets { get; set; } = 0;
        public string WorkoutType { get; set; } = "General"; // Running, Yoga, Gym, Swimming

        public TimeSpan GetRemainingTime() => TimeSpan.FromSeconds(TimerDurationSeconds - TimerElapsedSeconds);
        public double GetProgressPercentage() => TimerDurationSeconds > 0 ? (double)TimerElapsedSeconds / TimerDurationSeconds * 100 : 0;

        public string Serialize() => JsonSerializer.Serialize(this);
        public static SportActivityData Deserialize(string json) => JsonSerializer.Deserialize<SportActivityData>(json) ?? new SportActivityData();
    }
}
