using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using NijieDownloader.UI.ViewModel;
using System.Windows;

namespace NijieDownloader.UI
{
    public class JobTypeToVisibilityConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            JobType val = (JobType)value;
            
            if (targetType == typeof(Visibility))
            {
                if (val.ToString() == parameter.ToString())
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsMangaToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isManga = (bool)value;

            if (targetType == typeof(Visibility))
            {
                if (isManga)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
