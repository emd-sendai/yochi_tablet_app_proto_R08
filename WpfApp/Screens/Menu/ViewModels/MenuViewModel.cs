using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    public partial class MenuViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly IMessageDialogService _message;
        private readonly ShutdownService _shutdownService;

        [ObservableProperty]
        private bool isLeftDrawerOpen;

        public MenuViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _message = new MessageDialogService();
            _shutdownService = new ShutdownService();
        }

        [RelayCommand]
        private void ToggleDrawer()
        {
            // サイドバーを開く場合
            if (!IsLeftDrawerOpen)
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
                    if (pwVm.Result == "0000")
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
                        }
                    }
                    // パスワードが不正解の場合
                    else
                    {
                        //メッセージウィンドウを表示
                        _message.ShowMessage("パスワードが違います");
                    }
                }
            }
            // サイドバーを閉じる場合
            else
            {
                IsLeftDrawerOpen = false;
            }
        }

        [RelayCommand]
        private void GoToDisplaySetttings()
        {
            _nav.Navigate(new DisplaySettingsViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void GoToAdminDocument()
        {
            _nav.Navigate(new AdminDocumentViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void GoToAramashiMenu()
        {
            _nav.Navigate(new AramashiMenuViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void GoToDocumentMenu()
        {
            _nav.Navigate(new DocumentMenuViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void Exit()
        {
            _shutdownService.Cleanup();

            Application.Current.Shutdown();
        }

    }
}
