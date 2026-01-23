using System.IO;
using WpfApp.Interfaces;

namespace WpfApp.Services
{
    public class ResourceInitializeService
    {
        private readonly IAppPathService _path;

        private static readonly string AppDire = AppDomain.CurrentDomain.BaseDirectory;

        public ResourceInitializeService(IAppPathService appPathService)
        {
            _path = appPathService;
        }

        public void InitializeIfNeeded()
        {
            if (File.Exists(_path.InitializeFlagFile)) { return; }

            var sourceRoot = Path.Combine(AppDire, "Data");
            CopyDirectoryIfNotExists(sourceRoot, _path.DataDire);

            sourceRoot = Path.Combine(AppDire, "DocumentResources");
            CopyDirectoryIfNotExists(sourceRoot, _path.DocumentResourcesRoot);

            sourceRoot = Path.Combine(AppDire, "MediaResources");
            CopyDirectoryIfNotExists(sourceRoot, _path.MediaResourcesRoot);

            sourceRoot = Path.Combine(AppDire, "SlideResources");
            CopyDirectoryIfNotExists(sourceRoot, _path.SlideResourcesRoot);

            // 初期化完了フラグ
            File.WriteAllText(_path.InitializeFlagFile, DateTime.Now.ToString("O"));
        }

        private void CopyDirectoryIfNotExists(string sourceDir, string targetDir)
        {
            // コピー先のディレクトリ作成
            Directory.CreateDirectory(targetDir);

            // ファイルコピー
            foreach (var filePath in Directory.GetFiles(sourceDir))
            {
                var fileName = Path.GetFileName(filePath);
                var destFile = Path.Combine(targetDir, fileName);

                if (!File.Exists(destFile))
                {
                    File.Copy(filePath, destFile);
                }
            }

            // サブディレクトリ再帰コピー
            foreach (var subDir in Directory.GetDirectories(sourceDir))
            {
                var dirName = Path.GetFileName(subDir);
                var destSubDir = Path.Combine(targetDir, dirName);

                CopyDirectoryIfNotExists(subDir, destSubDir);
            }
        }
    }
}
