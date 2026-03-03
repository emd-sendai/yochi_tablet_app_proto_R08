using System.Diagnostics;
using System.IO;
using System.Text;
using WpfApp.Interfaces;

namespace WpfApp.Services
{
    /// <summary>
    /// ログを出力するサービスクラス。
    /// </summary>
    public class LoggingService : ILoggingService
    {
        private readonly IAppPathService _path;

        private readonly string _logDire;

        public LoggingService()
        {
            _path = new AppPathService();
            _logDire = _path.LogsDire;

            // ログ用フォルダが存在しない場合は作成する
            Directory.CreateDirectory(_logDire);
        }

        /// <summary>
        /// ログファイルにエラーメッセージを追記する。
        /// ログファイルは日別に作成される。
        /// </summary>
        /// <param name="ex"></param>
        public void LogError(Exception ex)
        {
            try
            {
                var path = GetTodayLogFilePath();
                var message = CreateLogMessage(ex);
                // ファイルが存在しない場合は作成してログを書き込み、保存する。
                File.AppendAllText(path, message, Encoding.UTF8);
                Debug.WriteLine($"[ERROR] エラーログを出力しました: {path}");

                DeleteOldLogFiles();
            }
            catch
            {

            }
        }

        /// <summary>
        /// 書き込み対象のログファイルパスを取得する。
        /// </summary>
        /// <returns></returns>
        private string GetTodayLogFilePath()
        {
            string fileName = $"error_{DateTime.Now:yyyyMMdd}.log";
            return Path.Combine(_logDire, fileName);
        }

        /// <summary>
        /// ログメッセージを作成する。
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private string CreateLogMessage(Exception ex)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]");
            sb.AppendLine(ex.ToString());
            sb.AppendLine();
            sb.AppendLine("----------------------------------------");
            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// 最新10件以外の古いログファイルを削除する。
        /// </summary>
        private void DeleteOldLogFiles()
        {
            var files = new DirectoryInfo(_logDire)
                .GetFiles("error_*.log")
                .OrderByDescending(f => f.Name)
                .Skip(10);

            foreach (var file in files)
            {
                file.Delete();
            }
        }

    }
}
