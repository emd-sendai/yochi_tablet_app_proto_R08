using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfApp.Common.ViewModels;
using WpfApp.Common.Views;
using WpfApp.Interfaces;
using WpfApp.Screens.Aramashi.ViewModels;
using WpfApp.Screens.Documents.ViewModels;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using WpfApp.Shell.Models;


namespace WpfApp.Shell.ViewModels
{
    public partial class ShellViewModel : ObservableObject
    {
        [ObservableProperty]
        private UserControl headerView = null!;

        [ObservableProperty]
        private UserControl sidebarView = null!;

        [ObservableProperty]
        private bool isSidebarOpen = false;

        public ShellViewModel()
        {
        }

        public void UpdateLayout(IShellLayout layout, object viewModel)
        {
            // ヘッダー
            HeaderView = CreateView(layout.HeaderType);
            // サイドバー
            SidebarView = CreateView(layout.SidebarType);

            HeaderView.DataContext = viewModel;
            SidebarView.DataContext = viewModel;
        }

        private UserControl CreateView(Type viewType)
        {
            if (!typeof(UserControl).IsAssignableFrom(viewType))
            {
                throw new ArgumentException("指定された型がUserControlではありません。");
            }

            if (Activator.CreateInstance(viewType) is not UserControl view)
            {
                throw new InvalidOperationException("UserControlの作成に失敗しました");
            }

            return view;
        }

        [RelayCommand]
        private void ToggleSidebar()
        {
            IsSidebarOpen = !IsSidebarOpen;
        }

        [RelayCommand]
        private void GoToMenu()
        {
            //ChangeScreen(new MenuViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void GoToAramashiMenu()
        {
            //ChangeScreen(new AramashiMenuViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void GoToDocumentMenu()
        {
            //ChangeScreen(new DocumentMenuViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void GoToAdminMenu()
        {
            // パスワードダイアログを生成
            var pwVm = new PasswordDialogViewModel();
            var pwDialog = new PasswordDialogWindow
            {
                DataContext = pwVm,
                Owner = Application.Current.MainWindow
            };

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
                        // 管理メニューに遷移
                    }
                }
            }
        }
    }
}
