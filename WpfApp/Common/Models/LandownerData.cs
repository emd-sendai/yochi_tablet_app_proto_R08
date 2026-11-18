namespace WpfApp.Common.Models
{
    /// <summary>
    /// 地権者データを保持するクラス。
    /// </summary>
    public class LandownerData
    {
        /// <summary>
        /// 地権者ID
        /// </summary>
        public int LandownerId { get; set; }

        /// <summary>
        /// 地権者識別用Guid
        /// </summary>
        public string LandownerGuid { get; set; } = string.Empty;

        /// <summary>
        /// 地権者氏名（漢字）
        /// </summary>
        public string LandownerName { get; set; } = string.Empty;

        /// <summary>
        /// 地権者氏名（かな）
        /// </summary>
        public string LandownerKana {  get; set; } = string.Empty;

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
