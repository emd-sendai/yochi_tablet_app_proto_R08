using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfApp.Common.Models
{
    /// <summary>
    /// 地権者データを保持するクラス。
    /// </summary>
    public partial class LandownerData : ObservableObject
    {
        /// <summary>
        /// 地権者ID
        /// </summary>
        [ObservableProperty]
        private int landownerId;

        /// <summary>
        /// 地権者ID（6桁の文字列）
        /// </summary>
        public string FormattedLandownerId => LandownerId.ToString("D6");

        /// <summary>
        /// 地権者識別用Guid
        /// </summary>
        [ObservableProperty]
        private string landownerGuid = string.Empty;

        /// <summary>
        /// 地権者氏名（漢字）
        /// </summary>
        [ObservableProperty]
        private string landownerName = string.Empty;

        /// <summary>
        /// 地権者氏名（かな）
        /// </summary>
        [ObservableProperty]
        private string landownerKana = string.Empty;

        /// <summary>
        /// 作成日時
        /// </summary>
        [ObservableProperty]
        private DateTime createdAt;

        /// <summary>
        /// 更新日時
        /// </summary>
        [ObservableProperty]
        private DateTime updatedAt;

        /// <summary>
        /// 一覧表示用（地権者ID（6桁の文字列）：地権者氏名（漢字））
        /// </summary>
        public string DisplayLabel => LandownerId.ToString("D6") + "：" + LandownerName;
    }
}
