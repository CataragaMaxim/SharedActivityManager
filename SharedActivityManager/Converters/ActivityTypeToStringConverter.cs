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
                    ActivityType.Study => "Study",
                    ActivityType.Work => "Work",
                    ActivityType.Health => "Health",
                    ActivityType.Personal => "Personal",
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
