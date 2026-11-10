using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfApp.Converters
{
    public class SelectedModeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int selected && int.TryParse(parameter?.ToString(), out int target))
            {
                return selected == target;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();

            /*
            if (value is true && int.TryParse(parameter?.ToString(), out int target))
            {
                return target;
            }
            return Binding.DoNothing;
            */
        }
    }
}
