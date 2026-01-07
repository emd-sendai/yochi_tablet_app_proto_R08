using WpfApp.Common.Models;

namespace WpfApp.Interfaces
{
    public interface IPdfDocumentService
    {
        IReadOnlyList<PdfData> GetPdfList(string folderPath);

        void OpenPdf(PdfData data);

        void OpenPdf(string folderPath, string fileNameWithoutExtension);
    }
}
