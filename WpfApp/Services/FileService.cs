using System.Diagnostics;
using System.IO;
using WpfApp.Common.Models;
using WpfApp.Interfaces;

namespace WpfApp.Services
{
    public class FileService : IFileService
    {
        private readonly IVideoService _video;

        public FileService()
        {
            _video = new VideoService();
        }

        /// <summary>
        /// ディレクトリ内に存在するファイルデータをリストで取得する。
        /// </summary>
        /// <param name="direPath"></param>
        /// <returns></returns>
        public List<FileData> GetFiles(string direPath)
        {
            var files = new List<FileData>();

            if (Directory.Exists(direPath))
            {
                files = Directory.GetFiles(direPath).Select(path => new FileData { FilePath = path }).ToList();
            }

            return files;
        }

        /// <summary>
        /// 指定のファイルパスをデフォルトのビュアーを起動して開く。
        /// </summary>
        /// <param name="filePath"></param>
        private void OpenInternal(string filePath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }

        /// <summary>
        /// 指定のファイルパスをアプリ内の動画再生機能を起動して開く。
        /// </summary>
        /// <param name="filePath"></param>
        private void OpenVideo(string filePath)
        {
           _video.OpenVideo(filePath);
        }

        /// <summary>
        /// 指定のファイルデータからファイルパスを開く。
        /// </summary>
        /// <param name="file"></param>
        public void OpenFile(FileData file)
        {
            // 動画ファイルの場合
            if (string.Equals(file.FileExtension, ".mp4", StringComparison.OrdinalIgnoreCase))
            {
                OpenVideo(file.FilePath);
            }
            // それ以外の場合
            else
            {
                OpenInternal(file.FilePath);
            }
        }

        /// <summary>
        /// 指定のファイルパスを開く。
        /// </summary>
        /// <param name="filePath"></param>
        public void OpenFile(string filePath)
        {
            var extension = Path.GetExtension(filePath);

            // 動画ファイルの場合
            if (string.Equals(extension, ".mp4", StringComparison.OrdinalIgnoreCase))
            {
                OpenVideo(filePath);
            }
            // それ以外の場合
            else
            {
                OpenInternal(filePath);
            }
        }

        /// <summary>
        /// ファイルを指定のディレクトリ内にが追加可能か（既存のファイル名と重複していないか）判定する。
        /// </summary>
        /// <param name="direPath"></param>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        public bool CanAddFile(string direPath, string sourceFilePath)
        {
            var fileName = Path.GetFileName(sourceFilePath);
            var destinationPath = Path.Combine(direPath, fileName);

            if (Path.Exists(destinationPath))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// ファイルを指定のディレクトリ内に追加する。
        /// </summary>
        /// <param name="direPath"></param>
        /// <param name="sourceFilePath"></param>
        /// <returns></returns>
        public FileData AddFile(string direPath, string sourceFilePath)
        {
            // ディレクトリが存在しない場合は作成する
            Directory.CreateDirectory(direPath);

            var fileName = Path.GetFileName(sourceFilePath);
            var destinationPath = Path.Combine(direPath, fileName);

            File.Copy(sourceFilePath, destinationPath);
            Debug.WriteLine($"[DEBUG] ファイルを追加しました: {destinationPath}");
            var file = new FileData() { FilePath = destinationPath };
            return file;
        }

        /// <summary>
        /// 指定のファイルの内容を上書きする。
        /// </summary>
        /// <param name="sourceFilePath">コピー元ファイルパス</param>
        /// <param name="filePath">上書き先ファイルパス</param>
        public void UpdateFile(string sourceFilePath, string filePath)
        {
            File.Copy(sourceFilePath, filePath, overwrite: true);
            Debug.WriteLine($"[DEBUG] ファイルの内容を上書きしました: {filePath}");
        }

        /// <summary>
        /// 指定のファイルを削除する。
        /// </summary>
        /// <param name="file"></param>
        public void DeleteFile(FileData file)
        {
            File.Delete(file.FilePath);
            Debug.WriteLine($"[DEBUG] ファイルを削除しました: {file.FilePath}");
        }

        /// <summary>
        /// 指定のディレクトリとその配下のディレクトリ、フォルダをすべて削除する。
        /// </summary>
        /// <param name="direPath"></param>
        public void DeleteDirectory(string direPath)
        {
            Directory.Delete(direPath, recursive: true);
            Debug.WriteLine($"[DEBUG] ディレクトリを削除しました: {direPath}");
        }
    }
}
