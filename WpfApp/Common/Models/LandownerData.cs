namespace WpfApp.Common.Models
{
    /// <summary>
    /// 地権者ユーザのデータを保持するクラス。
    /// </summary>
    public class LandownerData
    {
        /// <summary>
        /// 地権者ユーザID
        /// </summary>
        public string LandownerId { get; set; } = string.Empty;

        /// <summary>
        /// 地権者ユーザ名
        /// </summary>
        public string LandownerName { get; set; } = string.Empty;
    }
}
