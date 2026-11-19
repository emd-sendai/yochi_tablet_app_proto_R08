namespace WpfApp.Common.Models
{
    public class PasswordData
    {
        /// <summary>
        /// ID（DB上では1のみ許容）
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// パスワードハッシュ
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// ソルト
        /// </summary>
        public string Salt { get; set; } = string.Empty;

        /// <summary>
        /// イテレーション
        /// </summary>
        public int Iterations { get; set; }

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
