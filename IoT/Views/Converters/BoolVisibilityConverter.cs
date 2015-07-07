using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace IoT.Views.Converters
{
    public class BoolVisibilityConverter : IValueConverter
    {
        private bool Check<T>(object value)
        {
            if (value == null || value.GetType() != typeof(T))
                return false;
            return true;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!Check<bool>(value))
                return Visibility.Collapsed;

            var bvalue = (bool)value;
            return bvalue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
