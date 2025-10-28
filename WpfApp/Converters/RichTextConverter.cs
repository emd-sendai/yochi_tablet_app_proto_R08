using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;

namespace WpfApp.Converters
{
    public class RichTextConverter : IValueConverter
    {

        private static readonly Regex BoldRegex = new Regex(@"<b>(.*?)</b>", RegexOptions.IgnoreCase);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value as string ?? "";
            var inlines = new List<Inline>();

            string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                // 各行の先頭に1マス空白
                inlines.Add(new Run(" "));

                int lastIndex = 0;
                foreach (Match match in BoldRegex.Matches(line))
                {
                    if (match.Index > lastIndex)
                    {
                        inlines.Add(new Run(line.Substring(lastIndex, match.Index - lastIndex)));
                    }

                    inlines.Add(new Run(match.Groups[1].Value) { FontWeight = System.Windows.FontWeights.Bold });
                    lastIndex = match.Index + match.Length;
                }

                if (lastIndex < line.Length)
                    inlines.Add(new Run(line.Substring(lastIndex)));

                inlines.Add(new LineBreak());
            }

            return inlines;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
