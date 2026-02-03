namespace WpfApp.Common.Models
{
    /// <summary>
    /// チャプターのデータを格納する。
    /// </summary>
    public class ChapterData
    {
        public string Title { get; set; } = string.Empty;
        public double Start { get; set; }

        /// <summary>
        /// Startの値の変更、チャプター自体の削除か判定する（管理メニューでのみ使用）。
        /// Titleは常に変更可能とする。
        /// </summary>
        public bool IsProtected { get; set; }
    }
}
