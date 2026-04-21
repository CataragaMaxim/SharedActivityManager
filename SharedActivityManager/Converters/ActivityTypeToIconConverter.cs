using System.Globalization;
using SharedActivityManager.Enums;
using SharedActivityManager.Services.Flyweight;

namespace SharedActivityManager.Converters
{
    public class ActivityTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ActivityType type)
            {
                var metadata = type.GetMetadata();
                return metadata.Icon;
            }
            return "📝";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}