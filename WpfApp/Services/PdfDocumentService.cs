using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using WpfApp.Common.Models;
using WpfApp.Interfaces;


namespace WpfApp.Services
{
    /// <summary>
    /// アプリ内のPDFファイルの閲覧、追加、上書き、削除を行うサービスクラス。
    /// </summary>
    public class PdfDocumentService : IPdfDocumentService
    {
        private readonly IMessageDialogService _messageDialog = new MessageDialogService();

        /// <summary>
        /// ディレクトリ内のPDFファイルの情報を取得する。（閲覧用）
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public IReadOnlyList<PdfData> GetPdfList(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return Array.Empty<PdfData>();

            return Directory.GetFiles(folderPath, "*.pdf")
                            .Select(p => new PdfData(p))
                            .ToList();
        }

        /// <summary>
        /// ディレクトリ内のPDFファイルの情報を取得する。（追加、上書き、削除用）
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
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

        /// <summary>
        /// PdfDataからPDFファイルを閲覧する。
        /// </summary>
        /// <param name="data"></param>
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

        /// <summary>
        /// ディレクトリのパスとファイル名（拡張子なし）からPDFファイルを閲覧する。
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fileNameWithoutExtension"></param>
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

        /// <summary>
        /// デフォルトのPDFビュアーを起動し、PDFファイルを閲覧する。
        /// </summary>
        /// <param name="fullPath"></param>
        private void OpenInternal(string fullPath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = fullPath,
                UseShellExecute = true
            });
        }

        /// <summary>
        /// PDFファイルをコピーして、アプリ内のディレクトリに追加する。
        /// </summary>
        /// <param name="folderPath">追加対象ディレクトリパス</param>
        /// <param name="sourceFilePath">コピー元ファイルパス</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// ディレクトリに既に同名のPDFファイルが存在する場合。
        /// </exception>
        public PdfData AddPdf(string folderPath, string sourceFilePath)
        {
            // フォルダが存在しない場合は作成
            Directory.CreateDirectory(folderPath);

            var fileName = Path.GetFileName(sourceFilePath);
            var addFilePath = Path.Combine(folderPath, fileName);

            if (Path.Exists(addFilePath))
            {
                throw new InvalidOperationException($"追加対象と同一の名称のPDFファイルが既に存在します: {fileName}");
            }

            File.Copy(sourceFilePath, addFilePath);
            Debug.WriteLine($"[DEBUG] PDFファイルを追加しました: {addFilePath}");

            return new PdfData(addFilePath);
        }

        /// <summary>
        /// PDFファイルの内容をコピーし、アプリ内の既存PDFファイルの内容を上書きする。
        /// </summary>
        /// <param name="filePath">上書き対象PDFファイルパス</param>
        /// <param name="sourceFilePath">コピー元ファイルパス</param>
        /// <exception cref="InvalidOperationException">
        /// 上書き対象ファイルが存在しない場合。
        /// </exception>
        public void UpdatePdf(string filePath, string sourceFilePath)
        {
            var fileName = Path.GetFileName(filePath);

            if (!Path.Exists(filePath))
            {
                throw new InvalidOperationException($"上書き対象のPDFが存在しません: {fileName}");
            }

            File.Copy(sourceFilePath, filePath, overwrite: true);
            Debug.WriteLine($"[DEBUG] PDFファイルの内容を上書きしました: {filePath}");
        }

        /// <summary>
        /// アプリ内の既存PDFファイルを削除する。
        /// </summary>
        /// <param name="data">削除対象のPDF情報（PdfData）</param>
        /// <exception cref="InvalidOperationException">
        /// 削除対象ファイルが存在しない場合。
        /// </exception>
        public void DeletePdf(PdfData data)
        {
            if (!File.Exists(data.FullPath))
            {
                throw new InvalidOperationException($"削除対象のPDFファイルが存在しません: {data.FullPath}");
            }

            File.Delete(data.FullPath);
            Debug.WriteLine($"[DEBUG] PDFファイルを削除しました: {data.FullPath}");

        }
    }
}
