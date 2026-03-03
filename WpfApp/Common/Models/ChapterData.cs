namespace WpfApp.Common.Models
{
    /// <summary>
    /// チャプターのデータを保持するクラス。
    /// </summary>
    public class ChapterData
    {
        /// <summary>
        /// チャプターの名称
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// チャプターの開始時間
        /// </summary>
        public double Start { get; set; }

        /// <summary>
        /// Startの値の変更やチャプター自体の削除か判定するためフラグ（管理メニューでのみ使用）。
        /// Titleは常に変更可能とする。
        /// </summary>
        public bool IsProtected { get; set; }
    }
}
