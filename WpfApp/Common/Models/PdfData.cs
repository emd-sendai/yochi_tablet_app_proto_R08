using System.IO;

namespace WpfApp.Common.Models
{
    /// <summary>
    /// PDF資料のデータを保持するクラス。
    /// </summary>
    public class PdfData
    {
        /// <summary>
        /// PDFファイル名（拡張子なし）
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// PDFファイル名（拡張子あり）
        /// </summary>
        public string FullFileName { get; }

        /// <summary>
        /// PDFファイルパス（ファイル名含む）
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// 引数のfullPathをもとにPdfDataの各変数にデータを格納する。
        /// </summary>
        /// <param name="fullPath"></param>
        public PdfData(string fullPath)
        {
            FullPath = fullPath;
            FileName = Path.GetFileNameWithoutExtension(fullPath);
            FullFileName = Path.GetFileName(fullPath);
        }
    }
}
