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
                TypeId = ActivityType.Study,
                SpecificDataJson = new StudyActivityData().Serialize()
            };
        }

        protected override void ConfigureSpecificProperties(Activity activity, Dictionary<string, object> additionalParams)
        {
            var studyData = StudyActivityData.Deserialize(activity.SpecificDataJson);

            studyData.VideoUrl = GetParamValue(additionalParams, "VideoUrl", "");
            studyData.Subject = GetParamValue(additionalParams, "Subject", "");
            studyData.StudyMaterial = GetParamValue(additionalParams, "StudyMaterial", "");

            activity.SpecificDataJson = studyData.Serialize();
        }

        protected override ReminderType GetDefaultReminderType() => ReminderType.Weekly;

        // ========== METODE SPECIFICE STUDY ==========

        public StudyActivityData GetStudyData(Activity activity)
        {
            return StudyActivityData.Deserialize(activity.SpecificDataJson);
        }

        public void SaveStudyData(Activity activity, StudyActivityData data)
        {
            activity.SpecificDataJson = data.Serialize();
        }

        public void SetVideoUrl(Activity activity, string url)
        {
            var data = GetStudyData(activity);
            data.VideoUrl = url;
            SaveStudyData(activity, data);
        }

        public async Task UpdateVideoProgressAsync(Activity activity, double progress)
        {
            var data = GetStudyData(activity);
            data.VideoProgress = Math.Min(100, Math.Max(0, progress));

            if (data.VideoProgress >= 99.9)
            {
                activity.IsCompleted = true;
            }

            SaveStudyData(activity, data);
            await Task.CompletedTask;
        }

        public void AddNote(Activity activity, string note)
        {
            var data = GetStudyData(activity);
            if (!string.IsNullOrEmpty(data.Notes))
                data.Notes += "\n" + note;
            else
                data.Notes = note;
            SaveStudyData(activity, data);
        }

        public void AddQuizQuestion(Activity activity, StudyQuizQuestion question)
        {
            var data = GetStudyData(activity);
            data.QuizQuestions.Add(question);
            SaveStudyData(activity, data);
        }

        public int SubmitQuizAnswer(Activity activity, int questionIndex, int answerIndex)
        {
            var data = GetStudyData(activity);
            if (questionIndex >= 0 && questionIndex < data.QuizQuestions.Count)
            {
                data.QuizQuestions[questionIndex].UserAnswerIndex = answerIndex;
                if (data.QuizQuestions[questionIndex].IsCorrect)
                {
                    data.CurrentQuizScore++;
                }
                SaveStudyData(activity, data);
            }
            return data.CurrentQuizScore;
        }

        public double GetQuizScorePercentage(Activity activity)
        {
            var data = GetStudyData(activity);
            if (data.QuizQuestions.Count == 0) return 0;
            return (double)data.CurrentQuizScore / data.QuizQuestions.Count * 100;
        }

        public string GetYouTubeEmbedUrl(string url)
        {
            // Convertește URL YouTube în embed format
            if (url.Contains("youtube.com/watch?v="))
            {
                var videoId = url.Split("v=")[1].Split('&')[0];
                return $"https://www.youtube.com/embed/{videoId}";
            }
            return url;
        }
    }
}