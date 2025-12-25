using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

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

        public ObservableCollection<Chapter> Chapters { get; } = new()
        {
        // 動画と同じフォルダにチャプターJSONを作成して読み込む
        new("Intro",  TimeSpan.FromSeconds(0)),
        new("Part 1", TimeSpan.FromSeconds(56)),
        new("Part 2", TimeSpan.FromSeconds(96)),
        new("Part 3", TimeSpan.FromSeconds(204)),
        new("Part 4", TimeSpan.FromSeconds(271)),
        new("Part 5", TimeSpan.FromSeconds(372)),
        new("Outro",  TimeSpan.FromSeconds(475)),
        };

        private TimeSpan Current => PositionProvider?.Invoke() ?? TimeSpan.Zero;


        public VideoPlayerViewModel(string videoFileName)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MediaResources", videoFileName);

            if (File.Exists(path)){
                VideoSource = new Uri(path);
                Debug.WriteLine($"[DEBUG] 動画のパスを設定しました: {path}");
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
        private void Stop()
        {
            IsPlaying = false;
            Timer.Stop();
            StopRequested?.Invoke();
            SeekAction?.Invoke(TimeSpan.Zero);
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

        [RelayCommand]
        private void NextChapter()
        {
            var current = Current;

            var next = Chapters
                .OrderBy(c => c.Start)
                .FirstOrDefault(c => c.Start > current);

            if (next != null)
                SeekAction?.Invoke(next.Start);
        }

        [RelayCommand]
        private void PreviousChapter()
        {
            var current = Current;

            var prev = Chapters
                .OrderByDescending(c => c.Start)
                .FirstOrDefault(c => c.Start < current);

            if (prev != null)
                SeekAction?.Invoke(prev.Start);
        }

        [RelayCommand]
        public void Close(Window window) => window.Close();
    }
}
