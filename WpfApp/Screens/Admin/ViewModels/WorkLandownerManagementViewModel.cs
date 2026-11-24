using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfApp.Common.Repositories.Interfaces;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using WpfApp.Shell.Models;
using WpfApp.Shell.Views.Header;
using WpfApp.Shell.Views.Sidebar;

namespace WpfApp.Screens.Admin.ViewModels
{
    public partial class WorkLandownerManagementViewModel : ObservableObject, IShellLayout
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly IMessageDialogService _message;
        private readonly IWorkLandownerRepository _worklandownerRepos;

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
        private int selectedMode = 5;

        public WorkLandownerManagementViewModel(NavigationService navigationService, IDialogService dialogService,
            IWorkLandownerRepository workLandownerRepository)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _worklandownerRepos = workLandownerRepository ?? throw new ArgumentNullException(nameof(workLandownerRepository));
            _message = new MessageDialogService();

            Title = "事業実績管理";
        }

        /// <summary>
        /// 管理メニュー画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToAdminMenu()
        {
            var vm = new AdminDocumentViewModel(_nav, _dia);
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
            var vm = new LandownerManagementViewModel(_nav, _dia, App.LandownerRepository);
            _nav.Navigate(vm);
            vm.IsExpander1Open = IsExpander1Open;
            vm.IsExpander2Open = IsExpander2Open;
            vm.IsExpander3Open = IsExpander3Open;
        }

        /// <summary>
        /// 事業実績管理画面に留まる。（バインドエラー防止用）
        /// </summary>
        [RelayCommand]
        private void GoToWorkLandownerManagement()
        {
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
