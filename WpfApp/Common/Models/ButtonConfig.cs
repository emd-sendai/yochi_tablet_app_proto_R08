using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfApp.Common.Models
{
    /// <summary>
    /// ボタンの設定データを保持するクラス。
    /// </summary>
    public partial class ButtonConfig : ObservableObject
    {
        /// <summary>
        /// ボタンID
        /// </summary>
        [ObservableProperty]
        private string btnId = string.Empty;

        /// <summary>
        /// ボタンの名称
        /// </summary>
        [ObservableProperty]
        private string btnText = string.Empty;

        /// <summary>
        /// 一覧表示用ラベル（管理メニューでのみ使用）
        /// </summary>
        public string LabelText => BtnText?.Replace("\r", "").Replace("\n", " ") ?? "";

        /// <summary>
        /// サイドバー表示用ラベル
        /// </summary>
        public string SidebarLabelText => BtnText?.Replace("\r", "").Replace("\n", "") ?? "";

        /// <summary>
        /// ボタンの名称が変更されたとき、一覧表示用ラベルを変更する。
        /// </summary>
        /// <param name="value"></param>
        partial void OnBtnTextChanged(string value)
        {
            OnPropertyChanged(nameof(LabelText));
            OnPropertyChanged(nameof(SidebarLabelText));
        }

    }
}
