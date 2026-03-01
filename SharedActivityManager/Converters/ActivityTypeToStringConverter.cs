using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedActivityManager.Enums;

namespace SharedActivityManager.Converters
{
    public class ActivityTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ActivityType type)
            {
                return type switch
                {
                    ActivityType.Sport => "Sport",
                    ActivityType.Exercise => "Exercise",
                    ActivityType.Study => "Study",
                    ActivityType.Work => "Work",
                    ActivityType.Meeting => "Meeting",
                    ActivityType.Entertainment => "Entertainment",
                    ActivityType.Health => "Health",
                    ActivityType.Family => "Family",
                    ActivityType.Social => "Social",
                    ActivityType.Shopping => "Shopping",
                    ActivityType.Travel => "Travel",
                    ActivityType.Other => "Other",
                    _ => type.ToString()
                };
            }
            return "Other";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
