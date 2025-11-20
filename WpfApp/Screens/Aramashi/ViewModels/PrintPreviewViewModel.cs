using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;

namespace WpfApp.Screens.Aramashi.ViewModels
{
    public partial class PrintPreviewViewModel : ObservableObject
    {
        public event EventHandler? RequestClose;

        [ObservableProperty]
        private FixedDocument document;

        private string _previewXpsPath = "";

        [ObservableProperty]
        private double scale = 1.0;

        public double MinScale { get; } = 0.5;
        public double MaxScale { get; } = 5.0;

        public PrintPreviewViewModel(FlowDocument flowDoc)
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string tempFolder = Path.Combine(appDir, "Temp");
            Directory.CreateDirectory(tempFolder);

            _previewXpsPath = Path.Combine(tempFolder, $"tmp_{Guid.NewGuid()}.xps");

            Document = ConvertFlowDocumentToFixedDocument(flowDoc, _previewXpsPath);
        }

        /// <summary>
        /// FlowDocument → FixedDocument（XPS経由）
        /// </summary>
        private FixedDocument ConvertFlowDocumentToFixedDocument(FlowDocument flowDoc, string tempXpsPath)
        {
            try
            {
                // FlowDocument を XPS に書き出す
                using (var xps = new XpsDocument(tempXpsPath, FileAccess.ReadWrite))
                {
                    var writer = XpsDocument.CreateXpsDocumentWriter(xps);
                    writer.Write(((IDocumentPaginatorSource)flowDoc).DocumentPaginator);
                }

                // XPS から FixedDocument を取得
                using (var xpsForPreview = new XpsDocument(tempXpsPath, FileAccess.Read))
                {
                    var fds = xpsForPreview.GetFixedDocumentSequence();
                    if (fds == null || fds.References.Count == 0)
                        throw new InvalidOperationException("FixedDocumentSequence が取得できません。");

                    return fds.References[0].GetDocument(true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] FlowDocument → FixedDocument 変換失敗: {ex.Message}");
                MessageBox.Show("プレビューの作成に失敗しました。", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        /// <summary>
        /// FixedDocument → 一時 XPS に保存（印刷用）
        /// </summary>
        private string SaveFixedDocumentToXps(FixedDocument fixedDoc)
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string tempFolder = Path.Combine(appDir, "Temp");
            Directory.CreateDirectory(tempFolder);
            string tempXps = Path.Combine(tempFolder, $"tmp_{Guid.NewGuid()}.xps");
            try
            {
                using (var xps = new XpsDocument(tempXps, FileAccess.Write))
                {
                    var writer = XpsDocument.CreateXpsDocumentWriter(xps);
                    writer.Write(((IDocumentPaginatorSource)fixedDoc).DocumentPaginator);

                }
                return tempXps;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] FixedDocument → XPS 書き出し失敗: {ex.Message}");
                MessageBox.Show("PDF 作成用 XPS の生成に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        /// <summary>
        /// XPS → PDF（Microsoft Print to PDF）
        /// </summary>
        private void PrintXpsViaMicrosoftPdf(string xpsPath)
        {
            try
            {
                const string printerName = "Microsoft Print to PDF";
                var printServer = new LocalPrintServer();
                var printQueue = printServer.GetPrintQueue(printerName);
                printQueue.AddJob("PrintToPDF", xpsPath, false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] XPS → PDF 印刷処理で失敗: {ex.Message}");
                MessageBox.Show("PDF の作成に失敗しました。", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public void CleanupTempFiles()
        {
            // プレビュー用 XPS を削除
            try
            {
                if (File.Exists(_previewXpsPath)) 
                {
                    File.Delete(_previewXpsPath);
                    Debug.WriteLine($"[DEBUG] プレビュー用の一時XPSファイルを削除しました: {_previewXpsPath}");
                }
            } 
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] プレビュー用の一時XPSファイルを削除できませんでした: {_previewXpsPath}");
                Debug.WriteLine($"[ERROR] {ex.Message}");
            }
        }

        [RelayCommand]
        public void Zoom(double factor)
        {
            double newScale = Scale * factor;
            if (newScale < MinScale) newScale = MinScale;
            if (newScale > MaxScale) newScale = MaxScale;
            Scale = newScale;
        }

        [RelayCommand]
        public void ZoomIn() => Zoom(1.1);

        [RelayCommand]
        public void ZoomOut() => Zoom(1 / 1.1);

        [RelayCommand]
        private void SaveAsPdf()
        {
            string tempXps = SaveFixedDocumentToXps(Document);

            try
            {
                PrintXpsViaMicrosoftPdf(tempXps);
            }
            finally
            {
                try { if (File.Exists(tempXps)) File.Delete(tempXps); } catch { }
            }
        }

        [RelayCommand]
        private void Close()
        {
            CleanupTempFiles();
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
