using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedActivityManager.Models
{
    public class StudyQuizQuestion
    {
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public int CorrectAnswerIndex { get; set; } = 0;
        public int UserAnswerIndex { get; set; } = -1;
        public bool IsCorrect => UserAnswerIndex == CorrectAnswerIndex;
    }
}
