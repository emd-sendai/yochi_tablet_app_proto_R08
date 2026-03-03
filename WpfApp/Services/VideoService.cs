using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WpfApp.Common.Models;
using WpfApp.Interfaces;
using WpfApp.Screens.Admin.Models;
using static WpfApp.Common.ViewModels.VideoPlayerViewModel;

namespace WpfApp.Services
{
    /// <summary>
    /// アプリ内の動画ファイルやチャプター設定ファイルのパスの取得を行うサービスクラス。
    /// （動画ファイルの差し替えやチャプター設定ファイルの編集処理については作成中）
    /// </summary>
    public class VideoService : IVideoService
    {
        /// <summary>
        /// 動画ファイルのパスを取得する。
        /// </summary>
        /// <param name="direPath"></param>
        /// <returns></returns>
        public string GetVideoFilePath(string direPath)
        {
            if (!Directory.Exists(direPath))
            {
                return string.Empty;
            }
            else
            {
                var list = Directory.GetFiles(direPath, "*.mp4").ToList();

                if (list.Count > 0)
                {
                    return list[0];
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// チャプターファイルのパスを取得する。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IReadOnlyList<Chapter> GetVideoChapterList(string path)
        {
            if (!File.Exists(path)) return Array.Empty<Chapter>();

            var json = File.ReadAllText(path);
            var list = JsonSerializer.Deserialize<List<ChapterData>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ChapterData>();
            Debug.WriteLine($"[DEBUG] チャプターを読み込みました: {path}");

            return list
                .OrderBy(c => c.Start)
                .Select(c => new Chapter(c.Title, TimeSpan.FromSeconds(c.Start)))
                .ToList();
        }

        /// <summary>
        /// チャプターデータを編集用の形式（ChapterItemData）で取得する。（作成中）
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ObservableCollection<ChapterItemData> GetVideoChapterDatas(string path)
        {
            if (!File.Exists(path)) return new ObservableCollection<ChapterItemData>();

            var json = File.ReadAllText(path);
            var list = JsonSerializer.Deserialize<List<ChapterData>>(json, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ChapterData>();
            Debug.WriteLine($"[DEBUG] チャプターを読み込みました: {path}");

            return new ObservableCollection<ChapterItemData>(
                list.OrderBy(c => c.Start)
                    .Select(c => new ChapterItemData(c)));
        }

        /// <summary>
        /// 動画ファイルを差し替えする。（作成中）
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sourceFilePath"></param>
        public void ChangeVideoFile(VideoData data, string sourceFilePath)
        {
            // 既存の動画ファイルをDirectoryごと削除
            DeleteVideoDirectory(data);
            // 動画ファイルをコピーしてDirectoryごと作成
            AddVideoFile(data, sourceFilePath);
        }

        /// <summary>
        /// 動画ファイルをディレクトリごと削除する。（作成中）
        /// </summary>
        /// <param name="data"></param>
        private void DeleteVideoDirectory(VideoData data)
        {
            if (Directory.Exists(data.DirectoryPath))
            {
                Directory.Delete(data.DirectoryPath, true);
                Debug.WriteLine($"[DEBUG] Directoryを削除しました: {data.DirectoryPath}");
            }
        }

        /// <summary>
        /// 動画ファイルをディレクトリごと追加する。（作成中）
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sourceFilePath"></param>
        private void AddVideoFile(VideoData data, string sourceFilePath)
        {
            Directory.CreateDirectory(data.DirectoryPath);

            var fileName = Path.GetFileName(sourceFilePath);
            var addFilePath = Path.Combine(data.DirectoryPath, fileName);

            File.Copy(sourceFilePath, addFilePath);
            Debug.WriteLine($"[DEBUG] mp4ファイルを追加しました: {addFilePath}");
        }

        /// <summary>
        /// チャプター設定データを初期化する。（作成中）
        /// 動画ファイル差し替え時に実行予定。
        /// </summary>
        /// <param name="path"></param>
        public void ClearChapter(string path)
        {
            if (!File.Exists(path)) { return; }

            var chapter = new List<ChapterData>()
            {
                new ChapterData { Title = "Start", Start = 0, IsProtected = true }  // 初期値（開始チャプターのみ）
            };

            // チャプター設定を初期化する
            File.WriteAllText(path, JsonSerializer.Serialize(chapter, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
