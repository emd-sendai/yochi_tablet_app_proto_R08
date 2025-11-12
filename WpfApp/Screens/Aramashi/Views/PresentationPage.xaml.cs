using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using WpfApp.Screens.Aramashi.Models;
using WpfApp.Screens.Aramashi.ViewModels;

namespace WpfApp.Screens.Aramashi.Views
{
    /// <summary>
    /// PresentationPage.xaml の相互作用ロジック
    /// </summary>
    public partial class PresentationPage : UserControl
    {
        public PresentationPage()
        {
            InitializeComponent();
        }

        public void ApplyPage(PresentationPageConfig page, PresentationViewerViewModel viewModel)
        {
            MainPanel.Children.Clear();

            // タイトル
            /*
            if (page.Title != null)
            {
                var title = CreateTextBlock(page.Title);
                title.Margin = new Thickness(0, 10, 0, 10);
                MainPanel.Children.Add(title);
            }
            */

            // 各セクション
            foreach (var section in page.Sections)
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal };

                if (section.Subtitle != null)
                    row.Children.Add(CreateTextBlock(section.Subtitle));

                if (section.CheckBox != null)
                {
                    var cb = new CheckBox
                    {
                        Content = section.CheckBox.Label,
                        IsChecked = section.CheckBox.IsChecked,
                        Margin = new Thickness(10, 0, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        //LayoutTransform = new ScaleTransform(1.1, 1.1) 
                    };

                    cb.Checked += (_, __) =>
                        viewModel.UpdateCheckBoxState(page.Id, section.CheckBox.Id, true);
                    cb.Unchecked += (_, __) =>
                        viewModel.UpdateCheckBoxState(page.Id, section.CheckBox.Id, false);

                    row.Children.Add(cb);
                }

                MainPanel.Children.Add(row);

                if (section.Body != null)
                {
                    var body = CreateTextBlock(section.Body);
                    body.Margin = new Thickness(20, 4, 0, 8);
                    MainPanel.Children.Add(body);
                }
            }

            if (!string.IsNullOrWhiteSpace(page.ImagePath) && File.Exists(page.ImagePath))
            {
                var img = new Image
                {
                    Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(page.ImagePath)),
                    MaxWidth = 600,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                MainPanel.Children.Add(img);
            }
        }

        private TextBlock CreateTextBlock(TextBlockData data)
        {
            var tb = new TextBlock { TextWrapping = TextWrapping.Wrap, FontSize = data.FontSize };

            foreach (var runInfo in data.TextRuns)
            {
                var parts = runInfo.Text.Split('\n');
                for (int i = 0; i < parts.Length; i++)
                {
                    var color = (Color)ColorConverter.ConvertFromString(runInfo.Color);
                    var brush = new SolidColorBrush(color);

                    var run = new Run(parts[i])
                    {
                        FontWeight = runInfo.Bold ? FontWeights.Bold : FontWeights.Normal,
                        Foreground = brush
                    };

                    if (runInfo.Underline)
                        run.TextDecorations = TextDecorations.Underline;

                    tb.Inlines.Add(run);

                    if (i < parts.Length - 1)
                        tb.Inlines.Add(new LineBreak());
                }
            }

            return tb;
        }

    }
}
