using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedActivityManager.Abstracts.Platforms;
using SharedActivityManager.Models;

namespace SharedActivityManager.Services
{
    public class WindowsAlarmServices : IAlarmService
    {
        public Task CancelAlarmAsync(int activityId)
        {
            throw new NotImplementedException();
        }

        public Task CancelAllAlarmsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasScheduledAlarmAsync(int activityId)
        {
            throw new NotImplementedException();
        }

        public Task RestoreAlarmsAsync(List<Activity> activities)
        {
            throw new NotImplementedException();
        }

        public Task ScheduleAlarmAsync(Activity activity)
        {
            throw new NotImplementedException();
        }

        public Task StopCurrentAlarmAsync()
        {
            throw new NotImplementedException();
        }

        public Task TriggerAlarmAsync(Activity activity)
        {
            throw new NotImplementedException();
        }
    }
}
