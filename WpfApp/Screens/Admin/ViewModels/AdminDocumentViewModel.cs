using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;

namespace WpfApp.Screens.Admin.ViewModels
{
    /// <summary>
    /// AdminDocumentView.xamlに対応するViewModel。
    /// 管理資料画面での処理を行う。
    /// </summary>
    public partial class AdminDocumentViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly IMessageDialogService _message;

        [ObservableProperty]
        private bool isLeftDrawerOpen;

        public AdminDocumentViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _message = new MessageDialogService();
        }

        [RelayCommand]
        private void ToggleDrawer()
        {
            IsLeftDrawerOpen = !IsLeftDrawerOpen;
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
        private void GoToMenu()
        {
            _nav.Navigate(new MenuViewModel(_nav, _dia));

        }

        /// <summary>
        /// 準備中ダイアログを表示する。（機能作成後削除）
        /// </summary>
        [RelayCommand]
        private void ShowTestMessage()
        {
            // 準備中ダイアログを表示する
            _message.ShowTestMessage();
        }
    }
}
