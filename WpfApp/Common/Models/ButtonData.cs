using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfApp.Common.Models
{
    public partial class ButtonData : ObservableObject
    {
        /// <summary>
        /// ボタンタイプ（1:用地補償の手順、2:その他、3:事業全般資料、4:地権者個別資料）
        /// </summary>
        [ObservableProperty]
        private int type;

        /// <summary>
        /// ボタンID（ボタンタイプごとに採番）
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FormattedId))]
        [NotifyPropertyChangedFor(nameof(DocumentDireName))]
        private int id;

        /// <summary>
        /// ボタンID（3桁の文字列）（用地補償の手順、その他）
        /// </summary>
        public string FormattedId => Id.ToString("D3");

        /// <summary>
        /// 資料ディレクトリ名（事業全般資料、地権者個別資料）
        /// </summary>
        public string DocumentDireName => "Document" + Id.ToString("D2");

        /// <summary>
        /// ボタンに表示する文字列
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LabelText))]
        [NotifyPropertyChangedFor(nameof(SidebarLabelText))]
        private string content = string.Empty;

        /// <summary>
        /// 一覧表示用ラベル（管理メニュー）
        /// </summary>
        public string LabelText => Content.Replace("\r", "").Replace("\n", " ") ?? "";

        /// <summary>
        /// 一覧表示用ラベル（サイドバー）
        /// </summary>
        public string SidebarLabelText => Content.Replace("\r", "").Replace("\n", "") ?? "";

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
