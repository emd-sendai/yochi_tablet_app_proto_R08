using System.Diagnostics;
using System.IO;
using WpfApp.Common.Models;
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

        public void OpenPdf(PdfData data)
        {
            OpenInternal(data.FullPath);
        }

        public void OpenPdf(string folderPath, string fileNameWithoutExtension)
        {
            var fullPath = Path.Combine(
                folderPath,
                fileNameWithoutExtension + ".pdf");

            if (!File.Exists(fullPath))
            {
                // ここでログ or MessageBox（UI層に任せてもOK）
                return;
            }

            OpenInternal(fullPath);
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
