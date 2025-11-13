using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp.Common.ViewModels;
using WpfApp.Common.Views;
using WpfApp.Interfaces;
using WpfApp.Screens.Admin.ViewModels;
using WpfApp.Screens.Aramashi.Models;
using WpfApp.Screens.Aramashi.ViewModels;
using WpfApp.Services;

namespace WpfApp.Screens.Aramashi.Views
{
    /// <summary>
    /// PresentationPage.xaml の相互作用ロジック
    /// </summary>
    public partial class PresentationPage : UserControl
    {
        //private bool isAllCheckBox = false;
        //private readonly IDialogService _dia;

        public PresentationPage()
        {
            InitializeComponent();
            //_dia = new DialogService();
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
                    hasBody = true;
                }

                if (!string.IsNullOrWhiteSpace(section.ImagePath) && File.Exists(section.ImagePath))
                {
                        hasImage = true;
                }

                // 本文と画像の場合
                if (hasBody && hasImage)
                {
                    var bodyRow = new StackPanel { Orientation = Orientation.Horizontal };
                    /* 本文 */
                    if (section.Body != null)
                    {
                        // 本文TextBlockを作成
                        var body = CreateTextBlock(section.Body);
                        body.Margin = new Thickness(20, 4, 0, 8);
                        body.MaxWidth = 490;
                        bodyRow.Children.Add(body);
                    }
                    /* 画像 */
                    if (!string.IsNullOrWhiteSpace(section.ImagePath) && File.Exists(section.ImagePath))
                    {
                        try
                        {
                            // Imageを作成
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
                // 本文のみの場合
                else if (hasBody)
                {
                    if (section.Body != null)
                    {
                        // 本文TextBlockを作成
                        var body = CreateTextBlock(section.Body);
                        body.Margin = new Thickness(20, 4, 0, 8);
                        MainPanel.Children.Add(body);
                    }
                }
                // 画像のみの場合
                else
                {
                    if (!string.IsNullOrWhiteSpace(section.ImagePath) && File.Exists(section.ImagePath))
                    {
                        try
                        {
                            // Imageを作成
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

                // 署名メッセージと署名ボタンが設定されている場合
                if(section.SignMessage !=  null && section.SignButton != null)
                {
                    /* 署名メッセージ */
                    // 現在の日付（和暦）を取得
                    var culture = new CultureInfo("ja-JP", false);
                    culture.DateTimeFormat.Calendar = new JapaneseCalendar();
                    string wareki = DateTime.Now.ToString("ggy年M月d日", culture);
                    Debug.WriteLine($"[DEBUG] 説明完了年月日: {wareki}");

                    // 署名メッセージに現在の年月日を追加
                    section.SignMessage.SetPlainText(wareki + "上記についてすべて説明を受けました。");

                    // 署名メッセージTextBlockを作成
                    var body = CreateTextBlock(section.SignMessage);
                    body.Margin = new Thickness(20, 4, 0, 8);
                    MainPanel.Children.Add(body);

                    /* 署名ボタン */
                    var button = CreateButton(section.SignButton);
                    button.HorizontalAlignment = HorizontalAlignment.Right;
                    button.Command = viewModel.EditSignatureCommand; // 一般ボタンの場合はCommandNameで判定して該当するCommandを設定する
                    MainPanel.Children.Add(button);

                }

            }
            // ページ初期表示時点のチェック状態を更新
            viewModel.UpdateAllCheckedState(page.Id);
            //isAllCheckBox = viewModel.AllPagesChecked;
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

        private Button CreateButton(ButtonData data)
        {
            var btn = new Button
            {
                //Padding = new Thickness(8, 4, 8, 4),
                Content = data.Text,
                FontSize = data.FontSize,
                FontWeight = data.Bold ? FontWeights.Bold : FontWeights.Normal,
                Height = data.Height,
                Width = data.Width,
                CommandParameter = data.CommandParameter
            };

            if (Application.Current.TryFindResource("CommonButtonStyle") is Style style)
            {
                btn.Style = style;
            }

            return btn;
        }
    }
}
