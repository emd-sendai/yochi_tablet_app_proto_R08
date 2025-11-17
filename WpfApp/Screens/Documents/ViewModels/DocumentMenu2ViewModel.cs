using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using WpfApp.Common.Models;
using WpfApp.Common.ViewModels;
using WpfApp.Common.Views;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;

namespace WpfApp.Screens.Documents.ViewModels
{
    public partial class DocumentMenu2ViewModel : ObservableObject
    {

        private readonly NavigationService _nav;
        private readonly IDialogService _dia;

        [ObservableProperty]
        private ObservableCollection<WorkData>? works;

        [ObservableProperty]
        private WorkData? selectedWork;

        [ObservableProperty]
        private ObservableCollection<LandownerData>? landowners;

        [ObservableProperty]
        private LandownerData? selectedLandowner;

        public ISnackbarMessageQueue SnackbarMessageQueue { get; }

        public DocumentMenu2ViewModel(NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            SnackbarMessageQueue = new SnackbarMessageQueue();
            LoadLandownersFromJson();
            LoadWorksFromJson();
        }



        [RelayCommand]
        private void GoToMenu()
        {
            _nav.Navigate(new MenuViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void GoToDocumentMenu()
        {
            _nav.Navigate(new DocumentMenuViewModel(_nav, _dia));
        }


        private void LoadWorksFromJson()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/works.json");

            if (!File.Exists(path))
            {
                MessageBox.Show("事業データが見つかりません");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] slides.json not found: {path}");
                return;
            }

            string json = File.ReadAllText(path);
            var array = JsonSerializer.Deserialize<WorkData[]>(json);
            Works = array != null
                ? new ObservableCollection<WorkData>(array)
                : new ObservableCollection<WorkData>();
        }

        private void LoadLandownersFromJson()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/landowners.json");

            if (!File.Exists(path))
            {
                MessageBox.Show("地権者データが見つかりません");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] slides.json not found: {path}");
                return;
            }

            string json = File.ReadAllText(path);
            var array = JsonSerializer.Deserialize<LandownerData[]>(json);
            Landowners = array != null
                ? new ObservableCollection<LandownerData>(array)
                : new ObservableCollection<LandownerData>();
        }

        [RelayCommand]
        private async Task SelectLandownerAsync()
        {
            if (SelectedLandowner == null)
            {
                SnackbarMessageQueue.Enqueue("地権者ユーザーを選択してください。");
                return;
            }

            // パスワードダイアログ呼び出し
            var pwWindow = new PasswordDialogWindow();
            var pwVM = new PasswordDialogViewModel();
            pwWindow.DataContext = pwVM;

            bool? result = pwWindow.ShowDialog();
            if (result == true)
            {
                if (pwVM.Result == "0000")
                {
                    SnackbarMessageQueue.Enqueue($"{SelectedLandowner.LandownerName} 様の資料を表示します。");
                }
                else
                {
                    SnackbarMessageQueue.Enqueue("パスワードが間違っています。");
                    SelectedLandowner = null;
                }
            }
        }
    }
}
