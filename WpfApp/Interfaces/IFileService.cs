using WpfApp.Common.Models;

namespace WpfApp.Interfaces
{
    /// <summary>
    /// 各種ファイルのデータの取得、閲覧、追加、上書き、削除を行うためのインターフェース。
    /// </summary>
    public interface IFileService
    {
        List<FileData> GetFiles(string direPath);

        void OpenFile(FileData file);

        void OpenFile(string filePath);

        bool CanAddFile(string direPath, string sourceFilePath);

        FileData AddFile(string direPath, string sourceFilePath);

        void UpdateFile(string filePath, string sourceFilePath);

        void DeleteFile(FileData file);

        void DeleteDirectory(string direPath);
    }
}
