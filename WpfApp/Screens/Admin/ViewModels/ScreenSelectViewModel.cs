using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Windows;
using WpfApp.Common.ViewModels;
using WpfApp.Common.Views;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using static WpfApp.Screens.Admin.ViewModels.DisplaySettingsViewModel;

namespace WpfApp.Screens.Admin.ViewModels
{
    /// <summary>
    /// ScreenSelectView.xamlに対応するViewModel。
    /// 画面選択画面での処理を行う。
    /// </summary>
    public partial class ScreenSelectViewModel : ObservableObject , INavigationTarget
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;

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

            _nextScreen = nextScreen;
            Title = GetTitle(_nextScreen);
        }

        private string? GetNextScreenTitle(DisplaySettingsCategory nextScreen)
        {
            Screens.TryGetValue(nextScreen ,out var nextScreenTitle);

            return nextScreenTitle;
        }

        private string GetTitle(DisplaySettingsCategory nextScreen)
        {
            // タイトルに初期値を設定
            var title = _titleItem;

            var nextScreentitle = GetNextScreenTitle(nextScreen);

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

        [RelayCommand]
        private void GoToNextScreenByAramashi()
        {
            if(_nextScreen == DisplaySettingsCategory.PdfDocumentSettings)
            {
                _nav.Navigate(new PdfDocumentSettingsViewModel(_nav, _dia, ScreenCategory.Aramashi));
            }
            else
            {
                Debug.WriteLine($"[ERROR] _nextScreenが存在しません");
                ShowMessageWindow("次の画面を表示できません");
            }
        }

        [RelayCommand]
        private void GoToNextScreenByDocument()
        {
            if (_nextScreen == DisplaySettingsCategory.PdfDocumentSettings)
            {
                _nav.Navigate(new PdfDocumentSettingsViewModel(_nav, _dia, ScreenCategory.Document));
            }
            else
            {
                Debug.WriteLine($"[ERROR] _nextScreenが存在しません");
                ShowMessageWindow("次の画面を表示できません");
            }
        }

        private void ShowMessageWindow(string message)
        {
            var vm = new MessageDialogViewModel
            {
                Message = message
            };

            var dialog = new MessageDialogWindow
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            dialog.ShowDialog();
        }
    }
}
