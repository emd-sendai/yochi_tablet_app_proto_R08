using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WpfApp.Common.Models;
using WpfApp.Interfaces;

namespace WpfApp.Services
{
    /// <summary>
    /// パスワードを照合するサービスクラス。
    /// </summary>
    public class PasswordService
    {
        private readonly IAppPathService _path;

        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 600000;

        public PasswordService()
        {
            _path = new AppPathService();
        }

        /// <summary>
        /// パスワード(string)をハッシュ化してPasswordDataで返す。
        /// </summary>
        /// <param name="password"></param>
        /// <returns>stringをハッシュ化したPasswordData</returns>
        public PasswordData HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

            return new PasswordData
            {
                PasswordHash = Convert.ToBase64String(hash),
                Salt = Convert.ToBase64String(salt),
                Iterations = Iterations
            };
        }

        /// <summary>
        /// パスワード(string)を、ハッシュ化して保存されたパスワード(PasswordData)と同一か照合する。
        /// </summary>
        /// <param name="password"></param>
        /// <param name="saveData"></param>
        /// <returns></returns>
        public bool Verify(string password, PasswordData saveData)
        {
            byte[] salt = Convert.FromBase64String(saveData.Salt);
            byte[] expectedHash = Convert.FromBase64String(saveData.PasswordHash);
            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, saveData.Iterations, HashAlgorithmName.SHA256, HashSize);

            return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }

        /// <summary>
        /// ハッシュ化して保存された管理者パスワードをPasswordDataで取得する。
        /// </summary>
        /// <returns>管理者パスワードデータ</returns>
        public PasswordData? LoasPasswordFromJson()
        {
            PasswordData? ret = null;
            string path = _path.AdminSettings;

            try
            {
                if (!File.Exists(path))
                {
                    Debug.WriteLine($"[ERROR] パスワードデータが見つかりません: {path}");
                    return ret;
                }

                string json = File.ReadAllText(path);
                ret = JsonSerializer.Deserialize<PasswordData>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return ret;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] パスワードデータを読み込むことができません: {ex}");
                return null;
            }
        }
    }
}
