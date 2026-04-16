using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PassOfficeApp
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value as string;

            if (string.IsNullOrEmpty(status))
                return new SolidColorBrush(Colors.Gray);

            switch (status.ToLower())
            {
                case "проверка":
                    return new SolidColorBrush(Color.FromRgb(243, 156, 18)); // #F39C12
                case "одобрена":
                    return new SolidColorBrush(Color.FromRgb(39, 174, 96));   // #27AE60
                case "не одобрена":
                    return new SolidColorBrush(Color.FromRgb(231, 76, 60));   // #E74C3C
                default:
                    return new SolidColorBrush(Color.FromRgb(149, 165, 166)); // #95A5A6
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}