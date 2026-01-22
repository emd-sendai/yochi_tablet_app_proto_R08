using System.Diagnostics;
using System.IO;
using System.Text;
using WpfApp.Interfaces;

namespace WpfApp.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly IAppPathService _path;

        private readonly string _logDire;

        public LoggingService()
        {
            _path = new AppPathService();
            _logDire = _path.LogsDire;

            Directory.CreateDirectory(_logDire);
        }

        public void LogError(Exception ex)
        {
            try
            {
                var path = GetTodayLogFilePath();
                var message = CreateLogMessage(ex);

                File.AppendAllText(path, message, Encoding.UTF8);
                Debug.WriteLine($"[ERROR] エラーログを出力しました: {path}");

                DeleteOldLogFiles();
            }
            catch
            {

            }
        }

        private string GetTodayLogFilePath()
        {
            string fileName = $"error_{DateTime.Now:yyyyMMdd}.log";
            return Path.Combine(_logDire, fileName);
        }

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
