namespace WpfApp.Common.Models
{
    /// <summary>
    /// 事業データを保持するクラス。
    /// </summary>
    public class WorkData
    {
        /// <summary>
        /// 事業ID
        /// </summary>
        public int WorkId { get; set; }

        /// <summary>
        /// 事業識別用Guid
        /// </summary>
        public string WorkGuid { get; set; } = string.Empty;

        /// <summary>
        /// 事業名
        /// </summary>
        public string WorkName { get; set; } = string.Empty;

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
