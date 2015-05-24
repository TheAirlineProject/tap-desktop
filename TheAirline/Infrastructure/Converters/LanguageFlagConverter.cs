using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace TheAirline.Infrastructure.Converters
{
    public class LanguageFlagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty((string) value))
            {
                value = "en-us";
            }

            string uri = $"pack://application:,,,/TheAirline.Resources;component/graphics/flags/round/{value}.png";

            return new BitmapImage(new Uri(uri));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}