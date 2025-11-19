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

        [ObservableProperty]
        private bool isLeftDrawerOpen;

        public MenuViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
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
                        var vm = new MessageDialogViewModel
                        {
                            Message = "パスワードが違います"
                        };

                        var dialog = new MessageDialogWindow
                        {
                            DataContext = vm,
                            Owner = Application.Current.MainWindow
                        };

                        bool? result = dialog.ShowDialog();
                        if (result == true)
                        {
                            // OK押下後の処理
                        }
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
        private void GoToAramashiMenuTest()
        {
            _nav.Navigate(new AramashiMenuTestViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void Exit()
        {
            ShutdownService.Cleanup();

            Application.Current.Shutdown();
        }

    }
}
