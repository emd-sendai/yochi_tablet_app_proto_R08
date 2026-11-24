using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfApp.Common.Models
{
    /// <summary>
    /// 事業データを保持するクラス。
    /// </summary>
    public partial class WorkData : ObservableObject
    {
        /// <summary>
        /// 事業ID
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FormattedWorkId))]
        [NotifyPropertyChangedFor(nameof(DisplayLabel))]
        private int workId;

        /// <summary>
        /// 事業ID（6桁の文字列）
        /// </summary>
        public string FormattedWorkId => WorkId.ToString("D6");

        /// <summary>
        /// 事業識別用Guid
        /// </summary>
        [ObservableProperty]
        private string workGuid = string.Empty;

        /// <summary>
        /// 事業名
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayLabel))]
        private string workName = string.Empty;

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
        /// 一覧表示用（事業ID（6桁の文字列）：事業名）
        /// </summary>
        public string DisplayLabel => WorkId.ToString("D6") + "：" + WorkName;

    }
}
