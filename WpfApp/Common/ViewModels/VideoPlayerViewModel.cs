using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using WpfApp.Common.Models;
using WpfApp.Interfaces;
using WpfApp.Services;

namespace WpfApp.Common.ViewModels
{
    public partial class VideoPlayerViewModel : ObservableObject
    {
        private readonly IMessageDialogService _message;
        private readonly IAppPathService _path;

        public event Action? PlayRequested;
        public event Action? PauseRequested;
        public event Action<TimeSpan>? SeekRequested;

        [ObservableProperty]
        private bool isPlaying;

        [ObservableProperty]
        private Uri? videoSource;

        [ObservableProperty]
        private TimeSpan duration;

        [ObservableProperty]
        private TimeSpan currentPosition;

        public double CurrentPositionSeconds
        {
            get => CurrentPosition.TotalSeconds;
            set => CurrentPosition = TimeSpan.FromSeconds(value);
        }

        public string CurrentTimeText => CurrentPosition.ToString(@"hh\:mm\:ss");
        public string DurationText => Duration.ToString(@"hh\:mm\:ss");
        public string TimeDisplay => $"{CurrentTimeText} / {DurationText}";

        partial void OnCurrentPositionChanged(TimeSpan oldValue, TimeSpan newValue)
        {
            OnPropertyChanged(nameof(CurrentPositionSeconds));
            OnPropertyChanged(nameof(CurrentTimeText));
            OnPropertyChanged(nameof(TimeDisplay));
        }

        partial void OnDurationChanged(TimeSpan oldValue, TimeSpan newValue)
        {
            OnPropertyChanged(nameof(DurationText));
            OnPropertyChanged(nameof(TimeDisplay));
        }

        public record Chapter(string Title, TimeSpan Start);

        public ObservableCollection<Chapter> Chapters { get; }

        public enum MediaCategory
        {
            Main,
            Parts
        }

        public VideoPlayerViewModel(string folderPath, MediaCategory category)
        {
            _message = new MessageDialogService();
            _path = new AppPathService();

            // 動画のパスを取得する
            var path = GetVideoPath(folderPath);

            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path)){
                VideoSource = new Uri(path);
                Debug.WriteLine($"[DEBUG] 動画のパスを設定しました: {path}");
            }
            else
            {
                _message.ShowMessage("動画が存在しません。");
                Debug.WriteLine($"[ERROR] 動画が存在しません: {path}");
            }

            // 全体動画の場合
            if (category == MediaCategory.Main)
            {
                // チャプターを取得する
                var chapters = LoadChapers(_path.MediaMeinAramashiChapterPath);
                // チャプターを設定する
                Chapters = new ObservableCollection<Chapter>(chapters);

                if (!Chapters.Any())
                {
                    Chapters.Add(new Chapter("Start", TimeSpan.Zero));
                }
            }
            // 個別動画の場合
            else
            {
                // チャプター（開始時間のみ）を設定する
                Chapters = new ObservableCollection<Chapter>();
                Chapters.Add(new Chapter("Start", TimeSpan.Zero));
            }
        }

        private string GetVideoPath(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                return string.Empty;
            }
            else
            {
                var list = Directory.GetFiles(folderPath, "*.mp4").ToList();

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

        private static IReadOnlyList<Chapter> LoadChapers(string path)
        {
            if(!File.Exists(path)) return Array.Empty<Chapter>();

            var json = File.ReadAllText(path);
            var list = JsonSerializer.Deserialize<List<ChapterData>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ChapterData>();
            Debug.WriteLine($"[DEBUG] チャプターを読み込みました: {path}");

            return list
                .OrderBy(c => c.Start)
                .Select(c => new Chapter(c.Title,TimeSpan.FromSeconds(c.Start)))
                .ToList();
        }

        // セクションジャンプ
        public void JumpTo(TimeSpan time) => SeekRequested?.Invoke(time);

        [RelayCommand]
        private void Play() => PlayRequested?.Invoke();

        [RelayCommand]
        private void Pause() => PauseRequested?.Invoke();

        [RelayCommand]
        private void Forward() => SeekRequested?.Invoke(CurrentPosition + TimeSpan.FromSeconds(10));


        [RelayCommand]
        private void Rewind() => SeekRequested?.Invoke(
            CurrentPosition > TimeSpan.FromSeconds(10)? CurrentPosition - TimeSpan.FromSeconds(10): TimeSpan.Zero);

        private int GetCurrentChapterIndex()
        {
            var current = CurrentPosition;
            var list = Chapters;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (current >= list[i].Start)
                    return i;
            }

            return -1;
        }

        [RelayCommand]
        private void NextChapter()
        {
            var list = Chapters;
            var index = GetCurrentChapterIndex();
            var nextIndex = index + 1;

            if (nextIndex >= 0 && nextIndex < list.Count)
            {
                SeekRequested?.Invoke(list[nextIndex].Start);
            }
        }

        [RelayCommand]
        private void PreviousChapter()
        {
            var list = Chapters;
            var index = GetCurrentChapterIndex();
            if (index < 0) return;

            var chapterStart = Chapters[index].Start;
            if (CurrentPosition - chapterStart > TimeSpan.FromSeconds(3))
            {
                SeekRequested?.Invoke(chapterStart);
            }
            else if (index > 0)
            {
                SeekRequested?.Invoke(Chapters[index - 1].Start);
            }
        }

        [RelayCommand]
        public void Close(Window window) => window.Close();
    }
}
