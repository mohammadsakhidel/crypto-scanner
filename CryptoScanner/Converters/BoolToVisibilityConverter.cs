using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CryptoScanner.Converters {
    public class BoolToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var v = (bool)value;
            var reverse = parameter != null && System.Convert.ToBoolean(parameter);

            var visibility = v 
                ? (reverse ? Visibility.Collapsed : Visibility.Visible) 
                : (reverse ? Visibility.Visible : Visibility.Collapsed);

            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
