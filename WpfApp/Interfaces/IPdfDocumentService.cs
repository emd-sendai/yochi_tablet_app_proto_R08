using System.Collections.ObjectModel;
using WpfApp.Common.Models;

namespace WpfApp.Interfaces
{
    /// <summary>
    /// PDFファイルの閲覧、追加、上書き、削除を行うためのインターフェース。
    /// </summary>
    public interface IPdfDocumentService
    {
        IReadOnlyList<PdfData> GetPdfList(string folderPath);

        ObservableCollection<PdfData> GetPdfDatas(string folderPath);

        void OpenPdf(PdfData data);

        void OpenPdf(string folderPath, string fileNameWithoutExtension);

        PdfData AddPdf(string folderPath, string sourceFilePath);

        void UpdatePdf(string folderPath, string sourceFilePath);

        void DeletePdf(PdfData data);
    }
}
