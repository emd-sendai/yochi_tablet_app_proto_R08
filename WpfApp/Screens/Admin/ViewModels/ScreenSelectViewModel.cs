using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Windows;
using WpfApp.Common.ViewModels;
using WpfApp.Common.Views;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;

namespace WpfApp.Screens.Admin.ViewModels
{
    public partial class ScreenSelectViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;

        [ObservableProperty]
        private bool isLeftDrawerOpen;

        private readonly string _nextScreen;

        [ObservableProperty]
        private string title;

        private static readonly string _titleItem = "画面選択";

        public Dictionary<string, string> Screens { get; } = new Dictionary<string, string>
        {
            { "ButtonSettings", "ボタンの名称設定" },
            { "PdfDocumentSettings", "PDF資料設定" }
        };

        public ScreenSelectViewModel(NavigationService navigationService, IDialogService dialogService, string nextScreen)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            _nextScreen = nextScreen;
            Title = GetTitle(_nextScreen);
        }

        private string? GetNextScreenTitle(string nextScreen)
        {
            Screens.TryGetValue(nextScreen ,out var nextScreenTitle);

            return nextScreenTitle;
        }

        private string GetTitle(string nextScreen)
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
        private void GoToNextScreen(string selectedScreen)
        {
            if(_nextScreen == "PdfDocumentSettings")
            {
                _nav.Navigate(new PdfDocumentSettingsViewModel(_nav, _dia, selectedScreen));
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
