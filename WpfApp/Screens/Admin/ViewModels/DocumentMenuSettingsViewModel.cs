using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using WpfApp.Shell.Models;
using WpfApp.Shell.Views.Header;
using WpfApp.Shell.Views.Sidebar;

namespace WpfApp.Screens.Admin.ViewModels
{
    public partial class DocumentMenuSettingsViewModel : ObservableObject, IShellLayout
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly IMessageDialogService _message;

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
        private int selectedMode = 2;

        public DocumentMenuSettingsViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _message = new MessageDialogService();

            Title = "メイン機能表示設定 - 各種資料";
        }

        [RelayCommand]
        private void GoToButtonSettings()
        {

        }

        [RelayCommand]
        private void GoToDocumentManagement()
        {
            var vm = new DocumentManagementViewModel(_nav, _dia, App.WorkRepository);
            _nav.Navigate(vm);
            vm.IsExpander1Open = IsExpander1Open;
            vm.IsExpander2Open = IsExpander2Open;
            vm.IsExpander3Open = IsExpander3Open;
        }

        /// <summary>
        /// 管理メニュー画面に留まる。（バインドエラー防止用）
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
        /// 管理資料設定画面に留まる。（バインドエラー防止用）
        /// </summary>
        [RelayCommand]
        private void GoToDocumentMenuSettings()
        {
        }

        /// <summary>
        /// 事業情報管理画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToWorkManagement()
        {
            var vm = new WorkManagementViewModel(_nav, _dia, App.WorkRepository);
            _nav.Navigate(vm);
            vm.IsExpander1Open = IsExpander1Open;
            vm.IsExpander2Open = IsExpander2Open;
            vm.IsExpander3Open = IsExpander3Open;
        }

        /// <summary>
        /// 地権者情報管理画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToLandownerManagement()
        {
            var vm = new LandownerManagementViewModel(_nav, _dia, App.LandownerRepository, App.WorkLandownerRepository);
            _nav.Navigate(vm);
            vm.IsExpander1Open = IsExpander1Open;
            vm.IsExpander2Open = IsExpander2Open;
            vm.IsExpander3Open = IsExpander3Open;
        }

        /// <summary>
        /// 事業実績管理画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToWorkLandownerManagement()
        {
            var vm = new WorkLandownerManagementViewModel(_nav, _dia, App.WorkRepository, App.WorkLandownerRepository);
            _nav.Navigate(vm);
            vm.IsExpander1Open = IsExpander1Open;
            vm.IsExpander2Open = IsExpander2Open;
            vm.IsExpander3Open = IsExpander3Open;
        }

        /// <summary>
        /// 管理者パスワード設定画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToAdminPasswordSettings()
        {
            var vm = new AdminPasswordSettingsViewModel(_nav, _dia, App.AdminPasswordRepository);
            _nav.Navigate(vm);
            vm.IsExpander1Open = IsExpander1Open;
            vm.IsExpander2Open = IsExpander2Open;
            vm.IsExpander3Open = IsExpander3Open;
        }
    }
}
