using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;
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

        public PasswordDialogViewModel()
        {
            _passwordService = new PasswordService();
        }

        /// <summary>
        /// 入力されたパスワードをpwBoxから受け取り照合する。
        /// </summary>
        /// <param name="pwBox"></param>
        [RelayCommand]
        private void Submit(PasswordBox pwBox)
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

            var passData = _passwordService.LoasPasswordFromJson();
            // 照合先のパスワードデータが取得できなかった場合
            if (passData == null)
            {
                // エラーメッセージを表示する
                ErrorMessage = "パスワードを照合できません。";
                return;
            }

            // パスワードが正解の場合
            if (_passwordService.Verify(inputPass, passData))
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
