using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MahAppBase.Converter
{
    public class VisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var isCheck = (bool) value;
                if (isCheck)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
