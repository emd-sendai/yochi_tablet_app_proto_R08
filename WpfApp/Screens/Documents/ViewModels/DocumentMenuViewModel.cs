using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using WpfApp.Common.Models;
using WpfApp.Common.ViewModels;
using WpfApp.Common.Views;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;

namespace WpfApp.Screens.Documents.ViewModels
{
    public partial class DocumentMenuViewModel : ObservableObject
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


        public DocumentMenuViewModel(NavigationService navigationService, IDialogService dialogService)
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
        private void GoToDocumentMenu2()
        {
            _nav.Navigate(new DocumentMenu2ViewModel(_nav, _dia));
        }

        private void LoadWorksFromJson()
        {
            string path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data/works.json");

            if (!File.Exists(path))
            {
                MessageBox.Show("works.json が見つかりません");
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
            string path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data/landowners.json");

            if (!File.Exists(path))
            {
                MessageBox.Show("landowners.json が見つかりません");
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
            var pwWindow = new Common.Views.PasswordDialogWindow();
            var pwVM = new Common.ViewModels.PasswordDialogViewModel();
            pwWindow.DataContext = pwVM;

            bool? result = pwWindow.ShowDialog();
            if (result == true)
            {
                if (pwVM.Result == SelectedLandowner.Password)
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
