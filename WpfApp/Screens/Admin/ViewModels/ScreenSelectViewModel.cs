using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using static WpfApp.Screens.Admin.ViewModels.DisplaySettingsViewModel;

namespace WpfApp.Screens.Admin.ViewModels
{
    /// <summary>
    /// ScreenSelectView.xamlに対応するViewModel。
    /// 画面選択画面での処理（項目を編集する画面の選択）を行う。
    /// </summary>
    public partial class ScreenSelectViewModel : ObservableObject , INavigationTarget
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly IMessageDialogService _message;

        public Func<object>? CreateBackViewModel => () => new DisplaySettingsViewModel(_nav, _dia);

        [ObservableProperty]
        private bool isLeftDrawerOpen;

        private readonly DisplaySettingsCategory _nextScreen;

        [ObservableProperty]
        private string title;

        private static readonly string _titleItem = "画面選択";

        public Dictionary<DisplaySettingsCategory, string> Screens { get; } = new Dictionary<DisplaySettingsCategory, string>
        {
            { DisplaySettingsCategory.ButtonSettings, "ボタンの名称設定" },
            { DisplaySettingsCategory.PdfDocumentSettings, "PDF資料設定" }
        };

        public enum ScreenCategory
        {
            Aramashi,   // 補償のあらまし
            Document    // 各種資料
        }

        public ScreenSelectViewModel(NavigationService navigationService, IDialogService dialogService, DisplaySettingsCategory nextScreen)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _message = new MessageDialogService();

            _nextScreen = nextScreen;
            Title = GetTitle();
        }

        /// <summary>
        /// 次に遷移する画面のタイトルを取得する。
        /// </summary>
        /// <returns></returns>
        private string? GetNextScreenTitle()
        {
            Screens.TryGetValue(_nextScreen, out var nextScreenTitle);

            return nextScreenTitle;
        }

        /// <summary>
        /// 画面のタイトルを取得する。
        /// </summary>
        /// <returns></returns>
        private string GetTitle()
        {
            // タイトルに初期値を設定
            var title = _titleItem;

            var nextScreentitle = GetNextScreenTitle();

            if (!string.IsNullOrWhiteSpace(nextScreentitle))
            {
                title = nextScreentitle + " - " + _titleItem;
            }

            return title;
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
        private void GoBack()
        {
            _nav.GoBack();
        }

        [RelayCommand]
        private void GoToMenu()
        {
            _nav.Navigate(new MenuViewModel(_nav, _dia));

        }

        /// <summary>
        /// 補償のあらまし画面を選択し、PDF資料設定画面へ遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToNextScreenByAramashi()
        {
            if(_nextScreen == DisplaySettingsCategory.PdfDocumentSettings)
            {
                _nav.Navigate(new PdfDocumentSettingsViewModel(_nav, _dia, ScreenCategory.Aramashi));
            }
            else
            {
                Debug.WriteLine($"[ERROR] _nextScreenの値が不正です: ScreenCategory.{_nextScreen}");
                _message.ShowMessage("次の画面を表示できません。\n前の画面に戻って、もう一度お試しください。");
            }
        }

        /// <summary>
        /// 各種資料画面を選択し、PDF資料設定画面へ遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToNextScreenByDocument()
        {
            if (_nextScreen == DisplaySettingsCategory.PdfDocumentSettings)
            {
                _nav.Navigate(new PdfDocumentSettingsViewModel(_nav, _dia, ScreenCategory.Document));
            }
            else
            {
                Debug.WriteLine($"[ERROR] _nextScreenの値が不正です: ScreenCategory.{_nextScreen}");
                _message.ShowMessage("次の画面を表示できません。\n前の画面に戻って、もう一度お試しください。");
            }
        }
    }
}
