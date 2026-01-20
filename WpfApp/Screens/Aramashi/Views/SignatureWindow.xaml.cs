using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp.Common.ViewModels;
using WpfApp.Common.Views;
using WpfApp.Interfaces;
using WpfApp.Screens.Aramashi.Models;
using WpfApp.Screens.Aramashi.ViewModels;
using WpfApp.Services;

namespace WpfApp.Screens.Aramashi.Views
{
    /// <summary>
    /// SignatureWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SignatureWindow : Window
    {
        private bool _isClosingFromViewModel = false;
        private IAppPathService? _path;

        private string? DataFolder;
        private string? IsfPath;
        private string? PngPath;

        public SignatureWindow()
        {
            InitializeComponent();

            Loaded += SignatureWindow_Loaded;
        }

        private void SignatureWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not SignatureViewModel vm)
            {
                Debug.WriteLine("[ERROR] DataContext が ViewModel ではありません");
                return;
            }

            Debug.WriteLine("[DEBUG] SignatureWindow Loaded - DataContext 取得成功");

            _path = new AppPathService();
            DataFolder = _path.SignatureDire;
            IsfPath = Path.Combine(DataFolder, vm.SignaturefileNames["isf"]);
            PngPath = Path.Combine(DataFolder, vm.SignaturefileNames["png"]);

            LoadCanvas();

            vm.ShowMessageRequested += (_, msg) => ShowMessageWindow(msg);
            //vm.LoadRequested += (_, _) => this.LoadCanvas();
            vm.SaveRequested += (_, canvas) => SaveCanvas(canvas);
            vm.PrintRequested += (_, slidePath) => PrintPreview(slidePath);
            vm.ClearRequested += (_, canvas) => ClearCanvas(canvas);
            vm.RequestClose += (_, _) =>
                {
                    _isClosingFromViewModel = true;
                };

            this.Closing += Window_Closing;
        }

        private void Window_Closing(object? sender, CancelEventArgs e)
        {
            if (!_isClosingFromViewModel && DataContext is SignatureViewModel vm)
            {
                _isClosingFromViewModel = true;
                vm.OnRequestClose(); // ここでイベントを発火
            }
        }

        private void ShowMessageWindow(string msg)
        {
            //メッセージウィンドウを表示
            var vm = new MessageDialogViewModel
            {
                Message = msg
            };

            var dialog = new MessageDialogWindow
            {
                DataContext = vm,
                Owner = this
            };

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                // OK押下後の処理
            }
        }

        private void LoadCanvas()
        {
            if (!string.IsNullOrEmpty(IsfPath) && File.Exists(IsfPath))
                LoadSignatureIsf(SignatureCanvas, IsfPath);
        }

        private void LoadSignatureIsf(InkCanvas canvas, string path)
        {
            canvas.Strokes.Clear();
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            canvas.Strokes = new StrokeCollection(fs);
            Debug.WriteLine($"[DEBUG] 署名のISFデータを取得しました: {path}");
        }

        private void ClearCanvas(InkCanvas canvas)
        {
            canvas.Strokes.Clear();

            if (File.Exists(IsfPath))
            {
                File.Delete(IsfPath);
                Debug.WriteLine($"[DEBUG] 署名のISFデータを削除しました: {IsfPath}");
            }

            if (File.Exists(PngPath))
            {
                File.Delete(PngPath);
                Debug.WriteLine($"[DEBUG] 署名のPNGデータを削除しました: {PngPath}");
            }            
        }

        // 署名だけをPNG画像としてローカルフォルダに保存する
        private void SaveCanvasAsPng(InkCanvas canvas)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PNGファイル (*.png)|*.png",
                FileName = "signature.png"
            };

            if (dlg.ShowDialog() != true) return;

            var size = new Size(canvas.ActualWidth, canvas.ActualHeight);
            canvas.Measure(size);
            canvas.Arrange(new Rect(size));

            var rtb = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96d, 96d, PixelFormats.Default);
            rtb.Render(canvas);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using var fs = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write);
            encoder.Save(fs);

            ShowMessageWindow("署名をPNG画像として保存しました");
        }

        private void SaveCanvas(InkCanvas canvas)
        {
            if (string.IsNullOrWhiteSpace(DataFolder) || string.IsNullOrWhiteSpace(IsfPath)
                || string.IsNullOrWhiteSpace(PngPath))
            {
                ShowMessageWindow("署名データを保存できません");
                return;
            }

            if (!Directory.Exists(DataFolder))
            {
                Directory.CreateDirectory(DataFolder);
            }

            // 署名をISF形式で保存
            bool isf = SaveSignatureIsf(canvas, IsfPath);
            // 署名をPNG形式で保存
            bool png = SaveSignaturePng(canvas, PngPath);

            if (isf && png)
            {
                ShowMessageWindow("署名データを保存しました");
            }
            else
            {
                ShowMessageWindow("署名データを保存できません");
            }
        }

        // ISF保存
        private bool SaveSignatureIsf(InkCanvas canvas, string path)
        {
            bool ret = false;

            try
            {
                using var fs = new FileStream(path, FileMode.Create);
                canvas.Strokes.Save(fs);

                ret = true;
                Debug.WriteLine($"[DEBUG] 署名をISFデータとして保存しました: {path}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] 署名をISFデータとして保存できません: {ex}");
            }

            return ret;
        }

        // PNG保存
        private bool SaveSignaturePng(InkCanvas canvas, string path)
        {
            bool ret = false;

            try
            {
                var bounds = canvas.Strokes.GetBounds();
                var rtb = new RenderTargetBitmap(
                    (int)Math.Ceiling(bounds.Width),
                    (int)Math.Ceiling(bounds.Height),
                    96, 96,
                    PixelFormats.Pbgra32);

                var dv = new DrawingVisual();
                using (var dc = dv.RenderOpen())
                {
                    dc.PushTransform(new TranslateTransform(-bounds.X, -bounds.Y));
                    canvas.Strokes.Draw(dc);
                }

                rtb.Render(dv);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));

                using var fs = new FileStream(path, FileMode.Create);
                encoder.Save(fs);

                ret = true;
                Debug.WriteLine($"[DEBUG] 署名をPNGデータとして保存しました: {path}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] 署名をPNGデータとして保存できません: {ex}");
            }

            return ret;
        }

        private void PrintPreview(string slidePath)
        {
            if (!File.Exists(PngPath))
            {
                Debug.WriteLine($"[DEBUG] 署名のPNGデータが存在しません: {PngPath}");
                ShowMessageWindow("保存された署名がありません\n署名を保存してからPDF出力を行ってください");
                return;
            }

            if (!File.Exists(slidePath))
            {
                Debug.WriteLine($"[DEBUG] スライドデータが見つかりません: {slidePath}");
                return;
            }

            var jsonText = File.ReadAllText(slidePath);
            var slideList = JsonSerializer.Deserialize<List<PresentationPageConfig>>(jsonText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (slideList == null)
            {
                Debug.WriteLine($"[DEBUG] スライドデータが空です: {slidePath}");
                return;
            }

            // FixedDocumentを作成する
            FixedDocument doc = new FixedDocument();
            doc = GeneratFixedDocument(slideList, PngPath);

            if(doc == null)
            {
                Debug.WriteLine($"[DEBUG] FixedDocumentを作成できませんでした");
                ShowMessageWindow("PDFデータを出力できませんでした");
                return;
            }

            // プレビュー画面を表示する
            DialogService dialogService = new DialogService();
            dialogService.ShowWindow<PrintPreviewViewModel>(doc);
        }

        private FixedDocument GeneratFixedDocument(List<PresentationPageConfig> slides, string singPath)
        {
            const double inch = 96; // WPF単位 = 1/96 inch
            double pageWidth = 8.27 * inch;
            double pageHeight = 11.69 * inch;
            double pagePadding = 50;

            double itemWidth = 670;

            FixedDocument fixedDoc = new FixedDocument();
            FixedPage currentPage;
            double yOffset = 0;

            void NewPage()
            {
                currentPage = new FixedPage
                {
                    Width = pageWidth,
                    Height = pageHeight,
                    Background = Brushes.White
                };
                PageContent pc = new PageContent();
                ((IAddChild)pc).AddChild(currentPage);
                fixedDoc.Pages.Add(pc);
                yOffset = pagePadding;
            }

            NewPage();

            string preTitle = "";

            foreach (var slide in slides)
            {
                // ページ区切り（タイトル変更時）
                if (!string.IsNullOrEmpty(preTitle) && slide.Title != null &&
                    slide.Title.GetPlainText() != preTitle && slide.Title.GetPlainText() != "署名")
                {
                    NewPage();
                }

                // Title
                if (slide.Title != null && slide.Title.GetPlainText() != preTitle &&
                    slide.Title.GetPlainText() != "署名")
                {
                    TextBlock titleBlock = new TextBlock
                    {
                        Text = slide.Title.GetPlainText(),
                        FontSize = slide.Title.FontSize,
                        FontWeight = FontWeights.Bold,
                        TextWrapping = TextWrapping.Wrap,
                        Width = itemWidth, //pageWidth - 2 * pagePadding,  // 左右余白を均等に
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    titleBlock.Measure(new Size(titleBlock.Width, double.PositiveInfinity));
                    double titleHeight = titleBlock.DesiredSize.Height;

                    if (yOffset + titleHeight > pageHeight - pagePadding)
                        NewPage();

                    FixedPage.SetLeft(titleBlock, pagePadding); // 左余白固定
                    FixedPage.SetTop(titleBlock, yOffset);
                    currentPage.Children.Add(titleBlock);
                    yOffset += titleHeight + 10;
                }

                // Sections
                foreach (var sec in slide.Sections)
                {
                    // Subtitle + Checkbox
                    if (sec.Subtitle != null)
                    {
                        string subtitleText = sec.Subtitle.GetPlainText();
                        if (sec.CheckBox != null)
                        {
                            string cbMark = sec.CheckBox.IsChecked ? "☑" : "□";
                            subtitleText += $" {cbMark} {sec.CheckBox.Label}";
                        }

                        TextBlock subtitleBlock = new TextBlock
                        {
                            Text = subtitleText,
                            FontSize = sec.Subtitle.FontSize,
                            FontWeight = FontWeights.Bold,
                            TextWrapping = TextWrapping.Wrap,
                            Width = itemWidth,  //pageWidth - 2 * pagePadding,  // 左右余白均等
                            Margin = new Thickness(0, 10, 0, 5)
                        };
                        subtitleBlock.Measure(new Size(subtitleBlock.Width, double.PositiveInfinity));
                        double subtitleHeight = subtitleBlock.DesiredSize.Height;

                        if (yOffset + subtitleHeight > pageHeight - pagePadding)
                            NewPage();

                        FixedPage.SetLeft(subtitleBlock, pagePadding); // 左余白固定
                        FixedPage.SetTop(subtitleBlock, yOffset);
                        currentPage.Children.Add(subtitleBlock);
                        yOffset += subtitleHeight + 5;
                    }

                    // Body
                    if (sec.Body != null)
                    {
                        string fullText = sec.Body.GetPlainText();
                        double fontSize = sec.Body.FontSize;

                        int startIndex = 0;

                        while (startIndex < fullText.Length)
                        {
                            TextBlock bodyBlock = new TextBlock
                            {
                                FontSize = fontSize,
                                TextWrapping = TextWrapping.Wrap,
                                Width = itemWidth,
                                Margin = new Thickness(0, 2, 0, 10)
                            };

                            double availableHeight = pageHeight - pagePadding - yOffset;

                            int length = 0;

                            // 1文字ずつ追加し、高さが収まるところまで伸ばす
                            for (int i = startIndex; i < fullText.Length; i++)
                            {
                                length++;

                                bodyBlock.Text = fullText.Substring(startIndex, length);
                                bodyBlock.Measure(new Size(itemWidth, double.PositiveInfinity));

                                if (bodyBlock.DesiredSize.Height > availableHeight)
                                {
                                    length--; // 1つ戻す
                                    break;
                                }
                            }

                            // 一文字も入らない場合は強制で1文字入れる
                            length = Math.Max(length, 1);

                            // 行頭禁則（読点・句点）
                            string segment = fullText.Substring(startIndex, length);
                            if (segment.Length > 0 && (segment[0] == '、' || segment[0] == '。'))
                            {
                                segment = "\u200B" + segment;
                            }

                            bodyBlock.Text = segment;
                            bodyBlock.Measure(new Size(itemWidth, double.PositiveInfinity));

                            // ページに置く
                            FixedPage.SetLeft(bodyBlock, pagePadding + 20);
                            FixedPage.SetTop(bodyBlock, yOffset);
                            currentPage.Children.Add(bodyBlock);

                            yOffset += bodyBlock.DesiredSize.Height + 10;

                            startIndex += length;

                            // 次のページが必要なら新しいページを作る
                            if (startIndex < fullText.Length)
                            {
                                NewPage();
                            }
                        }
                    }

                    // SignMessage + Image
                    if (sec.SignMessage != null)
                    {
                        string wareki = DateTime.Now.ToString("ggy年M月d日", new CultureInfo("ja-JP") { DateTimeFormat = { Calendar = new JapaneseCalendar() } });
                        string signText = wareki + "上記についてすべて説明を受けました。";

                        TextBlock signBlock = new TextBlock
                        {
                            Text = signText,
                            FontSize = 18,
                            FontWeight = FontWeights.Bold,
                            TextWrapping = TextWrapping.Wrap,
                            Width = itemWidth, //pageWidth - 2 * pagePadding,  // 左右余白均等
                            Margin = new Thickness(0, 20, 0, 10)
                        };
                        signBlock.Measure(new Size(signBlock.Width, double.PositiveInfinity));
                        double signHeight = signBlock.DesiredSize.Height;

                        double imgHeight = 0;
                        Image? img = null;
                        if (File.Exists(singPath))
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(singPath, UriKind.Absolute);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();

                            img = new Image
                            {
                                Source = bitmap,
                                Height = Math.Min(bitmap.PixelHeight, 125),
                                Stretch = Stretch.Uniform,
                                Width = pageWidth - 2 * pagePadding - 20, // 左右余白を均等に
                            };
                            imgHeight = img.Height;
                        }

                        // ページに収まらなければ改ページ
                        if (yOffset + signHeight + imgHeight + 5 > pageHeight - pagePadding)
                            NewPage();

                        FixedPage.SetLeft(signBlock, pagePadding);
                        FixedPage.SetTop(signBlock, yOffset);
                        currentPage.Children.Add(signBlock);
                        yOffset += signHeight + 10;

                        if (img != null)
                        {
                            FixedPage.SetLeft(img, pagePadding + 20);
                            FixedPage.SetTop(img, yOffset);
                            currentPage.Children.Add(img);
                            yOffset += imgHeight + 5;
                        }
                    }
                }

                // タイトルを記録
                if (slide.Title != null)
                    preTitle = slide.Title.GetPlainText();
            }

            return fixedDoc;
        }
    }
}
