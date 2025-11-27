using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp.Screens.Aramashi.Models;
using WpfApp.Screens.Aramashi.ViewModels;
using static System.Collections.Specialized.BitVector32;

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

            int cnt = 0;

            // 各セクション
            foreach (var section in page.Sections)
            {
                //bool hasSubtitle = false;
                bool hasBody = false;
                bool hasImage = false;
                bool hasImageButton = false;

                if (section.Subtitle != null) 
                {
                    var subtitleRow = new StackPanel { Orientation = Orientation.Horizontal };
                    // ページ内の２つ目以降の要素である場合、上にMarginを設定する
                    if (cnt > 0)
                    {
                        subtitleRow.Margin = new Thickness(0,10,0,0);
                    }

                    subtitleRow.Children.Add(CreateTextBlock(section.Subtitle));

                    if (section.CheckBox != null)
                    {
                        var cb = new CheckBox
                        {
                            //Content = section.CheckBox.Label,
                            IsChecked = section.CheckBox.IsChecked,
                            Margin = new Thickness(10, 0, 0, 0),
                            Padding = new Thickness(0),
                            VerticalAlignment = VerticalAlignment.Center,
                            //LayoutTransform = new ScaleTransform(1.1, 1.1) 
                            FontSize = section.Subtitle.FontSize,
                        };

                        cb.Checked += (_, __) =>
                            viewModel.UpdateCheckBoxState(page.Id, section.CheckBox.Id, true);
                        cb.Unchecked += (_, __) =>
                            viewModel.UpdateCheckBoxState(page.Id, section.CheckBox.Id, false);

                        subtitleRow.Children.Add(cb);
                    }
                    MainPanel.Children.Add(subtitleRow);
                    cnt++;
                }

                if (section.Body != null)
                {
                    hasBody = true;
                }

                if (!string.IsNullOrWhiteSpace(section.ImagePath) && File.Exists(section.ImagePath))
                {
                    hasImage = true;
                }

                if (section.ImageButton != null)
                {
                    hasImageButton = true;
                }

                // 本文と画像と画像ボタンの場合
                if (hasBody && hasImage && hasImageButton)
                {
                    var bodyRow = new StackPanel { Orientation = Orientation.Horizontal };

                    /* 本文 */
                    if (section.Body != null)
                    {
                        // 本文TextBlockを作成
                        var body = CreateTextBlock(section.Body);
                        body.Margin = new Thickness(20, 4, 0, 8);
                        body.MaxWidth = 470;
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
                                Margin = new Thickness(10, 4, 0, 0)
                            };
                            bodyRow.Children.Add(img);
                            Debug.WriteLine($"[DEBUG] 読み込み完了: {section.ImagePath}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[ERROR] 画像を読み込むことができません: {ex}");
                        }
                    }
                    /* 画像ボタン */
                    if (section.ImageButton != null)
                    {
                        List<StyledImageButton> imgbtns = section.ImageButton.ImageButtons;

                        if (imgbtns.Count > 0)
                        {
                            double maxWidth = section.ImageButton.MaxWidth;
                            double maxHeight = section.ImageButton.MaxHeight;

                            // 行StackPanelを作成
                            StackPanel rowPanel = new StackPanel()
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Center,
                            };

                            foreach (StyledImageButton btn in imgbtns)
                            {
                                // 列StackPanelを作成
                                StackPanel colPanel = new StackPanel()
                                {
                                    Orientation = Orientation.Vertical,
                                    Margin = new Thickness(8, 8, 8, 3),
                                };

                                // ボタンラベルを作成
                                TextBlock label = CreateImageButtonLabel(btn);
                                colPanel.Children.Add(label);

                                // ボタンを作成
                                Button imgbtn = CreateImageButton(btn, maxWidth, maxHeight);
                                imgbtn.Command = viewModel.PreviewImageCommand;

                                colPanel.Children.Add(imgbtn);
                                rowPanel.Children.Add(colPanel);
                            }

                            bodyRow.Children.Add(rowPanel);
                        }
                    }

                    MainPanel.Children.Add(bodyRow);
                    cnt++;
                }
                // 本文と画像の場合
                else if (hasBody && hasImage)
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
                                Margin = new Thickness(10, 4, 0, 0)
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
                    cnt++;
                }
                // 本文と画像ボタンの場合
                else if (hasBody && hasImageButton)
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
                    /* 画像ボタン */
                    if (section.ImageButton != null)
                    {
                        List<StyledImageButton> imgbtns = section.ImageButton.ImageButtons;

                        if (imgbtns.Count > 0)
                        {
                            double maxWidth = section.ImageButton.MaxWidth;
                            double maxHeight = section.ImageButton.MaxHeight;

                            // 行StackPanelを作成
                            StackPanel rowPanel = new StackPanel()
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Center,
                            };

                            foreach (StyledImageButton btn in imgbtns)
                            {
                                // 列StackPanelを作成
                                StackPanel colPanel = new StackPanel()
                                {
                                    Orientation = Orientation.Vertical,
                                    Margin = new Thickness(8, 8, 8, 3),
                                };

                                // ボタンラベルを作成
                                TextBlock label = CreateImageButtonLabel(btn);
                                colPanel.Children.Add(label);

                                // ボタンを作成
                                Button imgbtn = CreateImageButton(btn, maxWidth, maxHeight);
                                imgbtn.Command = viewModel.PreviewImageCommand;

                                colPanel.Children.Add(imgbtn);
                                rowPanel.Children.Add(colPanel);
                            }

                            bodyRow.Children.Add(rowPanel);
                        }
                    }
                    MainPanel.Children.Add(bodyRow);
                    cnt++;
                }
                // 画像と画像ボタンの場合
                else if (hasImage && hasImageButton)
                {
                    var bodyRow = new StackPanel { Orientation = Orientation.Horizontal };

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
                                Margin = new Thickness(10, 4, 0, 0)
                            };
                            bodyRow.Children.Add(img);
                            Debug.WriteLine($"[DEBUG] 読み込み完了: {section.ImagePath}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[ERROR] 画像を読み込むことができません: {ex}");
                        }
                    }
                    /* 画像ボタン */
                    if (section.ImageButton != null)
                    {
                        List<StyledImageButton> imgbtns = section.ImageButton.ImageButtons;

                        if (imgbtns.Count > 0)
                        {
                            double maxWidth = section.ImageButton.MaxWidth;
                            double maxHeight = section.ImageButton.MaxHeight;

                            // 行StackPanelを作成
                            StackPanel rowPanel = new StackPanel()
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Center,
                            };

                            foreach (StyledImageButton btn in imgbtns)
                            {
                                // 列StackPanelを作成
                                StackPanel colPanel = new StackPanel()
                                {
                                    Orientation = Orientation.Vertical,
                                    Margin = new Thickness(8, 8, 8, 3),
                                };

                                // ボタンラベルを作成
                                TextBlock label = CreateImageButtonLabel(btn);
                                colPanel.Children.Add(label);

                                // ボタンを作成
                                Button imgbtn = CreateImageButton(btn, maxWidth, maxHeight);
                                imgbtn.Command = viewModel.PreviewImageCommand;

                                colPanel.Children.Add(imgbtn);
                                rowPanel.Children.Add(colPanel);
                            }

                            bodyRow.Children.Add(rowPanel);
                        }
                    }
                    MainPanel.Children.Add(bodyRow);
                    cnt++;
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
                        cnt++;
                    }
                }
                // 画像のみの場合
                else if(hasImage)
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
                            cnt++;
                            Debug.WriteLine($"[DEBUG] 読み込み完了: {section.ImagePath}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[ERROR] 画像を読み込むことができません: {ex}");
                        }
                    }
                }
                // 画像ボタンのみの場合
                else if (hasImageButton)
                {
                    if (section.ImageButton != null)
                    {
                        List<StyledImageButton> imgbtns = section.ImageButton.ImageButtons;

                        if (imgbtns.Count > 0)
                        {
                            double maxWidth = section.ImageButton.MaxWidth;
                            double maxHeight = section.ImageButton.MaxHeight;

                            // 行StackPanelを作成
                            StackPanel rowPanel = new StackPanel()
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Center,
                            };

                            foreach (StyledImageButton btn in imgbtns)
                            {
                                // 列StackPanelを作成
                                StackPanel colPanel = new StackPanel()
                                {
                                    Orientation = Orientation.Vertical,
                                    Margin = new Thickness(8, 8, 8, 3),
                                };

                                // ボタンラベルを作成
                                TextBlock label = CreateImageButtonLabel(btn);
                                colPanel.Children.Add(label);

                                // ボタンを作成
                                Button imgbtn = CreateImageButton(btn, maxWidth, maxHeight);
                                imgbtn.Command = viewModel.PreviewImageCommand;

                                colPanel.Children.Add(imgbtn);
                                rowPanel.Children.Add(colPanel);
                            }

                            MainPanel.Children.Add(rowPanel);
                            cnt++;
                        }
                    }
                }

                // 署名メッセージと署名ボタンが設定されている場合
                if(section.SignTarget != null && section.SignMessage !=  null && section.SignButton != null)
                {
                    /* 署名対象 */
                    var signTarget = CreateTextBlock(section.SignTarget);
                    signTarget.Margin = new Thickness(20, 40, 0, 0);
                    MainPanel.Children.Add(signTarget);

                    /* 署名メッセージ */
                    // 現在の日付（和暦）を取得
                    var culture = new CultureInfo("ja-JP", false);
                    culture.DateTimeFormat.Calendar = new JapaneseCalendar();
                    string wareki = DateTime.Now.ToString("ggy年M月d日", culture);
                    
                    string editedSignMessage = wareki + "上記についてすべて説明を受けました。";
                    Debug.WriteLine($"[DEBUG] 説明完了年月日: {wareki}");

                    // 署名メッセージに現在の年月日を追加
                    section.SignMessage.SetPlainText(editedSignMessage, true, "#D32F2F");

                    // 署名メッセージTextBlockを作成
                    var signMessage = CreateTextBlock(section.SignMessage);
                    signMessage.Margin = new Thickness(20, 20, 0, 0);
                    signMessage.FontSize = 20;
                    MainPanel.Children.Add(signMessage);

                    /* 署名ボタン */
                    var button = CreateButton(section.SignButton);
                    button.HorizontalAlignment = HorizontalAlignment.Right;
                    button.Command = viewModel.EditSignatureCommand; // 一般ボタンの場合はCommandNameで判定して該当するCommandを設定する
                    button.CommandParameter = editedSignMessage;
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

        private TextBlock CreateImageButtonLabel(StyledImageButton data)
        {
            var color = (Color)ColorConverter.ConvertFromString(data.LabelColor);
            var brush = new SolidColorBrush(color);

            TextBlock label = new TextBlock()
            {
                Text = data.Label,
                Foreground = brush,
            };
            return label;
        }

        private Button CreateImageButton(StyledImageButton data, double maxWidth, double maxHeight)
        {
            // ボタンを作成
            Button imgbtn = new Button()
            {
                CommandParameter = data.ImagePath,
            };

            if (Application.Current.TryFindResource("ImageButtonStyle") is Style style)
            {
                imgbtn.Style = style;
            }

            // 画像データを作成
            if (!string.IsNullOrWhiteSpace(data.ImagePath) && File.Exists(data.ImagePath))
            {
                try
                {
                    var img = new Image
                    {
                        Source = new BitmapImage(new Uri(data.ImagePath)),
                        MaxWidth = maxWidth,
                        MaxHeight = maxHeight,
                        Stretch = Stretch.Uniform,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    // 画像データをボタンに設定
                    imgbtn.Content = img;

                    Debug.WriteLine($"[DEBUG] 読み込み完了: {data.ImagePath}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] ボタンの画像を読み込むことができません: {ex}");
                }
            }
            return imgbtn;
        }
    }
}
