using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO;
using System.Printing;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;
using WpfApp.Interfaces;
using WpfApp.Services;

namespace WpfApp.Screens.Aramashi.ViewModels
{
    /// <summary>
    /// PrintPreviewWindow.xamlに対応するViewModel。
    /// 印刷プレビュー画面での処理を行う。
    /// </summary>
    public partial class PrintPreviewViewModel : ObservableObject
    {
        public event EventHandler? RequestClose;

        private readonly IMessageDialogService _message;
        private readonly IAppPathService _path;

        [ObservableProperty]
        private FixedDocument document;

        private string _previewXpsPath = "";

        public PrintPreviewViewModel(FixedDocument doc)
        {
            _message = new MessageDialogService();
            _path = new AppPathService();

            _previewXpsPath = SaveFixedDocumentToXps(doc);

            Document = doc;
        }

        /// <summary>
        /// FixedDocument → XPS に変換して一時保存する。（印刷プレビュー用）
        /// </summary>
        private string SaveFixedDocumentToXps(FixedDocument fixedDoc)
        {
            string tempFolder = _path.TempDire;
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
                _message.ShowMessage("PDF作成用XPSの生成に失敗しました。");
                throw;
            }
        }

        /// <summary>
        /// XPS → PDF（Microsoft Print to PDF）に変換する。
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
                _message.ShowMessage("PDFの作成に失敗しました。");
                throw;
            }
        }

        /// <summary>
        /// 一時 XPS ファイルを削除する。
        /// </summary>
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

        /// <summary>
        /// プレビュー対象ファイルをPDFファイルとして保存する。
        /// </summary>
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

        /// <summary>
        /// 一時 XPS ファイルを削除して、画面を閉じる。
        /// </summary>
        [RelayCommand]
        private void Close()
        {
            CleanupTempFiles();
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
