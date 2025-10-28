using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace WpfApp.Helpers
{
    public class RichTextHelper
    {
        public static class RichTextBoxHelper
        {
            private static readonly Regex BoldRegex = new Regex(@"<b>(.*?)</b>", RegexOptions.IgnoreCase);

            public static readonly DependencyProperty RichTextProperty =
                DependencyProperty.RegisterAttached(
                    "RichText",
                    typeof(string),
                    typeof(RichTextHelper),
                    new PropertyMetadata("", OnRichTextChanged));

            public static string GetRichText(DependencyObject obj) => (string)obj.GetValue(RichTextProperty);
            public static void SetRichText(DependencyObject obj, string value) => obj.SetValue(RichTextProperty, value);

            private static void OnRichTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                if (d is TextBlock textBlock)
                {
                    textBlock.Inlines.Clear();
                    string text = e.NewValue as string ?? "";
                    string[] lines = text.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);

                    foreach (var line in lines)
                    {
                        textBlock.Inlines.Add(new Run(" ")); // 行頭の空白

                        int lastIndex = 0;
                        foreach (Match match in BoldRegex.Matches(line))
                        {
                            if (match.Index > lastIndex)
                                textBlock.Inlines.Add(new Run(line.Substring(lastIndex, match.Index - lastIndex)));

                            textBlock.Inlines.Add(new Run(match.Groups[1].Value) { FontWeight = FontWeights.Bold });
                            lastIndex = match.Index + match.Length;
                        }

                        if (lastIndex < line.Length)
                            textBlock.Inlines.Add(new Run(line.Substring(lastIndex)));

                        textBlock.Inlines.Add(new LineBreak());
                    }
                }
            }
        }

    }
}
