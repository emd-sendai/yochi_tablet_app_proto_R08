namespace WpfApp.Common.Models
{
    /// <summary>
    /// 動画ファイルのデータを格納するクラス。
    /// </summary>
    public class VideoData
    {
        /// <summary>
        /// 紐付けられたボタンの表示用ラベル（管理メニューでのみ使用）
        /// </summary>
        public string LinkedPartLabel { get; set; } = string.Empty;

        /// <summary>
        /// 動画ファイルのディレクトリパス
        /// </summary>
        public string DirectoryPath { get; set; } = string.Empty;
    }
}
