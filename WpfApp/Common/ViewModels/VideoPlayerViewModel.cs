using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using WpfApp.Common.Models;

namespace WpfApp.Common.ViewModels
{
    public partial class VideoPlayerViewModel : ObservableObject
    {
        public Func<TimeSpan>? PositionProvider { get; set; }
        public Action<TimeSpan>? SeekAction { get; set; }

        public Action? PlayRequested;
        public Action? PauseRequested;
        public Action? StopRequested;

        private bool _isDragging;

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

        [ObservableProperty]
        private bool isPlaying;

        public DispatcherTimer Timer { get; }

        public record Chapter(string Title, TimeSpan Start);

        public ObservableCollection<Chapter> Chapters { get; }

        private TimeSpan Current => PositionProvider?.Invoke() ?? TimeSpan.Zero;


        public VideoPlayerViewModel(string videoFileName)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MediaResources", videoFileName);

            if (File.Exists(path)){
                VideoSource = new Uri(path);
                Debug.WriteLine($"[DEBUG] 動画のパスを設定しました: {path}");
            }
            else
            {
                MessageBox.Show("動画が存在しません");
                Debug.WriteLine($"[ERROR] 動画が存在しません: {path}");
            }

            var chapters = LoadChapers();
            Chapters = new ObservableCollection<Chapter>(chapters);

            if (!Chapters.Any())
            {
                Chapters.Add(new Chapter("Start", TimeSpan.Zero));
            }

            Timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            Timer.Tick += (_, _) =>
            {
                if (!_isDragging)
                    CurrentPosition = PositionProvider?.Invoke() ?? TimeSpan.Zero;
            };
        }

        private static IReadOnlyList<Chapter> LoadChapers()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MediaResources", "chapters.json");

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

        // シーク開始
        public void BeginSeek() => _isDragging = true;

        // シーク確定
        public void EndSeek()
        {
            SeekAction?.Invoke(CurrentPosition);
            _isDragging = false;
        }

        // セクションジャンプ
        public void JumpTo(TimeSpan time)
        {
            SeekAction?.Invoke(time);
        }

        [RelayCommand]
        private void Play()
        {
            IsPlaying = true;
            Timer.Start();
            PlayRequested?.Invoke();
        }

        [RelayCommand]
        private void Pause()
        {
            IsPlaying = false;
            Timer.Stop();
            PauseRequested?.Invoke();
        }

        [RelayCommand]
        private void TogglePlayPause()
        {
            if (IsPlaying)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        [RelayCommand]
        private void Forward()
        {
            SeekAction?.Invoke(CurrentPosition + TimeSpan.FromSeconds(10));
        }

        [RelayCommand]
        private void Rewind()
        {
            var target = CurrentPosition - TimeSpan.FromSeconds(10);
            if (target < TimeSpan.Zero) target = TimeSpan.Zero;
            SeekAction?.Invoke(target);
        }

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
                SeekAction?.Invoke(list[nextIndex].Start);
            }
        }

        [RelayCommand]
        private void PreviousChapter()
        {
            var list = Chapters;
            var index = GetCurrentChapterIndex();
            if (index < 0) return;

            var chapterStart = Chapters[index].Start;
            if (Current - chapterStart > TimeSpan.FromSeconds(3))
            {
                SeekAction?.Invoke(chapterStart);
            }
            else if (index > 0)
            {
                SeekAction?.Invoke(Chapters[index - 1].Start);
            }
        }

        [RelayCommand]
        public void Close(Window window) => window.Close();
    }
}
