using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedActivityManager.Enums;
using SQLite;

namespace SharedActivityManager.Models
{
    public class Activity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public ActivityType TypeId { get; set; }
        public string? Title { get; set; }
        public string? Desc { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StartDate { get; set; }
        public bool AlarmSet { get; set; }
        public bool isCompleted { get; set; }
        public ReminderType ReminderTypeId { get; set; }
        public DateTime NextReminderDate { get; set; }
        public string? RingTone { get; set; }

    }
}
