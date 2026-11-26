using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using WpfApp.Common.Models;
using WpfApp.Common.Enums;
using WpfApp.Interfaces;
using WpfApp.Services;

namespace WpfApp.Common.ViewModels
{
    /// <summary>
    /// VideoPlayerWindow.xamlに対応するViewModel。
    /// 動画ファイル（mp4）の再生を行う。
    /// </summary>
    public partial class VideoPlayerViewModel : ObservableObject
    {
        private readonly IMessageDialogService _message;
        private readonly IAppPathService _path;
        private readonly IVideoService _video;

        /// <summary>
        /// Viewに対して動画の再生を要求するためのイベント。
        /// </summary>
        public event Action? PlayRequested;

        /// <summary>
        /// Viewに対して動画の再一時停止を要求するためのイベント。
        /// </summary>
        public event Action? PauseRequested;

        /// <summary>
        /// Viewに対して指定の時間への動画の再生時間の変更を要求するためのイベント。
        /// </summary>
        public event Action<TimeSpan>? SeekRequested;

        /// <summary>
        /// Viewに対して指定の速度へ動画の再生速度の変更を要求するためのイベント。
        /// </summary>
        public event Action<double>? SpeedChangedRequested;

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

        [ObservableProperty]
        private double selectedSpeed = 1.0;

        public List<PlaybackSpeedData> SpeedOptions { get; } = new()
        {
            new PlaybackSpeedData { Label = "標準", Value = 1.0 },
            new PlaybackSpeedData { Label = "1.5倍速", Value = 1.5 },
            new PlaybackSpeedData { Label = "2.0倍速", Value = 2.0 }
        };

        partial void OnSelectedSpeedChanged(double value)
        {
            SpeedChangedRequested?.Invoke(value);
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

        public VideoPlayerViewModel(string path, VideoCategory category)
        {
            _message = new MessageDialogService();
            _path = new AppPathService();
            _video = new VideoService();
            var filePath = string.Empty;

            try
            {
                // 補償のあらましの場合
                if (category == VideoCategory.Main || category == VideoCategory.Parts)
                {
                    // 動画ファイルのパスを取得する
                    filePath = _video.GetVideoFilePath(path);
                }
                // それ以外の場合
                else
                {
                    filePath = path;
                }

                if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
                {
                    VideoSource = new Uri(filePath);
                    Debug.WriteLine($"[DEBUG] 動画ファイルのパスを設定しました: {filePath}");
                }
                else
                {
                    _message.ShowMessage("動画ファイルが存在しません。");
                    Debug.WriteLine($"[ERROR] 動画ファイルが存在しません: {filePath}");
                }

                // 補償のあらまし（統合版動画）の場合
                if (category == VideoCategory.Main)
                {
                    // チャプターを取得する
                    var chapters = _video.GetVideoChapterList(_path.MediaMeinAramashiChapterPath);
                    // チャプターを設定する
                    Chapters = new ObservableCollection<Chapter>(chapters);

                    if (!Chapters.Any())
                    {
                        Chapters.Add(new Chapter("Start", TimeSpan.Zero));
                    }
                }
                // それ以外の場合
                else
                {
                    // チャプター（開始時間のみ）を設定する
                    Chapters = new ObservableCollection<Chapter>();
                    Chapters.Add(new Chapter("Start", TimeSpan.Zero));
                }
            }
            catch (Exception ex) 
            {
                // チャプター（開始時間のみ）を設定する
                Chapters = new ObservableCollection<Chapter>();
                Chapters.Add(new Chapter("Start", TimeSpan.Zero));

                Debug.WriteLine($"[ERROR] 動画ファイルの読み込みに失敗: {ex.Message}");
                _message.ShowMessage("動画ファイルの読み込みに失敗しました。");
                throw;
            }
        }

        /// <summary>
        /// 指定された時間に再生時間を変更する要求を、Viewのコードビハインドに通知する。
        /// </summary>
        /// <param name="time"></param>
        public void JumpTo(TimeSpan time) => SeekRequested?.Invoke(time);

        /// <summary>
        /// 動画を再生する要求を、Viewのコードビハインドに通知する。
        /// </summary>
        [RelayCommand]
        private void Play() => PlayRequested?.Invoke();

        /// <summary>
        /// 動画を一時停止する要求を、Viewのコードビハインドに通知する。
        /// </summary>
        [RelayCommand]
        private void Pause() => PauseRequested?.Invoke();

        /// <summary>
        /// 現在の再生時間から10秒前へ再生時間を変更する要求を、Viewのコードビハインドに通知する。
        /// </summary>
        [RelayCommand]
        private void Forward() => SeekRequested?.Invoke(CurrentPosition + TimeSpan.FromSeconds(10));

        /// <summary>
        /// 現在の再生時間から10秒後へ再生時間を変更する要求を、Viewのコードビハインドに通知する。
        /// </summary>
        [RelayCommand]
        private void Rewind() => SeekRequested?.Invoke(
            CurrentPosition > TimeSpan.FromSeconds(10)? CurrentPosition - TimeSpan.FromSeconds(10): TimeSpan.Zero);

        /// <summary>
        /// 現在のチャプター番号を取得する。
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 次のチャプターの開始時間へ再生時間を変更する要求を、Viewのコードビハインドに通知する。
        /// </summary>
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

        /// <summary>
        /// 前のチャプターの開始時間へ再生時間を変更する要求を、Viewのコードビハインドに通知する。
        /// 現在のチャプターの開始時間から3秒以上経過していない場合は、現在のチャプターの開始時間へ再生時間を変更する。
        /// </summary>
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

        /// <summary>
        /// 画面を閉じる。
        /// </summary>
        /// <param name="window"></param>
        [RelayCommand]
        public void Close(Window window) => window.Close();
    }
}
