using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfApp.Interfaces;

namespace WpfApp.Screens.Admin.ViewModels
{
    /// <summary>
    /// EditButtonWindow.xamlに対応するViewModel。
    /// ボタンの名称編集画面での処理を行う。
    /// </summary>
    public partial class EditButtonViewModel : ObservableObject, IDialogResult<string>
    {
        public event EventHandler? RequestClose;
        public string? Result { get; private set; }

        /// <summary>
        /// 最大入力可能文字数
        /// </summary>
        private const double MaxFullWidthLength = 10;

        /// <summary>
        /// ボタンの名称（１行目）
        /// </summary>
        [ObservableProperty] private string line1;

        /// <summary>
        /// ボタンの名称（２行目）
        /// </summary>
        [ObservableProperty] private string line2;

        /// <summary>
        /// ボタンの名称（３行目）
        /// </summary>
        [ObservableProperty] private string line3;

        /// <summary>
        /// 入力可能文字数（１行目）
        /// </summary>
        [ObservableProperty] private double line1Remaining;

        /// <summary>
        /// 入力可能文字数（２行目）
        /// </summary>
        [ObservableProperty] private double line2Remaining;

        /// <summary>
        /// 入力可能文字数（３行目）
        /// </summary>
        [ObservableProperty] private double line3Remaining;

        /// <summary>
        /// プレビューエリアに表示するボタンの名称
        /// </summary>
        public string PreviewText => string.Join("\n", new[] { Line1, Line2, Line3 }.Where(l => !string.IsNullOrWhiteSpace(l)));

        public EditButtonViewModel(string text)
        {
            var lines = (text ?? "").Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            Line1 = TruncateByFullWidth(lines.ElementAtOrDefault(0) ?? "");
            Line2 = TruncateByFullWidth(lines.ElementAtOrDefault(1) ?? "");
            Line3 = TruncateByFullWidth(lines.ElementAtOrDefault(2) ?? "");

            UpdateRemaining();
        }

        /// <summary>
        /// ボタンの名称（１行目）を更新する。
        /// </summary>
        /// <param name="value"></param>
        partial void OnLine1Changed(string value)
        {
            Line1 = TruncateByFullWidth(value);
            UpdateRemaining();
            OnPropertyChanged(nameof(PreviewText));
        }

        /// <summary>
        /// ボタンの名称（２行目）を更新する。
        /// </summary>
        /// <param name="value"></param>
        partial void OnLine2Changed(string value)
        {
            Line2 = TruncateByFullWidth(value);
            UpdateRemaining();
            OnPropertyChanged(nameof(PreviewText));
        }

        /// <summary>
        /// ボタンの名称（３行目）を更新する。
        /// </summary>
        /// <param name="value"></param>
        partial void OnLine3Changed(string value)
        {
            Line3 = TruncateByFullWidth(value);
            UpdateRemaining();
            OnPropertyChanged(nameof(PreviewText));
        }

        /// <summary>
        /// 入力可能文字数を更新する。
        /// </summary>
        private void UpdateRemaining()
        {
            Line1Remaining = Math.Max(0, MaxFullWidthLength - CountFullWidthChars(Line1));
            Line2Remaining = Math.Max(0, MaxFullWidthLength - CountFullWidthChars(Line2));
            Line3Remaining = Math.Max(0, MaxFullWidthLength - CountFullWidthChars(Line3));
        }

        /// <summary>
        /// 文字列を最大入力可能文字数までにして返す。
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string TruncateByFullWidth(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            double count = 0;
            int index = 0;

            while (index < text.Length)
            {
                char c = text[index];
                double add = IsFullWidth(c) ? 1 : 0.5;

                if (count + add > MaxFullWidthLength)
                    break;

                count += add;
                index++;
            }

            return text.Substring(0, index);
        }

        /// <summary>
        /// 文字列の文字数をカウントする。
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static double CountFullWidthChars(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;

            double count = 0;
            foreach (char c in text)
                count += IsFullWidth(c) ? 1 : 0.5;

            return count;
        }

        /// <summary>
        /// 文字のカウント方法を判定する。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static bool IsFullWidth(char c)
        {
            // Unicodeの全角文字 + ASCII英数字を全角扱いにする
            if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                return true; // 英数字も全角扱いにする

            // 通常の全角文字範囲
            return (c >= 0x1100 && c <= 0x115F) ||
                   (c >= 0x2329 && c <= 0x232A) ||
                   (c >= 0x2E80 && c <= 0xA4CF) ||
                   (c >= 0xAC00 && c <= 0xD7A3) ||
                   (c >= 0xF900 && c <= 0xFAFF) ||
                   (c >= 0xFE10 && c <= 0xFE19) ||
                   (c >= 0xFE30 && c <= 0xFE6F) ||
                   (c >= 0xFF01 && c <= 0xFF60) ||
                   (c >= 0xFFE0 && c <= 0xFFE6);
        }

        /// <summary>
        /// 編集後の名称を返して画面を閉じる。
        /// </summary>
        [RelayCommand]
        private void Confirm()
        {
            Result = string.Join("\n", new[] { Line1, Line2, Line3 }.Where(l => !string.IsNullOrWhiteSpace(l)));
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 編集状態を初期化して画面を閉じる。
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            Result = null;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
