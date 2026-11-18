namespace WpfApp.Common.Models
{
    public class WorkLandownerData
    {
        /// <summary>
        /// 事業と地権者の紐付け識別用Guid
        /// </summary>
        public string WorkLandownerGuid { get; set; } = string.Empty;

        /// <summary>
        /// 事業ID
        /// </summary>
        public int WorkId { get; set; }

        /// <summary>
        /// 地権者ID
        /// </summary>
        public int LandownerId { get; set; }

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
