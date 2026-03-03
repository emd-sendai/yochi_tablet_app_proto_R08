using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfApp.Converters
{
    /// <summary>
    /// bool型(true/false)の変数をVisibility(Visible/Hidden)に変換するコンバータクラス。
    /// 変数がtureであればVisible（表示）、falseであればHidden（非表示）を返す。
    /// </summary>
    public class BooleanToHiddenVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value is bool b && b;
            return flag ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}