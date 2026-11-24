using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfApp.Common.Models
{
    public partial class WorkLandownerData : ObservableObject
    {
        /// <summary>
        /// 事業と地権者の紐付け識別用Guid
        /// </summary>
        [ObservableProperty]
        private string workLandownerGuid = string.Empty;

        /// <summary>
        /// 事業ID
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FormattedWorkId))]
        private int workId;

        /// <summary>
        /// 事業ID（6桁の文字列）
        /// </summary>
        public string FormattedWorkId => WorkId.ToString("D6");

        /// <summary>
        /// 地権者ID
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FormattedLandownerId))]
        private int landownerId;

        /// <summary>
        /// 地権者ID（6桁の文字列）
        /// </summary>
        public string FormattedLandownerId => LandownerId.ToString("D6");

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
    }
}
