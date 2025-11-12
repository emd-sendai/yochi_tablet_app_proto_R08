using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

            // 各セクション
            foreach (var section in page.Sections)
            {
                //bool hasSubtitle = false;
                bool hasBody = false;
                bool hasImage = false;

                var subtitleRow = new StackPanel { Orientation = Orientation.Horizontal };

                if (section.Subtitle != null)
                    subtitleRow.Children.Add(CreateTextBlock(section.Subtitle));

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

                    subtitleRow.Children.Add(cb);
                }

                MainPanel.Children.Add(subtitleRow);

                if (section.Body != null)
                {
                    /*
                    var body = CreateTextBlock(section.Body);
                    body.Margin = new Thickness(20, 4, 0, 8);
                    MainPanel.Children.Add(body);
                    */
                    hasBody = true;
                }

                if (!string.IsNullOrWhiteSpace(section.ImagePath) && File.Exists(section.ImagePath))
                {
                    /*
                    try
                    {
                        var img = new Image
                        {
                            Source = new BitmapImage(new Uri(section.ImagePath)),
                            MaxWidth = 200,
                            MaxHeight = 200,
                            Stretch = Stretch.Uniform,
                            StretchDirection = StretchDirection.DownOnly,
                            Margin = new Thickness(0, 10, 0, 0)
                        };
                        MainPanel.Children.Add(img);
                    */
                        hasImage = true;
                    /*
                        Debug.WriteLine($"[DEBUG] 読み込み完了: {section.ImagePath}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[ERROR] 画像を読み込むことができません: {ex}");
                    }
                    */
                }

                // テキストと画像
                if (hasBody && hasImage)
                {
                    var bodyRow = new StackPanel { Orientation = Orientation.Horizontal };
                    // テキスト
                    if (section.Body != null)
                    {
                        var body = CreateTextBlock(section.Body);
                        body.Margin = new Thickness(20, 4, 0, 8);
                        body.MaxWidth = 490;
                        bodyRow.Children.Add(body);
                    }
                    // 画像
                    if (!string.IsNullOrWhiteSpace(section.ImagePath) && File.Exists(section.ImagePath))
                    {
                        try
                        {
                            var img = new Image
                            {
                                Source = new BitmapImage(new Uri(section.ImagePath)),
                                MaxWidth = 200,
                                MaxHeight = 200,
                                Stretch = Stretch.Uniform,
                                StretchDirection = StretchDirection.DownOnly,
                                Margin = new Thickness(10, 0, 0, 0)
                            };
                            bodyRow.Children.Add(img);
                            Debug.WriteLine($"[DEBUG] 読み込み完了: {section.ImagePath}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[ERROR] 画像を読み込むことができません: {ex}");
                        }
                    }
                    MainPanel.Children.Add(bodyRow);
                }
                // テキストのみ
                else if (hasBody)
                {
                    if (section.Body != null)
                    {
                        var body = CreateTextBlock(section.Body);
                        body.Margin = new Thickness(20, 4, 0, 8);
                        MainPanel.Children.Add(body);
                    }
                }
                // 画像のみ
                else
                {
                    if (!string.IsNullOrWhiteSpace(section.ImagePath) && File.Exists(section.ImagePath))
                    {
                        try
                        {
                            var img = new Image
                            {
                                Source = new BitmapImage(new Uri(section.ImagePath)),
                                MaxWidth = 700,
                                MaxHeight = 400,
                                Stretch = Stretch.Uniform,
                                //StretchDirection = StretchDirection.DownOnly,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                                Margin = new Thickness(0, 10, 0, 0)
                            };
                            MainPanel.Children.Add(img);
                            Debug.WriteLine($"[DEBUG] 読み込み完了: {section.ImagePath}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[ERROR] 画像を読み込むことができません: {ex}");
                        }
                    }
                }



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
