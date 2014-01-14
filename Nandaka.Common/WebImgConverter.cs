using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.IO;


namespace Nandaka.Common
{
    public class WebImgConverter : IValueConverter
    {
        public object Convert(object value)
        {
            if (value != null)
            {
                var url = value.ToString();
                if (url.StartsWith("//"))
                {
                    url = "http:" + url;
                }

                //ExtendedWebClient client = new ExtendedWebClient();
                //var result = client.DownloadData(url);
                //using (var ms = new MemoryStream(result))
                //{
                //    var image = new BitmapImage();
                //    image.BeginInit();
                //    image.CacheOption = BitmapCacheOption.OnLoad;
                //    image.StreamSource = ms;
                //    image.EndInit();
                //    return image;
                //}
                BitmapImage bi = new BitmapImage(new Uri(url));
                return bi;

            }
            else
            {
                return null;
            }

        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Convert(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Convert(value);
        }
    }
}
