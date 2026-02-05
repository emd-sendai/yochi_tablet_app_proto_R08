using System.Diagnostics;
using System.IO;
using WpfApp.Interfaces;

namespace WpfApp.Services
{
    /// <summary>
    /// アプリ内で使用する初期ファイルの配置、削除を行う。
    /// </summary>
    public class ResourceInitializeService
    {
        private readonly IAppPathService _path;
        private readonly IResourcePathService _sourcePath;
        private readonly ILoggingService _log;
        private readonly IMessageDialogService _message;

        public ResourceInitializeService(IAppPathService appPathService)
        {
            _path = appPathService;
            _sourcePath = new ResourcePathService();
            _log = new LoggingService();
            _message = new MessageDialogService();
        }

        /// <summary>
        /// アプリ初回起動時に初期ファイルをLocalAppDataにコピーする
        /// </summary>
        public void InitializeIfNeeded()
        {
            // 初期ファイル配置完了フラグが存在する場合は処理を行わない
            if (File.Exists(_path.InitializeFlagFile)) { return; }

            // 設定その他ファイル
            CopyDirectoryIfNotExists(_sourcePath.DataDire, _path.DataDire);
            // PDFファイル
            CopyDirectoryIfNotExists(_sourcePath.DocumentResourcesRoot, _path.DocumentResourcesRoot);
            // 動画関連ファイル
            CopyDirectoryIfNotExists(_sourcePath.MediaResourcesRoot, _path.MediaResourcesRoot);
            // 補償のあらましスライドファイル
            CopyDirectoryIfNotExists(_sourcePath.SlideResourcesRoot, _path.SlideResourcesRoot);

            // 初期ファイル配置完了フラグを作成する
            File.WriteAllText(_path.InitializeFlagFile, DateTime.Now.ToString("O"));
        }

        /// <summary>
        /// sourceDirの内容をサブディレクトリごとtargetDirにコピーする
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="targetDir"></param>
        private void CopyDirectoryIfNotExists(string sourceDir, string targetDir)
        {
            // コピー先のディレクトリを作成する
            Directory.CreateDirectory(targetDir);

            // ファイルをコピーする
            foreach (var filePath in Directory.GetFiles(sourceDir))
            {
                var fileName = Path.GetFileName(filePath);
                var destFile = Path.Combine(targetDir, fileName);

                if (!File.Exists(destFile))
                {
                    File.Copy(filePath, destFile);
                }
            }

            // サブディレクトリを再帰コピーする
            foreach (var subDir in Directory.GetDirectories(sourceDir))
            {
                var dirName = Path.GetFileName(subDir);
                var destSubDir = Path.Combine(targetDir, dirName);

                CopyDirectoryIfNotExists(subDir, destSubDir);
            }
        }

        /// <summary>
        /// 初期ファイルのうち必須ファイルが存在しない場合はLocalAppDataにコピーする
        /// </summary>
        public void EnsureRequiredResources()
        {
            // 地権者ユーザデータファイル
            CopyFileIfNotExists(_sourcePath.LandownersPath, _path.DataDire, _path.LandownersPath);

            // ボタン表示設定ファイル
            CopyDirectoryIfNotExists(_sourcePath.ButtonRoot, _path.ButtonRoot);

            // 直接参照されるPDFファイルが存在するディレクトリ
            // 補償のあらまし - その他3
            CopyDirectoryIfNotExists(_sourcePath.Others003PdfDire, _path.Others003PdfDire);
            // 補償のあらまし - その他4
            CopyDirectoryIfNotExists(_sourcePath.Others004PdfDire, _path.Others004PdfDire);

            // 動画関連ファイル（動画編集機能実装後はチャプターファイルのみとする）
            CopyDirectoryIfNotExists(_sourcePath.MediaResourcesRoot, _path.MediaResourcesRoot);
            // 動画チャプター設定ファイル
            //CopyFileIfNotExists(_sourcePath.MediaMeinAramashiChapterPath, _path.MediaMeinAramashiDire, _path.MediaMeinAramashiChapterPath);

            // 補償のあらましスライドファイル（スライド編集機能実装後に見直しの必要あり）
            CopyDirectoryIfNotExists(_sourcePath.SlideResourcesRoot, _path.SlideResourcesRoot);
        }

        /// <summary>
        /// targetPathが存在しない場合はsourcePathをコピーする
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetDire"></param>
        /// <param name="targetPath"></param>
        private void CopyFileIfNotExists(string sourcePath, string targetDire, string targetPath)
        {
            // 必須ファイルが存在する場合コピーしない
            if (File.Exists(targetPath)) { return; }

            // コピー先のディレクトリを作成する
            Directory.CreateDirectory(targetDire);
            // ファイルをコピーする
            File.Copy(sourcePath, targetPath);
        }

        /// <summary>
        /// アプリを初期化する（アンインストール用）
        /// </summary>
        public void CleanupResources()
        {
            if (!Directory.Exists(_path.AppRoot))
            {
                _message.ShowConfirm("アプリは既に初期化されています。");
                return;
            }

            try
            {
                Directory.Delete(_path.AppRoot, true);
                _message.ShowConfirm("アプリの初期化が完了しました。");
            }
            catch (IOException ex)
            {
                Debug.WriteLine($"[ERROR] アプリ内のファイルが使用中か読み取り専用のためディレクトリを削除できません");
                Debug.WriteLine($"[ERROR] {ex}");
                _log.LogError(ex);
                _message.ShowConfirm("アプリで使用中のファイルまたは他のアプリで開いているファイルがあります。" +
                    "\nファイルを閉じてから、もう一度お試しください。");
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"[ERROR] アクセス権が不足しているためディレクトリを削除できません");
                Debug.WriteLine($"[ERROR] {ex}");
                _log.LogError(ex);
                _message.ShowConfirm("アクセス権が不足しているため、ファイルを削除できません。");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] ディレクトリを削除できません");
                Debug.WriteLine($"[ERROR] {ex}");
                _log.LogError(ex);
                _message.ShowConfirm("ファイルを削除できません。\n管理者へお問い合わせください。");
            }
        }
    }
}
