using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;

namespace WpfApp.Screens.Admin.ViewModels
{
    /// <summary>
    /// DisplaySettingsView.xamlに対応するViewModel。
    /// 表示設定画面での処理を行う。
    /// </summary>
    public partial class DisplaySettingsViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly IMessageDialogService _message;

        [ObservableProperty]
        private bool isLeftDrawerOpen;

        public enum DisplaySettingsCategory
        {
            ButtonSettings, // ボタンの名称設定
            PdfDocumentSettings, // PDF資料設定
            VideoSettings // 動画設定
        }

        public DisplaySettingsViewModel(NavigationService navigationService, IDialogService dialogService)
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
        /// ボタンの名称設定画面へ遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToButtonSettings()
        {
            _nav.Navigate(new ButtonSettingsViewModel(_nav, _dia));
        }

        /// <summary>
        /// 画面選択画面（PDF資料設定）へ遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToPdfDocumentSettings()
        {
            _nav.Navigate(new ScreenSelectViewModel(_nav, _dia, DisplaySettingsCategory.PdfDocumentSettings));
        }

        /// <summary>
        /// 詳細項目選択画面（動画設定）へ遷移する。（作成中のためコメントアウト中）
        /// </summary>
        [RelayCommand]
        private void GoToVideoSettings()
        {
            //_nav.Navigate(new SubCategorySelectViewModel(_nav, _dia, DisplaySettingsCategory.VideoSettings));

            // 準備中ダイアログを表示する（作成後削除）
            _message.ShowTestMessage();
        }
    }
}
