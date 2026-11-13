using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WpfApp.Common.Models;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using WpfApp.Shell.Models;
using WpfApp.Shell.Views.Header;
using WpfApp.Shell.Views.Sidebar;

namespace WpfApp.Screens.Admin.ViewModels
{
    public partial class AdminPasswordSettingsViewModel : ObservableObject, IShellLayout
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly PdfDocumentService _pdf;
        private readonly IMessageDialogService _message;
        private readonly IAppPathService _path;
        private readonly PasswordService _password;

        public Type HeaderType => typeof(HeaderType001View);
        public Type SidebarType => typeof(SidebarType003View);

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private bool isExpander1Open = true;

        [ObservableProperty]
        private bool isExpander2Open = true;

        [ObservableProperty]
        private bool isExpander3Open = true;

        [ObservableProperty]
        private int selectedMode = 6;

        public event Action? FocusConfirmPassRequested;

        public event Action? FocusNewPassRequested;

        public event Action? ClearCurrentPassRequested;

        public event Action? ClearAllPassRequested;

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isPassError = false;

        [ObservableProperty]
        private bool isNewPassError = false;

        [ObservableProperty]
        private bool isConfirmPassError = false;

        public AdminPasswordSettingsViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _pdf = new PdfDocumentService();
            _message = new MessageDialogService();
            _path = new AppPathService();
            _password = new PasswordService();

            Title = "管理者パスワード";
        }

        public void ChangePassword(string currentPass, string newPass, string confirmPass)
        {
            // エラーメッセージの初期化
            ClearErrorMessage();

            // いずれかのパスワードが未入力の場合
            if (CheckNullOrEmpty(currentPass, newPass, confirmPass))
            {
                ErrorMessage = "未入力の項目があります。";
                _message.ShowMessage("未入力の項目があります。");
                return;
            }

            // パスワードの文字数が4文字未満の場合
            if (CheckMinLength(currentPass, newPass, confirmPass))
            {
                ErrorMessage = "4文字以上のパスワードを入力してください。";
                _message.ShowMessage("4文字以上のパスワードを入力してください。");
                return;
            }

            // 新しいパスワードと確認用パスワードが一致しない場合
            if (!string.Equals(newPass, confirmPass, StringComparison.Ordinal))
            {
                IsNewPassError = true;
                IsConfirmPassError = true;
                FocusConfirmPassRequested?.Invoke();
                ErrorMessage = "新しいパスワードと確認用パスワードが一致しません。";
                _message.ShowMessage("新しいパスワードと確認用パスワードが一致しません。");
                return;
            }

            // 現在のパスワードを取得
            var passData = _password.LoasPasswordFromJson();
            if (passData == null) {
                _message.ShowMessage("現在のパスワードを照合できません。");
                return;
            }

            // 現在のパスワードが間違っている場合
            if (!_password.Verify(currentPass, passData))
            {
                IsPassError = true;
                ClearCurrentPassRequested?.Invoke();
                ErrorMessage = "現在のパスワードが違います。";
                _message.ShowMessage("現在のパスワードが違います。");
                return;
            }

            // 現在のパスワードと新しいパスワードが同一の場合
            if (string.Equals(currentPass, newPass, StringComparison.Ordinal))
            {
                IsNewPassError = true;
                IsConfirmPassError = true;
                FocusNewPassRequested?.Invoke();
                ErrorMessage = "現在のパスワードと新しいパスワードが同一です。\n現在とは異なるパスワードを入力してください。";
                _message.ShowMessage("現在のパスワードと新しいパスワードが同一です。\n現在とは異なるパスワードを入力してください。");
                return;
            }

            // パスワード更新処理
            if (_message.ShowConfirm("管理者パスワードを変更します。\nよろしいですか？"))
            {
                if (SavePasswordToJson(newPass))
                {
                    _message.ShowMessage("管理者パスワードを変更しました。");
                    ClearAllPassRequested?.Invoke();
                }
                else
                {
                    _message.ShowMessage("管理者パスワードの変更に失敗しました。");
                }
            }            
        }

        private void ClearErrorMessage()
        {
            ErrorMessage = string.Empty;
            IsPassError = false;
            IsNewPassError = false;
            IsConfirmPassError = false;
        }

        private bool CheckNullOrEmpty(string currentPass, string newPass, string confirmPass)
        {
            bool ret = false;

            // 現在のパスワード
            if (string.IsNullOrEmpty(currentPass))
            {
                IsPassError = true;
                ret = true;
            }

            // 新しいパスワード
            if (string.IsNullOrEmpty(newPass))
            {
                IsNewPassError = true;
                ret = true;
            }

            // 新しいパスワード（確認用）
            if (string.IsNullOrEmpty(confirmPass))
            {
                IsConfirmPassError = true;
                ret = true;
            }

            return ret;
        }

        private bool CheckMinLength(string currentPass, string newPass, string confirmPass)
        {
            bool ret = false;

            // 現在のパスワード
            if (currentPass.Length < 4)
            {
                IsPassError = true;
                ret = true;
            }

            // 新しいパスワード
            if (newPass.Length < 4)
            {
                IsNewPassError = true;
                ret = true;
            }

            // 新しいパスワード（確認用）
            if (confirmPass.Length < 4)
            {
                IsConfirmPassError = true;
                ret = true;
            }

            return ret;
        }

        private bool SavePasswordToJson(string newPass)
        {
            bool ret = false;

            string path = _path.AdminSettings;

            try
            {
                var newPassData = _password.HashPassword(newPass);
                if (newPassData == null)
                {
                    Debug.WriteLine($"[ERROR] パスワード文字列をハッシュ化できません: {newPass}");
                    return ret;
                }

                if (!File.Exists(path))
                {
                    Debug.WriteLine($"[ERROR] パスワードデータが見つかりません: {path}");
                    return ret;
                }

                var json = JsonSerializer.Serialize(newPassData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
                Debug.WriteLine($"[DEBUG] パスワードデータを更新しました: {path}");
                ret = true;

                return ret;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] パスワードデータを更新できません: {ex}");
                return ret;
            }
        }

        /// <summary>
        /// 管理メニュー画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToAdminMenu()
        {
            var vm = new AdminMenuViewModel(_nav, _dia);
            _nav.Navigate(vm);
            vm.IsExpander1Open = IsExpander1Open;
            vm.IsExpander2Open = IsExpander2Open;
            vm.IsExpander3Open = IsExpander3Open;
        }

        /// <summary>
        /// メインメニュー画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToMenu()
        {
            if (_message.ShowConfirm("メインメニューに戻ります。よろしいですか？"))
            {
                _nav.Navigate(new MenuViewModel(_nav, _dia));
            }
        }

        /// <summary>
        /// 管理資料画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToAdminDocument()
        {
            var vm = new AdminDocumentViewModel(_nav, _dia);
            _nav.Navigate(vm);
            vm.IsExpander1Open = IsExpander1Open;
            vm.IsExpander2Open = IsExpander2Open;
            vm.IsExpander3Open = IsExpander3Open;
        }

        /// <summary>
        /// 管理者パスワード設定画面に留まる。（バインドエラー防止用）
        /// </summary>
        [RelayCommand]
        private void GoToAdminPasswordSettings()
        {
        }
    }
}
