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
        private string DataFolder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Signature");
        private string IsfPath = "";
        private string PngPath = "";

        //private string SlidePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SlideResources", "Others", "slides.json");

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

            IsfPath = Path.Combine(DataFolder, vm.SignaturefileNames["isf"]);
            PngPath = Path.Combine(DataFolder, vm.SignaturefileNames["png"]);

            LoadCanvas();

            vm.ShowMessageRequested += (_, msg) => ShowMessageWindow(msg);
            //vm.LoadRequested += (_, _) => this.LoadCanvas();
            vm.SaveRequested += (_, canvas) => SaveCanvas(canvas);
            vm.PrintRequested += (_, slidePath) => PrintPreview(slidePath);
            vm.ClearRequested += (_, canvas) => ClearCanvas(canvas);
            vm.RequestClose += (_, _) => this.Close();
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

            PreviewDocument(slideList, PngPath);

        }

        public static void PreviewDocument(List<PresentationPageConfig> slides, string singPath)
        {
            const double inch = 96; // WPF 単位 = 1/96 inch

            // FlowDocument 作成
            FlowDocument doc = new FlowDocument();
            doc.PageWidth = 8.27 * inch;   // ≒ 794
            doc.PageHeight = 11.69 * inch; // ≒ 1123
            doc.PagePadding = new Thickness(50);
            doc.FontFamily = new FontFamily("メイリオ"); // 日本語対応
            doc.ColumnWidth = double.PositiveInfinity; // 折り返しを有効に

            string preTitle = "";

            foreach (var slide in slides)
            {
                // ページ区切り
                if (preTitle != "" && slide.Title != null && slide.Title.GetPlainText() != preTitle && slide.Title.GetPlainText() != "署名")
                {
                    doc.Blocks.Add(new Section()
                    {
                        BreakPageBefore = true
                    });
                }

                // Title
                if (slide.Title != null && slide.Title.GetPlainText() != preTitle && slide.Title.GetPlainText() != "署名")
                {
                    Paragraph title = new Paragraph(new Run(slide.Title.GetPlainText()));
                    title.FontSize = slide.Title.FontSize;
                    title.LineHeight = slide.Title.FontSize * 1.20;
                    title.FontWeight = FontWeights.Bold;
                    title.Margin = new Thickness(0, 0, 0, 10);
                    doc.Blocks.Add(title);
                }

                // Sections
                foreach (var sec in slide.Sections)
                {
                    // Subtitle + Checkbox 横並び
                    if (sec.Subtitle != null)
                    {
                        Paragraph subtitlePara = new Paragraph();
                        subtitlePara.Margin = new Thickness(0, 25, 0, 5);
                        subtitlePara.LineHeight = sec.Subtitle.FontSize * 1.20;
                        subtitlePara.Inlines.Add(new Run(sec.Subtitle.GetPlainText())
                        {
                            FontSize = sec.Subtitle.FontSize,
                            FontWeight = FontWeights.Bold
                        });

                        if (sec.CheckBox != null)
                        {
                            subtitlePara.Inlines.Add(new Run(" ")); // スペース
                                                                    // 簡易チェックボックス
                            string cbMark = sec.CheckBox.IsChecked ? "☑" : "□";
                            subtitlePara.Inlines.Add(new Run($"{cbMark} {sec.CheckBox.Label}")
                            {
                                FontSize = sec.Subtitle.FontSize
                            });
                        }

                        doc.Blocks.Add(subtitlePara);
                    }

                    // Body
                    if (sec.Body != null)
                    {
                        Paragraph bodyPara = new Paragraph();
                        bodyPara.Margin = new Thickness(20, 2, 0, 10);
                        bodyPara.FontSize = sec.Body.FontSize;
                        bodyPara.LineHeight = sec.Body.FontSize * 1.15;
                        bodyPara.Inlines.Add(new Run(sec.Body.GetPlainText()));
                        doc.Blocks.Add(bodyPara);
                    }

                    // SignMessage
                    if (sec.SignMessage != null)
                    {
                        StackPanel panel = new StackPanel();

                        /* 署名メッセージ */
                        // 現在の日付（和暦）を取得
                        var culture = new CultureInfo("ja-JP", false);
                        culture.DateTimeFormat.Calendar = new JapaneseCalendar();
                        string wareki = DateTime.Now.ToString("ggy年M月d日", culture);

                        string editedSignMessage = wareki + "上記についてすべて説明を受けました。";
                        Debug.WriteLine($"[DEBUG] 説明完了年月日: {wareki}");


                        Paragraph signMessagePara = new Paragraph();
                        signMessagePara.Margin = new Thickness(20, 25, 0, 10);
                        signMessagePara.FontSize = 18;
                        signMessagePara.LineHeight = 18 * 1.20;
                        signMessagePara.FontWeight = FontWeights.Bold;
                        signMessagePara.Inlines.Add(editedSignMessage);
                        //doc.Blocks.Add(signMessagePara);

                        //AddImageToFlowDocument(doc, singPath);

                        panel.Children.Add(new TextBlock
                        {
                            Text = editedSignMessage,
                            Margin = new Thickness(20, 25, 0, 10),
                            FontSize = 18,
                            LineHeight = 18 * 1.20,
                            FontWeight = FontWeights.Bold
                        });

                        AddImageToStackPanel(panel, singPath);

                        // BlockUIContainer に追加してFlowDocumentに追加
                        doc.Blocks.Add(new BlockUIContainer(panel));

                    }
                }
                // セクションのタイトルを記録
                if(slide.Title != null)
                {
                    preTitle = slide.Title.GetPlainText();
                }
            }

            // 印刷プレビュー
            /*
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                IDocumentPaginatorSource idpSource = doc;
                pd.PrintDocument(idpSource.DocumentPaginator, "Preview");
            }
            */

            DialogService dialogService = new DialogService();
            dialogService.ShowWindow<PrintPreviewViewModel>(doc);
        }

        private static void AddImageToStackPanel(StackPanel panel, string imagePath)
        {
            if (!File.Exists(imagePath))
                return;

            double maxHeight = 125;
            //double maxWidth = 375;

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // ファイルロック防止
            bitmap.EndInit();

            Image img = new Image
            {
                Source = bitmap,
                Height = Math.Min(bitmap.PixelHeight, maxHeight),
                //Width = Math.Min(bitmap.PixelWidth, maxWidth),
                Stretch = Stretch.Uniform,
                Margin = new Thickness(0, 10, 0, 0)
            };

            panel.Children.Add(img);
        }
    }
}
