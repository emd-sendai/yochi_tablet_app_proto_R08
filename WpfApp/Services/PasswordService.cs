using System.Security.Cryptography;
using System.Text;

namespace WpfApp.Services
{
    public class PasswordService
    {
        private readonly string _storedEncryptedPassword;

        public PasswordService()
        {
            string initialPassword = "0000";
            byte[] encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(initialPassword),
                Encoding.UTF8.GetBytes("CommonMasterKey"), 
                DataProtectionScope.CurrentUser
            );

            _storedEncryptedPassword = Convert.ToBase64String(encrypted);

        }

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
