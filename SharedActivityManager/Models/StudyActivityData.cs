using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SharedActivityManager.Models
{
    public class StudyActivityData
    {
        public string VideoUrl { get; set; } = string.Empty;
        public double VideoProgress { get; set; } = 0; // 0-100
        public string Notes { get; set; } = string.Empty;
        public List<StudyQuizQuestion> QuizQuestions { get; set; } = new();
        public int CurrentQuizScore { get; set; } = 0;
        public string Subject { get; set; } = string.Empty;
        public string StudyMaterial { get; set; } = string.Empty; // PDF, Video, Article

        public bool IsVideoCompleted => VideoProgress >= 99.9;

        public string Serialize() => JsonSerializer.Serialize(this);
        public static StudyActivityData Deserialize(string json) => JsonSerializer.Deserialize<StudyActivityData>(json) ?? new StudyActivityData();
    }
}
