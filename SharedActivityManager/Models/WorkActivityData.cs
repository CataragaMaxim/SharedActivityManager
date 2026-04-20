using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SharedActivityManager.Models
{
    public class WorkActivityData
    {
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
        public string ProjectName { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        public string Assignee { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public int EstimatedHours { get; set; } = 1;
        public int LoggedHours { get; set; } = 0;

        public bool IsOverdue => Deadline.HasValue && Deadline.Value < DateTime.Now;
        public double GetProgressPercentage() => EstimatedHours > 0 ? (double)LoggedHours / EstimatedHours * 100 : 0;

        public string Serialize() => JsonSerializer.Serialize(this);
        public static WorkActivityData Deserialize(string json) => JsonSerializer.Deserialize<WorkActivityData>(json) ?? new WorkActivityData();
    }
}
