using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WpfApp.Common.Models;
using WpfApp.Interfaces;
using WpfApp.Screens.Admin.Models;
using static WpfApp.Common.ViewModels.VideoPlayerViewModel;

namespace WpfApp.Services
{
    public class VideoService : IVideoService
    {
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

        public void ChangeVideoFile(VideoData data, string sourceFilePath)
        {
            // 既存の動画ファイルをDirectoryごと削除
            DeleteVideoDirectory(data);
            // 動画ファイルをコピーしてDirectoryごと作成
            AddVideoFile(data, sourceFilePath);
        }

        private void DeleteVideoDirectory(VideoData data)
        {
            if (Directory.Exists(data.DirectoryPath))
            {
                Directory.Delete(data.DirectoryPath, true);
                Debug.WriteLine($"[DEBUG] Directoryを削除しました: {data.DirectoryPath}");
            }
        }

        private void AddVideoFile(VideoData data, string sourceFilePath)
        {
            Directory.CreateDirectory(data.DirectoryPath);

            var fileName = Path.GetFileName(sourceFilePath);
            var addFilePath = Path.Combine(data.DirectoryPath, fileName);

            File.Copy(sourceFilePath, addFilePath);
            Debug.WriteLine($"[DEBUG] mp4ファイルを追加しました: {addFilePath}");
        }

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
