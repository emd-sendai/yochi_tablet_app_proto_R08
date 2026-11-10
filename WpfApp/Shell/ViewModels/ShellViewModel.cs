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
            // 表示切替前のViewModelのヘッダーと表示切替先のViewModelのヘッダーのタイプが異なる場合
            if (HeaderView == null || HeaderView.GetType() != layout.HeaderType)
            {
                HeaderView = CreateView(layout.HeaderType);
            }

            // サイドバー
            // 表示切替前のViewModelのサイドバーと表示切替先のViewModelのサイドバーのタイプが異なる場合
            if (SidebarView == null || SidebarView.GetType() != layout.SidebarType)
            {
                SidebarView = CreateView(layout.SidebarType);
            }

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
    }
}
