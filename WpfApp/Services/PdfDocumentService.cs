using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using WpfApp.Common.Models;
using WpfApp.Interfaces;


namespace WpfApp.Services
{
    public class PdfDocumentService : IPdfDocumentService
    {
        private readonly IMessageDialogService _messageDialog = new MessageDialogService();

        public IReadOnlyList<PdfData> GetPdfList(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return Array.Empty<PdfData>();

            return Directory.GetFiles(folderPath, "*.pdf")
                            .Select(p => new PdfData(p))
                            .ToList();
        }

        public ObservableCollection<PdfData> GetPdfDatas(string folderPath)
        {
            var ret = new ObservableCollection<PdfData>();

            if (!Directory.Exists(folderPath))
            {
                return ret;
            }
            else
            {
                ret = new ObservableCollection<PdfData>(
                    Directory.GetFiles(folderPath, "*.pdf").Select(p => new PdfData(p)).ToList()
                );
                return ret;
            }

        }

        public void OpenPdf(PdfData data)
        {
            if (!File.Exists(data.FullPath))
            {
                Debug.WriteLine($"[ERROR] PDFファイルが存在しません: {data.FullPath}");
                //メッセージウィンドウを表示
                _messageDialog.ShowMessage("PDFファイルが存在しません");
            }
            else
            {
                OpenInternal(data.FullPath);
            }
        }

        public void OpenPdf(string folderPath, string fileNameWithoutExtension)
        {
            var fullPath = Path.Combine(folderPath, fileNameWithoutExtension + ".pdf");

            if (!File.Exists(fullPath))
            {
                Debug.WriteLine($"[ERROR] PDFファイルが存在しません: {fullPath}");
                _messageDialog.ShowMessage("PDFファイルが存在しません");
            }
            else
            {
                OpenInternal(fullPath);
            }
        }

        private void OpenInternal(string fullPath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = fullPath,
                UseShellExecute = true
            });
        }

        public PdfData AddPdf(string folderPath, string sourceFilePath)
        {
            // フォルダが存在しない場合は作成
            Directory.CreateDirectory(folderPath);

            var fileName = Path.GetFileName(sourceFilePath);
            var addFilePath = Path.Combine(folderPath, fileName);

            if (Path.Exists(addFilePath))
            {
                throw new InvalidOperationException($"[ERROR] PDFファイルが既に存在します: {fileName}");
            }

            File.Copy(sourceFilePath, addFilePath);
            Debug.WriteLine($"[DEBUG] PDFファイルを追加しました: {addFilePath}");

            return new PdfData(addFilePath);
        }

        public void UpdatePdf(string filePath, string sourceFilePath)
        {
            var fileName = Path.GetFileName(filePath);

            if (!Path.Exists(filePath))
            {
                throw new InvalidOperationException($"[ERROR] 上書き対象のPDFが存在しません: {fileName}");
            }

            File.Copy(sourceFilePath, filePath, overwrite: true);
            Debug.WriteLine($"[DEBUG] PDFファイルの内容を上書きしました: {filePath}");
        }

        public bool DeletePdf(PdfData data)
        {
            var result = false;

            if (!File.Exists(data.FullPath))
            {
                result = false;
                _messageDialog.ShowMessage("PDFファイルが存在しません");
                Debug.WriteLine($"[ERROR] PDFファイルが存在しません: {data.FullPath}");
            }
            else
            {
                File.Delete(data.FullPath);
                result = true;
                Debug.WriteLine($"[DEBUG] PDFファイルを削除しました: {data.FullPath}");
            }

            return result;
        }
    }
}
