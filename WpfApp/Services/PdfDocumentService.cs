using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using WpfApp.Common.Models;
using WpfApp.Common.ViewModels;
using WpfApp.Common.Views;
using WpfApp.Interfaces;


namespace WpfApp.Services
{
    public class PdfDocumentService : IPdfDocumentService
    {
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
                var vm = new MessageDialogViewModel
                {
                    Message = "PDFファイルが存在しません"
                };

                var dialog = new MessageDialogWindow
                {
                    DataContext = vm,
                    Owner = Application.Current.MainWindow
                };
                dialog.ShowDialog();
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
                //メッセージウィンドウを表示
                var vm = new MessageDialogViewModel
                {
                    Message = "PDFファイルが存在しません"
                };

                var dialog = new MessageDialogWindow
                {
                    DataContext = vm,
                    Owner = Application.Current.MainWindow
                };
                dialog.ShowDialog();
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
    }
}
