using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Windows;
using WpfApp.Common.ViewModels;
using WpfApp.Common.Views;
using WpfApp.Interfaces;
using WpfApp.Screens.Admin.ViewModels;
using WpfApp.Screens.Aramashi.ViewModels;
using WpfApp.Screens.Documents.ViewModels;
using WpfApp.Services;

namespace WpfApp.Screens.Menu.ViewModels
{
    /// <summary>
    /// MenuView.xamlに対応するViewModel。
    /// メインメニュー画面での処理を行う。
    /// </summary>
    public partial class MenuViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly IMessageDialogService _message;
        private readonly ShutdownService _shutdownService;
        private readonly ResourceInitializeService _initializer;

        [ObservableProperty]
        private bool isLeftDrawerOpen;

        private bool isPasswordCorrect;

        public MenuViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _message = new MessageDialogService();
            _shutdownService = new ShutdownService();
            _initializer = new ResourceInitializeService();
        }

        /// <summary>
        /// パスワードを照合し、管理メニューバーを表示する。
        /// </summary>
        [RelayCommand]
        private void ToggleDrawer()
        {
            // パスワードが未照合の状態でサイドバーを開く場合
            if (!IsLeftDrawerOpen && !isPasswordCorrect)
            {
                // パスワードダイアログを生成
                var pwVm = new PasswordDialogViewModel();
                var pwDialog = new PasswordDialogWindow
                {
                    DataContext = pwVm,
                    Owner = Application.Current.MainWindow
                };

                // ShowDialog() は true/false/null を返す
                if (pwDialog.ShowDialog() == true)
                {
                    // パスワードが正解の場合
                    if (pwVm.Result)
                    {
                        //メッセージウィンドウを表示
                        var vm = new MessageDialogViewModel
                        {
                            Message = "管理メニューを表示します"
                        };

                        var dialog = new MessageDialogWindow
                        {
                            DataContext = vm,
                            Owner = Application.Current.MainWindow
                        };

                        bool? result = dialog.ShowDialog();
                        if (result == true)
                        {
                            IsLeftDrawerOpen = true;
                            isPasswordCorrect = true;
                        }
                    }
                }
            }
            // パスワードが照合済の状態でサイドバーを開く場合
            else if (!IsLeftDrawerOpen && isPasswordCorrect)
            {
                IsLeftDrawerOpen = true;
            }
            // サイドバーを閉じる場合
            else
            {
                IsLeftDrawerOpen = false;
            }
        }

        /// <summary>
        /// 表示設定画面に遷移する。（管理メニューバー）
        /// </summary>
        [RelayCommand]
        private void GoToDisplaySetttings()
        {
            _nav.Navigate(new DisplaySettingsViewModel(_nav, _dia));
        }

        /// <summary>
        /// 管理資料画面に遷移する。（管理メニューバー）
        /// </summary>
        [RelayCommand]
        private void GoToAdminDocument()
        {
            _nav.Navigate(new AdminDocumentViewModel(_nav, _dia));
        }

        /// <summary>
        /// アプリ全体のデータを初期化してアプリを終了する。（管理メニューバー）
        /// </summary>
        [RelayCommand]
        private void CleanupAppData()
        {
            // 確認ダイアログを表示
            if (!_message.ShowConfirm("データを初期化します。よろしいですか？\n※初期化完了後アプリケーションは自動で終了します。"))
            {
                return;
            }

            try
            {
                _initializer.CleanupResources();
            }
            catch (IOException)
            {
                _message.ShowMessage("アプリで使用中のファイルまたは他のアプリで開いているファイルがあります。" +
                    "\nファイルを閉じてから、もう一度お試しください。");
                return;
            }
            catch (UnauthorizedAccessException)
            {
                _message.ShowMessage("アクセス権が不足しているため、ファイルを削除できません。");
                return;
            }
            catch (Exception)
            {
                _message.ShowMessage("ファイルを削除できません。\n管理者へお問い合わせください。");
                return;
            }

            _shutdownService.Cleanup();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// 補償のあらまし画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToAramashiMenu()
        {
            _nav.Navigate(new AramashiMenuViewModel(_nav, _dia));
        }

        /// <summary>
        /// 各種資料画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToDocumentMenu()
        {
            _nav.Navigate(new DocumentMenuViewModel(_nav, _dia));
        }

        /// <summary>
        /// 一時保存データを初期化してアプリを終了する。
        /// </summary>
        [RelayCommand]
        private void Exit()
        {
            _shutdownService.Cleanup();
            Application.Current.Shutdown();
        }

    }
}
