using CommunityToolkit.Mvvm.ComponentModel;
using WpfApp.Common.Models;

namespace WpfApp.Screens.Admin.Models
{
    /// <summary>
    /// チャプターのデータを編集用の形式で格納する。
    /// </summary>
    public partial class ChapterItemData : ObservableObject
    {
        [ObservableProperty]
        public string title = string.Empty;

        [ObservableProperty]
        public TimeSpan start;

        [ObservableProperty]
        public bool isProtected;

        /// <summary>
        /// 一覧表示用ラベル
        /// </summary>
        [ObservableProperty]
        public string displayLabel = string.Empty;

        public ChapterItemData(ChapterData data)
        {
            Title = data.Title;
            Start = TimeSpan.FromSeconds(data.Start);
            IsProtected = data.IsProtected;
            UpdateDisplayLabel();
        }

        public int StartHour
        {
            get => Start.Hours;
            set => UpdateStart(value, StartMinute, StartSecond, StartFraction);
        }

        public int StartMinute
        {
            get => Start.Minutes;
            set => UpdateStart(StartHour, value, StartSecond, StartFraction);
        }

        public int StartSecond
        {
            get => Start.Seconds;
            set => UpdateStart(StartHour, StartMinute, value, StartFraction);
        }

        // 0～9（0.1秒単位）
        public int StartFraction
        {
            get => Start.Milliseconds / 100;
            set => UpdateStart(StartHour, StartMinute, StartSecond, value);
        }

        private void UpdateStart(int h, int m, int s, int f)
        {
            h = Math.Clamp(h, 0, 23);
            m = Math.Clamp(m, 0, 59);
            s = Math.Clamp(s, 0, 59);
            f = Math.Clamp(f, 0, 9);

            Start = new TimeSpan(0, h, m, s, f * 100);
        }

        private void UpdateDisplayLabel()
        {
            DisplayLabel = $"【{Start:hh\\:mm\\:ss\\.f}】 {Title}";
        }

        partial void OnTitleChanged(string? oldValue, string newValue)
        {
            UpdateDisplayLabel();
        }

        partial void OnStartChanged(TimeSpan oldValue, TimeSpan newValue)
        {
            UpdateDisplayLabel();
            OnPropertyChanged(nameof(StartHour));
            OnPropertyChanged(nameof(StartMinute));
            OnPropertyChanged(nameof(StartSecond));
            OnPropertyChanged(nameof(StartFraction));
        }

        public ChapterData ToData() => new ChapterData
        {
            Title = this.Title,
            Start = this.Start.TotalSeconds
        };
    }
}
