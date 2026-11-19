using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Windows.Controls;
using WpfApp.Common.Repositories.Interfaces;
using WpfApp.Interfaces;
using WpfApp.Services;


namespace WpfApp.Common.ViewModels
{
    /// <summary>
    /// PasswordDialogWindow.xamlに対応するViewModel。
    /// パスワードダイアログ画面での処理を行う。
    /// </summary>
    public partial class PasswordDialogViewModel : ObservableObject, IDialogResult<bool>
    {
        private readonly PasswordService _passwordService;
        private readonly IAdminPasswordRepository _adminPasswordRepos;

        public event EventHandler? RequestClose;

        /// <summary>
        /// パスワードの照合結果
        /// </summary>
        public bool Result { get; private set; }

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        [ObservableProperty]
        private string errorMessage = string.Empty;

        public PasswordDialogViewModel(IAdminPasswordRepository adminPasswordRepository)
        {
            _passwordService = new PasswordService();
            _adminPasswordRepos = adminPasswordRepository;
        }

        /// <summary>
        /// 入力されたパスワードをpwBoxから受け取り照合する。
        /// </summary>
        /// <param name="pwBox"></param>
        [RelayCommand]
        private async Task Submit(PasswordBox pwBox)
        {
            ErrorMessage = string.Empty;

            if (pwBox == null) return;

            var inputPass = pwBox.Password;

            if (string.IsNullOrEmpty(inputPass))
            {
                // エラーメッセージを表示する
                ErrorMessage = "パスワードを入力してください。";
                return;
            }

            // 照合先のパスワードデータを取得する
            var adminPassword = await _adminPasswordRepos.GetAsync();

            if (adminPassword == null)
            {
                // エラーメッセージを表示する
                ErrorMessage = "パスワードを照合できません。";
                Debug.WriteLine($"[ERROR] 照合先のパスワードデータを取得できません");
                return;
            }

            // パスワードが正しい場合
            if (_passwordService.Verify(inputPass, adminPassword))
            {
                // tureを返して画面を閉じる
                Result = true;
                RequestClose?.Invoke(this, EventArgs.Empty);
                ErrorMessage = string.Empty;
            }
            // パスワードが間違っている場合
            else
            {
                // エラーメッセージを表示する
                ErrorMessage = "パスワードが違います。";
            }
            
        }
    }
}
