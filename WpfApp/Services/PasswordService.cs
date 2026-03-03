using System.Security.Cryptography;
using System.Text;

namespace WpfApp.Services
{
    /// <summary>
    /// パスワードを照合するサービスクラス。
    /// </summary>
    public class PasswordService
    {
        private readonly string _storedEncryptedPassword;

        public PasswordService()
        {
            // 現在は直接定義しているが、パスワード自体をファイル管理にして変更可能にする改良の余地あり
            string initialPassword = "0000";
            byte[] encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(initialPassword),
                Encoding.UTF8.GetBytes("CommonMasterKey"), 
                DataProtectionScope.CurrentUser
            );

            _storedEncryptedPassword = Convert.ToBase64String(encrypted);

        }

        /// <summary>
        /// パスワードを照合する。
        /// </summary>
        /// <param name="inputPassword"></param>
        /// <returns></returns>
        public bool Verify(string inputPassword)
        {
            try
            {
                byte[] decrypted = ProtectedData.Unprotect(
                    Convert.FromBase64String(_storedEncryptedPassword),
                    Encoding.UTF8.GetBytes("CommonMasterKey"),
                    DataProtectionScope.CurrentUser
                );

                string storedPassword = Encoding.UTF8.GetString(decrypted);
                return inputPassword == storedPassword;
            }
            catch
            {
                return false;
            }
        }
    }
}
