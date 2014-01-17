using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Nandaka.Common
{
    [ValueConversion(typeof(List<string>), typeof(string))]
    public class ListToStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
                throw new InvalidOperationException("The target must be a String");

            var delimiter = ", ";
            if (parameter != null && parameter.GetType() == typeof(string))
            {
                if (!String.IsNullOrWhiteSpace(parameter as string))
                    delimiter = parameter as string;
            }
            return String.Join(delimiter, ((List<string>)value).ToArray());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(List<string>))
                throw new InvalidOperationException("The target must be a List<string>");

            string delimiter = ", ";
            if (parameter != null && parameter.GetType() == typeof(string))
            {
                if (!String.IsNullOrWhiteSpace(parameter as string))
                    delimiter = parameter as string;
            }
            
            var temp = value as string;
            var result = new List<string>();
            if (!string.IsNullOrWhiteSpace(temp))
            {
                result = temp.Split(new string[] { delimiter }, StringSplitOptions.None).ToList<string>();
            }
            return result;
        }
    }
}
