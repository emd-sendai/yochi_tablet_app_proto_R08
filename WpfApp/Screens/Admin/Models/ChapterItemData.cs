using CommunityToolkit.Mvvm.ComponentModel;
using WpfApp.Common.Models;

namespace WpfApp.Screens.Admin.Models
{
    /// <summary>
    /// チャプターのデータを編集用の形式で保持するクラス。
    /// </summary>
    public partial class ChapterItemData : ObservableObject
    {
        /// <summary>
        /// チャプター名
        /// </summary>
        [ObservableProperty]
        public string title = string.Empty;

        /// <summary>
        /// チャプター開始時間（TimeSpan）
        /// </summary>
        [ObservableProperty]
        public TimeSpan start;

        /// <summary>
        /// 編集可能フラグ（チャプター開始時間の編集とチャプター自体の削除が可能かどうか）
        /// </summary>
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

        /// <summary>
        /// 入力用チャプター開始時間（hh）
        /// </summary>
        public int StartHour
        {
            get => Start.Hours;
            set => UpdateStart(value, StartMinute, StartSecond, StartFraction);
        }

        /// <summary>
        /// 入力用チャプター開始時間（mm）
        /// </summary>
        public int StartMinute
        {
            get => Start.Minutes;
            set => UpdateStart(StartHour, value, StartSecond, StartFraction);
        }

        /// <summary>
        /// 入力用チャプター開始時間（ss）
        /// </summary>
        public int StartSecond
        {
            get => Start.Seconds;
            set => UpdateStart(StartHour, StartMinute, value, StartFraction);
        }

        /// <summary>
        /// 入力用チャプター開始時間（f）
        /// </summary>
        public int StartFraction
        {
            get => Start.Milliseconds / 100;
            set => UpdateStart(StartHour, StartMinute, StartSecond, value);
        }

        /// <summary>
        /// 入力用の各チャプター開始時間を更新する。
        /// </summary>
        /// <param name="h"></param>
        /// <param name="m"></param>
        /// <param name="s"></param>
        /// <param name="f"></param>
        private void UpdateStart(int h, int m, int s, int f)
        {
            h = Math.Clamp(h, 0, 23);
            m = Math.Clamp(m, 0, 59);
            s = Math.Clamp(s, 0, 59);
            f = Math.Clamp(f, 0, 9);

            Start = new TimeSpan(0, h, m, s, f * 100);
        }

        /// <summary>
        /// 一覧表示用ラベルを更新する。
        /// </summary>
        private void UpdateDisplayLabel()
        {
            DisplayLabel = $"【{Start:hh\\:mm\\:ss\\.f}】 {Title}";
        }

        /// <summary>
        /// チャプター名が変更されたときに一覧表示用ラベルを更新する。
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        partial void OnTitleChanged(string? oldValue, string newValue)
        {
            UpdateDisplayLabel();
        }

        /// <summary>
        /// チャプター開始時間が変更されたときに一覧表示用ラベルと入力用変数を更新する。
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        partial void OnStartChanged(TimeSpan oldValue, TimeSpan newValue)
        {
            UpdateDisplayLabel();
            OnPropertyChanged(nameof(StartHour));
            OnPropertyChanged(nameof(StartMinute));
            OnPropertyChanged(nameof(StartSecond));
            OnPropertyChanged(nameof(StartFraction));
        }

        /// <summary>
        /// ChapterDataの形式に格納して返す。
        /// </summary>
        /// <returns></returns>
        public ChapterData ToData() => new ChapterData
        {
            Title = this.Title,
            Start = this.Start.TotalSeconds
        };
    }
}
