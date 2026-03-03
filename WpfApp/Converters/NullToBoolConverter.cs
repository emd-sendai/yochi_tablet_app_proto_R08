using System.Globalization;
using System.Windows.Data;

namespace WpfApp.Converters
{
    /// <summary>
    /// オブジェクトの状態をbool型(true/false)の変数に変換するコンバータクラス。
    /// オブジェクトがnullでなければtrue、nullであればfalseを返す。
    /// </summary>
    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value != null;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

}
