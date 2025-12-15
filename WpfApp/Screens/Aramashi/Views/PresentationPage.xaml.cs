using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

            int cnt = 0;

            // 各セクション
            foreach (var section in page.Sections)
            {
                // 並列描画可能な要素数
                int rowElementCount = 0;

                // サブタイトルが設定されている場合
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
                        var cb = CreateCheckBox(section.CheckBox);
                        cb.Margin = new Thickness(10, 0, 0, 0);
                        cb.Padding = new Thickness(0);
                        cb.FontSize = section.Subtitle.FontSize;

                        cb.Checked += (_, __) =>
                            viewModel.UpdateCheckBoxState(page.Id, section.CheckBox.Id, true);
                        cb.Unchecked += (_, __) =>
                            viewModel.UpdateCheckBoxState(page.Id, section.CheckBox.Id, false);

                        subtitleRow.Children.Add(cb);
                    }
                    MainPanel.Children.Add(subtitleRow);
                    cnt++;
                }

                // 本文が設定されている場合
                if (section.Body != null)
                {
                    rowElementCount++;
                }

                // テーブルが設定されている場合
                if (section.Table != null)
                {
                    rowElementCount++;
                }

                // 画像が設定されている場合
                if (section.Image != null)
                {
                    rowElementCount++;
                }

                // 画像ボタンが設定されている場合
                if (section.ImageButton != null)
                {
                    rowElementCount++;
                }

                // 並列描画可能な要素が２つ以上設定されている場合
                if (rowElementCount > 1)
                {
                    var bodyRow = new StackPanel { Orientation = Orientation.Horizontal };

                    /* 本文 */
                    if (section.Body != null)
                    {
                        // インデントを取得
                        var indent = section.Body.Indent;

                        // 本文TextBlockを作成
                        var body = CreateTextBlock(section.Body);
                        body.Margin = new Thickness(20 + indent * 20, 4, 0, 8);
                        //body.MaxWidth = 470 - indent * 20;
                        bodyRow.Children.Add(body);
                    }

                    /* テーブル */
                    if (section.Table != null)
                    {
                        Grid grid = CreateTableGrid(section.Table, viewModel, page.Id);
                        bodyRow.Children.Add(grid);
                    }

                    /* 画像 */
                    if (section.Image != null)
                    {
                        var images = section.Image.Images;

                        if (images.Count > 0)
                        {
                            // 行StackPanelを作成
                            StackPanel rowPanel = CreateImagePanel(section.Image);
                            bodyRow.Children.Add(rowPanel);
                        }
                    }

                    /* 画像ボタン */
                    if (section.ImageButton != null)
                    {
                        var imgbtns = section.ImageButton.ImageButtons;

                        if (imgbtns.Count > 0)
                        {
                            // 行StackPanelを作成
                            StackPanel rowPanel = CreateImageButtonPanel(section.ImageButton, viewModel);
                            bodyRow.Children.Add(rowPanel);
                        }
                    }

                    MainPanel.Children.Add(bodyRow);
                    cnt++;
                }
                // 並列可能な要素が１つの場合
                else if (rowElementCount == 1)
                {
                    // 本文が設定されている場合
                    if (section.Body != null)
                    {
                        // インデントを取得
                        var indent = section.Body.Indent;

                        // 本文TextBlockを作成
                        var body = CreateTextBlock(section.Body);
                        body.Margin = new Thickness(20 + indent * 20, 4, 0, 8);
                        MainPanel.Children.Add(body);
                        cnt++;
                    }

                    // テーブルが設定されている場合
                    if (section.Table != null)
                    {
                        Grid grid = CreateTableGrid(section.Table, viewModel, page.Id);
                        MainPanel.Children.Add(grid);
                        cnt++;
                    }

                    // 画像が設定されている場合
                    if (section.Image != null)
                    {
                        var images = section.Image.Images;

                        if (images.Count > 0)
                        {
                            // 行StackPanelを作成
                            StackPanel rowPanel = CreateImagePanel(section.Image);
                            MainPanel.Children.Add(rowPanel);
                            cnt++;
                        }
                    }

                    // 画像ボタンが設定されている場合
                    if (section.ImageButton != null)
                    {
                        var imgbtns = section.ImageButton.ImageButtons;

                        if (imgbtns.Count > 0)
                        {
                            // 行StackPanelを作成
                            StackPanel rowPanel = CreateImageButtonPanel(section.ImageButton, viewModel);
                            MainPanel.Children.Add(rowPanel);
                            cnt++;
                        }
                    }
                }

                // ボタン（テキストのみ）が設定されている場合
                if (section.Button != null)
                {
                    List<ButtonData> btns = section.Button.TextButtons;

                    if (btns.Count > 0)
                    {
                        StackPanel rowPanel = CreateButtonPanel(section.Button, viewModel);
                        MainPanel.Children.Add(rowPanel);
                        cnt++;
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
                    var button = CreateButton(section.SignButton, viewModel);
                    button.HorizontalAlignment = HorizontalAlignment.Right;
                    //button.Command = viewModel.EditSignatureCommand;
                    button.CommandParameter = editedSignMessage;
                    MainPanel.Children.Add(button);
                }
            }
            // ページ初期表示時点のチェック状態を更新
            viewModel.UpdateAllCheckedState(page.Id);
        }

        private TextBlock CreateTextBlock(TextBlockData data)
        {
            // デフォルトの背景色（透明）を設定する
            var background = Brushes.Transparent;
            // 透明以外が指定されている場合は設定を更新する
            if (data.BackgroundColor != "Transparent")
            {
                var color = (Color)ColorConverter.ConvertFromString(data.BackgroundColor);
                background = new SolidColorBrush(color);
            }

            var tb = new TextBlock 
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = data.FontSize,
                MaxWidth = data.MaxWidth,
                Background = background,
            };

            if (data.HorizontalAlignment == "center")
            {
                tb.HorizontalAlignment = HorizontalAlignment.Center;
            }

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

        private Button CreateButton(ButtonData data, PresentationViewerViewModel viewModel)
        {
            var btn = new Button
            {
                Content = data.Text,
                FontSize = data.FontSize,
                FontWeight = data.Bold ? FontWeights.Bold : FontWeights.Normal,
                Height = data.Height,
                Width = data.Width,
                CommandParameter = data.CommandParameter
            };

            // スタイルを設定
            if (data.Style == "CommonButtonStyle" && Application.Current.TryFindResource("CommonButtonStyle") is Style commonStyle)
            {
                btn.Style = commonStyle;
            }
            else if (data.Style == "TableButtonStyle" && Application.Current.TryFindResource("TableButtonStyle") is Style flatStyle)
            {
                var text = data.Text;
                var textBlock = new TextBlock
                {
                    Text = text,
                    TextDecorations = TextDecorations.Underline,
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                };
                btn.Content = textBlock;
                btn.Style = flatStyle;
            }

            // コマンドと引数を設定
            string? param = data.CommandParameter as string;

            if (data.CommandName == "EditSignatureCommand")
            {
                btn.Command = viewModel.EditSignatureCommand;
            }
            else if (data.CommandName == "ShowInheritanceOverviewCommand")
            {
                btn.Command = viewModel.ShowInheritanceOverviewCommand;
            }
            else if (data.CommandName == "GoToSlideCommand")
            {
                if (!string.IsNullOrWhiteSpace(param) && int.TryParse(param, out int value))
                {
                    btn.CommandParameter = value;
                }
                else
                {
                    Debug.WriteLine("CommandParameter は数値ではありません");
                    btn.CommandParameter = 0;    // 最初のページに遷移
                }
                btn.Command = viewModel.GoToSlideCommand;
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
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            return label;
        }

        private Button CreateImageButton(StyledImageButton data, double maxWidth, double maxHeight, PresentationViewerViewModel viewModel)
        {
            // ボタンを作成
            Button imgbtn = new Button()
            {
                CommandParameter = data.ImagePath,
                Command = viewModel.PreviewImageCommand,
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

        private TextBlock CreateImageLabel(StyledImage data)
        {
            var color = (Color)ColorConverter.ConvertFromString(data.LabelColor);
            var brush = new SolidColorBrush(color);

            TextBlock label = new TextBlock()
            {
                Text = data.Label,
                Foreground = brush,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            return label;
        }

        private Image CreateImage(StyledImage data, double maxWidth, double maxHeight)
        {
            var img = new Image();

            // 画像データを作成
            if (!string.IsNullOrWhiteSpace(data.ImagePath) && File.Exists(data.ImagePath))
            {
                try
                {
                    img = new Image
                    {
                        Source = new BitmapImage(new Uri(data.ImagePath)),
                        MaxWidth = maxWidth,
                        MaxHeight = maxHeight,
                        Stretch = Stretch.Uniform,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    };

                    Debug.WriteLine($"[DEBUG] 読み込み完了: {data.ImagePath}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] ボタンの画像を読み込むことができません: {ex}");
                }
            }
            return img;
        }

        private StackPanel CreateImagePanel(ImageData data)
        {
            var images = data.Images;

            var maxWidth = data.MaxWidth;
            var maxHeight = data.MaxHeight;
            var horizonalAlignment = data.HorizontalAlignment;

            var alignment = HorizontalAlignment.Center;

            if (horizonalAlignment == "left" || horizonalAlignment == "Left")
            {
                alignment = HorizontalAlignment.Left;
            }
            else if (horizonalAlignment == "right" || horizonalAlignment == "Right")
            {
                alignment = HorizontalAlignment.Right;
            }

            // 行StackPanelを作成
            StackPanel rowPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = alignment,
                Margin = new Thickness(10,0,0,0),
            };

            var cnt = 0;

            foreach (var image in images)
            {
                if (image.Label != string.Empty)
                {
                    // 列StackPanelを作成
                    StackPanel colPanel = new StackPanel()
                    {
                        Orientation = Orientation.Vertical,
                        //Margin = new Thickness(8, 8, 8, 3),
                    };

                    if (cnt > 0)
                    {
                        colPanel.Margin = new Thickness(8, 0, 0, 0);
                    }

                    // ラベル
                    var label = CreateImageLabel(image);
                    colPanel.Children.Add(label);

                    // 画像
                    var img = CreateImage(image, maxWidth, maxHeight);
                    colPanel.Children.Add(img);

                    rowPanel.Children.Add(colPanel);
                    cnt++;
                }
                else
                {
                    // 画像を作成
                    var img = CreateImage(image, maxWidth, maxHeight);

                    if(cnt > 0)
                    {
                        img.Margin = new Thickness(8, 0, 0, 0);
                    }

                    rowPanel.Children.Add(img);
                    cnt++;
                }
            }
            return rowPanel;
        }

        private Grid CreateTableGrid(TableData data, PresentationViewerViewModel viewModel, string pageId)
        {
            var rows = data.Rows;
            var cols = data.Columns;
            var indent = data.Indent;
            var showBorder = data.ShowBorder;
            var hasPadding = data.HasPadding;
            var cells = data.Cells;

            // Gridを作成
            Grid grid = new Grid
            {
                MaxWidth = data.MaxWidth,
                MaxHeight = data.MaxHeight,
                Margin = new Thickness(20 + indent * 20, 4, 0, 8),
                VerticalAlignment = VerticalAlignment.Center,
            };

            // 行定義
            for (int r = 0; r < rows; r++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }
            // 列定義
            for (int c = 0; c < cols; c++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }

            int index = 0;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    // デフォルト値を設定
                    Thickness border = new Thickness(0); // 罫線なし
                    Thickness padding = new Thickness(0); //内部の余白なし

                    // 罫線ありの場合
                    if (showBorder)
                    {
                        bool isFirstRow = r == 0;
                        bool isFirstCol = c == 0;

                        if (isFirstRow && isFirstCol)
                        {
                            border = new Thickness(1);
                        }
                        else if (isFirstRow)
                        {
                            border = new Thickness(0, 1, 1, 1);
                        }
                        else if (isFirstCol)
                        {
                            border = new Thickness(1, 0, 1, 1);
                        }
                        else
                        {
                            border = new Thickness(0, 0, 1, 1);
                        }
                    }

                    // 内部の余白ありの場合 
                    if (hasPadding)
                    {
                        padding = new Thickness(3);
                    }

                    Border cell = new Border
                    {
                        BorderThickness = border,
                        BorderBrush = Brushes.Black,
                        Padding = padding,
                        //Child = child,
                    };

                    var cellElement = cells[index];
                    var readOption = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    // ボタンの場合
                    if (cellElement.TryGetProperty("commandName", out _) || cellElement.TryGetProperty("CommandName", out _))
                    {
                        Debug.WriteLine($"[DEBUG] cell: {index} / {rows * cols - 1} をButtonDataに変換します");
                        var obj = JsonSerializer.Deserialize<ButtonData>(cellElement.GetRawText(),readOption);

                        if (obj != null && obj is ButtonData)
                        {
                            Debug.WriteLine($"cell: {index} / {rows * cols - 1} をButtonに変換します");
                            var child = CreateButton(obj, viewModel);

                            child.Padding = new Thickness(0);
                            child.Margin = new Thickness(0);
                            child.Height = double.NaN;
                            child.Width = double.NaN;
                            child.HorizontalAlignment = HorizontalAlignment.Left;

                            cell.Child = child;
                        }
                        else
                        {
                            Debug.WriteLine($"[ERROR] cell: {index} / {rows * cols - 1} をButtonDataに変換できません");
                        }
                    }
                    // チェックボックスの場合
                    else if (cellElement.TryGetProperty("isChecked", out _) || cellElement.TryGetProperty("IsChecked", out _))
                    {
                        Debug.WriteLine($"[DEBUG] cell: {index} / {rows * cols - 1} をCheckBoxDataに変換します");
                        var obj = JsonSerializer.Deserialize<CheckBoxData>(cellElement.GetRawText(), readOption);

                        if (obj != null && obj is CheckBoxData)
                        {
                            obj.CellIndex = index;

                            Debug.WriteLine($"[DEBUG] cell: {index} / {rows * cols - 1} をCheckBoxに変換します");
                            var child = CreateCheckBox(obj);

                            child.Padding = new Thickness(0);
                            child.Margin = new Thickness(0,0,4,0);

                            // CheckBoxの背景色に透明以外が指定されている場合
                            if (obj.BackgroundColor != "Transparent")
                            {
                                var color = (Color)ColorConverter.ConvertFromString(obj.BackgroundColor);
                                var background = new SolidColorBrush(color);
                                // セル（Border）に背景色を設定
                                cell.Background = background;
                            }

                            // チェックボックスの状態を保存する
                            child.Checked += (_, __) =>
                                viewModel.UpdateTableCheckBoxState(pageId, data, obj, obj.Id, true);
                            child.Unchecked += (_, __) =>
                                viewModel.UpdateTableCheckBoxState(pageId, data, obj, obj.Id, false);


                            cell.Child = child;
                        }
                        else
                        {
                            Debug.WriteLine($"[ERROR] cell: {index} / {rows * cols - 1} をCheckBoxDataに変換できません");
                        }

                    }
                    // それ以外（テキストブロック）の場合
                    else
                    {
                        Debug.WriteLine($"[DEBUG] cell: {index} / {rows * cols - 1} をTextBlockDataに変換します");
                        var obj = JsonSerializer.Deserialize<TextBlockData>(cellElement.GetRawText(), readOption);

                        if (obj != null && obj is TextBlockData)
                        {
                            Debug.WriteLine($"[DEBUG] cell: {index} / {rows * cols - 1} をTextBlockに変換します");
                            var child = CreateTextBlock(obj);

                            // テキストブロックの背景色に透明以外が指定されている場合
                            if (obj.BackgroundColor != "Transparent")
                            {
                                var color = (Color)ColorConverter.ConvertFromString(obj.BackgroundColor);
                                var background = new SolidColorBrush(color);
                                // テキストブロックの背景を透明に変更
                                child.Background = Brushes.Transparent;
                                // セル（Border）に背景色を設定
                                cell.Background = background;
                            }

                            cell.Child = child;
                        }
                        else
                        {
                            Debug.WriteLine($"[ERROR] cell: {index} / {rows * cols - 1} をTextBlockDataに変換できません");
                        }
                    }

                    Grid.SetRow(cell, r);
                    Grid.SetColumn(cell, c);
                    grid.Children.Add(cell);

                    index++;
                }
            }
            return grid;
        }

        private StackPanel CreateImageButtonPanel(ImageButtonData data, PresentationViewerViewModel viewModel)
        {
            var imgbtns = data.ImageButtons;

            double maxWidth = data.MaxWidth;
            double maxHeight = data.MaxHeight;

            // 行StackPanelを作成
            StackPanel rowPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
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
                Button imgbtn = CreateImageButton(btn, maxWidth, maxHeight,viewModel);

                colPanel.Children.Add(imgbtn);
                rowPanel.Children.Add(colPanel);
            }
            return rowPanel;
        }

        private StackPanel CreateButtonPanel(TextButtonData data, PresentationViewerViewModel viewModel)
        {
            var btns = data.TextButtons;

            // 行StackPanelを作成
            StackPanel rowPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(8, 15, 8, 3),
            };

            if (data.HorizontalAlignment == "left")
            {
                rowPanel.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else if (data.HorizontalAlignment == "right")
            {
                rowPanel.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else
            {
                rowPanel.HorizontalAlignment = HorizontalAlignment.Center;
            }

            foreach (var btn in btns)
            {
                Button button = CreateButton(btn, viewModel);
                rowPanel.Children.Add(button);
            }
            return rowPanel;
        }

        private CheckBox CreateCheckBox(CheckBoxData data)
        {
            var cb = new CheckBox
            {
                IsChecked = data.IsChecked,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = data.FontSize,
            };

            // ラベルが指定されている場合は設定する
            if (data.Label != null && data.Label != string.Empty)
            {
                cb.Content = data.Label;
            }

            return cb;
        }
    }
}
