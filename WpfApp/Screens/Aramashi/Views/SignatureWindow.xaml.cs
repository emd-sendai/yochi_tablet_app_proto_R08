using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp.Common.ViewModels;
using WpfApp.Common.Views;
using WpfApp.Screens.Aramashi.Models;
using WpfApp.Screens.Aramashi.ViewModels;
using WpfApp.Services;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace WpfApp.Screens.Aramashi.Views
{
    /// <summary>
    /// SignatureWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SignatureWindow : Window
    {

        private string DataFolder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Signature");
        private string IsfPath => Path.Combine(DataFolder, "signature.isf");
        private string PngPath => Path.Combine(DataFolder, "signature.png");
        //private string PdfPath => Path.Combine(DataFolder, "output.pdf");
        private string SlidePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SlideResources", "Others", "slides.json");

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

            LoadCanvas();

            vm.ShowMessageRequested += (_, msg) => ShowMessageWindow(msg);
            //vm.LoadRequested += (_, _) => this.LoadCanvas();
            vm.SaveRequested += (_, canvas) => SaveCanvas(canvas);
            vm.PrintRequested += (_, _) => PrintPreview();
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
        }

        private void ClearCanvas(InkCanvas canvas)
        {
            canvas.Strokes.Clear();
            if (File.Exists(IsfPath)) File.Delete(IsfPath);
            if (File.Exists(PngPath)) File.Delete(PngPath);
            //if (File.Exists(PdfPath)) File.Delete(PdfPath);
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

            SaveSignatureIsf(canvas, IsfPath);
            SaveSignaturePng(canvas, PngPath);

            ShowMessageWindow("署名データを保存しました");
        }

        // ISF保存
        private void SaveSignatureIsf(InkCanvas canvas, string path)
        {
            if (canvas.Strokes.Count == 0)
                return;

            try
            {
                using var fs = new FileStream(path, FileMode.Create);
                canvas.Strokes.Save(fs);
                Debug.WriteLine($"[DEBUG] 署名をISFデータとして保存しました");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] 署名をISFデータとして保存できません: {ex}");
            }

        }

        // PNG保存
        private void SaveSignaturePng(InkCanvas canvas, string path)
        {
            if (canvas.Strokes.Count == 0)
                return;

            try
            {
                var bounds = canvas.Strokes.GetBounds();
                var rtb = new RenderTargetBitmap(
                    (int)Math.Ceiling(bounds.Width),
                    (int)Math.Ceiling(bounds.Height),
                    96, 96,
                    System.Windows.Media.PixelFormats.Pbgra32);

                var dv = new System.Windows.Media.DrawingVisual();
                using (var dc = dv.RenderOpen())
                {
                    dc.PushTransform(new System.Windows.Media.TranslateTransform(-bounds.X, -bounds.Y));
                    canvas.Strokes.Draw(dc);
                }

                rtb.Render(dv);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));

                using var fs = new FileStream(path, FileMode.Create);
                encoder.Save(fs);
                Debug.WriteLine($"[DEBUG] 署名をPNGデータとして保存しました");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] 署名をPNGデータとして保存できません: {ex}");
            }

        }

        private void PrintPreview()
        {
            if (!File.Exists(PngPath))
            {
                Debug.WriteLine($"[DEBUG] 署名データが存在しません");
                ShowMessageWindow("保存された署名がありません\n署名を保存してからPDF出力してください");
                return;
            }


            if (!File.Exists(SlidePath))
            {
                Debug.WriteLine($"[DEBUG] スライドデータが見つかりません");
                return;
            }

            var jsonText = File.ReadAllText(SlidePath);
            var slideList = JsonSerializer.Deserialize<List<PresentationPageConfig>>(jsonText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (slideList == null)
            {
                Debug.WriteLine($"[DEBUG] スライドデータが空です");
                return;
            }

            PreviewDocument(slideList, PngPath);

        }

        public static void PreviewDocument(List<PresentationPageConfig> slides, string singPath)
        {
            // FlowDocument 作成
            FlowDocument doc = new FlowDocument();
            doc.PageWidth = 800;
            doc.PageHeight = 1120;
            doc.FontFamily = new FontFamily("メイリオ"); // 日本語対応
            doc.ColumnWidth = double.PositiveInfinity; // 折り返しを有効に

            string preId = "";

            foreach (var slide in slides)
            {
                // Title
                if (slide.Title != null && slide.Id != preId)
                {
                    Paragraph title = new Paragraph(new Run(slide.Title.GetPlainText()));
                    title.FontSize = slide.Title.FontSize;
                    title.FontWeight = FontWeights.Bold;
                    title.Margin = new Thickness(0, 20, 0, 20);
                    doc.Blocks.Add(title);
                }

                // Sections
                foreach (var sec in slide.Sections)
                {
                    // Subtitle + Checkbox 横並び
                    if (sec.Subtitle != null)
                    {
                        Paragraph subtitlePara = new Paragraph();
                        subtitlePara.Margin = new Thickness(0, 5, 0, 5);
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
                        bodyPara.Inlines.Add(new Run(sec.Body.GetPlainText()));
                        doc.Blocks.Add(bodyPara);
                    }

                    // SignMessage
                    if (sec.SignMessage != null)
                    {

                        /* 署名メッセージ */
                        // 現在の日付（和暦）を取得
                        var culture = new CultureInfo("ja-JP", false);
                        culture.DateTimeFormat.Calendar = new JapaneseCalendar();
                        string wareki = DateTime.Now.ToString("ggy年M月d日", culture);
                        Debug.WriteLine($"[DEBUG] 説明完了年月日: {wareki}");


                        Paragraph signMessagePara = new Paragraph();
                        signMessagePara.Margin = new Thickness(20, 20, 0, 10);
                        signMessagePara.FontSize = 18;
                        signMessagePara.Inlines.Add(wareki + "上記についてすべて説明を受けました。");
                        doc.Blocks.Add(signMessagePara);

                        AddImageToFlowDocument(doc, singPath);
                    }

                    // セクション間スペース
                    doc.Blocks.Add(new Paragraph(new Run("")) { Margin = new Thickness(0, 5, 0, 5) });
                }

                // ページ区切り
                if(preId != "" && slide.Id != preId)
                {
                    doc.Blocks.Add(new Paragraph(new Run("")) { BreakPageBefore = true });
                }
                
                // 前のセクションのIdを記録
                preId = slide.Id;
            }

            // 印刷プレビュー
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                IDocumentPaginatorSource idpSource = doc;
                pd.PrintDocument(idpSource.DocumentPaginator, "Presentation Preview");
            }
        }

        private static void AddImageToFlowDocument(FlowDocument doc, string imagePath, double maxWidth = 400)
        {
            if (!File.Exists(imagePath))
                return;

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // ファイルロック防止
            bitmap.EndInit();

            Image img = new Image
            {
                Source = bitmap,
                Width = Math.Min(bitmap.PixelWidth, maxWidth),
                Stretch = Stretch.Uniform,
                Margin = new Thickness(0, 10, 0, 20)
            };

            // FlowDocument に追加
            BlockUIContainer container = new BlockUIContainer(img)
            {
                Margin = new Thickness(0, 10, 0, 20)
            };
            doc.Blocks.Add(container);
        }

    }
}
