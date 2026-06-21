using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EnglishLearningApp.Converters
{
    public class MasteryToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double mastery)
            {
                return mastery switch
                {
                    >= 80 => new SolidColorBrush(Colors.Green),
                    >= 50 => new SolidColorBrush(Colors.Orange),
                    _ => new SolidColorBrush(Colors.Red)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MasteryToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double mastery)
            {
                return mastery switch
                {
                    >= 80 => new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                    >= 50 => new SolidColorBrush(Color.FromRgb(255, 152, 0)),
                    _ => new SolidColorBrush(Color.FromRgb(244, 67, 54))
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
