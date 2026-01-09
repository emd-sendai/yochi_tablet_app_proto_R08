using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using WpfApp.Common.Models;
using WpfApp.Common.ViewModels;
using WpfApp.Common.Views;
using WpfApp.Interfaces;
using WpfApp.Screens.Admin.Models;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using static WpfApp.Screens.Admin.Models.TabItemData;

namespace WpfApp.Screens.Admin.ViewModels
{
    public partial class PdfDocumentSettingsViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;

        private static readonly string BaseDire = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string AramashiDataFolder = Path.Combine(BaseDire, "DocumentResources", "Aramashi");
        private static readonly string DocumentDataFolder = Path.Combine(BaseDire, "DocumentResources", "Documents");

        [ObservableProperty]
        private bool isLeftDrawerOpen;

        private readonly string _selectedScreen;

        [ObservableProperty]
        private bool isAramashi;

        [ObservableProperty]
        private bool isDocument;

        [ObservableProperty]
        private string title;

        private static readonly string _titleItem = "PDF資料設定";

        public Dictionary<string, string> Screens { get; } = new Dictionary<string, string>
        {
            { "Aramashi", "補償のあらまし" },
            { "Document", "各種資料" }
        };

        [ObservableProperty]
        private ObservableCollection<TabItemData>? tabItems;

        [ObservableProperty]
        private int selectedTabIndex;

        [ObservableProperty]
        private LandownerData? selectedLandowner;

        [ObservableProperty]
        private PdfData? selectedPdfDocument;

        public PdfDocumentSettingsViewModel(NavigationService navigationService, IDialogService dialogService, string selectedScreen)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _selectedScreen = selectedScreen;
            Title = GetTitle();
            InitializeViewState();
        }

        private string? GetSelectedScreenTitle()
        {
            Screens.TryGetValue(_selectedScreen, out var selectedScreentitle);

            return selectedScreentitle;
        }

        private string GetTitle()
        {
            // タイトルに初期値を設定
            var title = _titleItem;

            var selectedScreentitle = GetSelectedScreenTitle();

            if (!string.IsNullOrWhiteSpace(selectedScreentitle))
            {
                title = _titleItem + " - " + selectedScreentitle;
            }
            return title;
        }

        private void InitializeViewState()
        {
            if(_selectedScreen == "Aramashi")
            {
                IsAramashi = true;
                IsDocument = false;
                SelectedTabIndex = 0;
                Debug.WriteLine($"[DEBUG] {_selectedScreen}のPDF資料選択項目を表示します");
            }
            else if(_selectedScreen == "Document")
            {
                IsAramashi = false;
                IsDocument = true;
                SelectedTabIndex = 2;
                Debug.WriteLine($"[DEBUG] {_selectedScreen}のPDF資料選択項目を表示します");
            }
            else
            {
                Debug.WriteLine($"[ERROR] _selectedScreenがNULLもしくは不正な値です");
                ShowMessageWindow("選択項目を表示できません");
            }
        }

        private void SetTabItems()
        {
            var items = new ObservableCollection<TabItemData>();

            var tab = new TabItemData();
            var landowners = new ObservableCollection<LandownerData>();
            var documents = new ObservableCollection<DocumentData>();

            // 用地補償の手順
            var doc = new DocumentData();
            doc.DisplayName = "用地補償の手順";
            doc.FolderPath = Path.Combine(AramashiDataFolder, "Aramashi");
            documents.Add(doc);
            tab.Documents = documents;
            items.Add(tab);

            // その他
            documents = new ObservableCollection<DocumentData>();
            doc = new DocumentData();
            doc.DisplayName = "税金等";
            doc.FolderPath = Path.Combine(AramashiDataFolder, "Others", "Zeikin");
            documents.Add(doc);
            doc = new DocumentData();
            doc.DisplayName = "損失補償基準";
            doc.FolderPath = Path.Combine(AramashiDataFolder, "Others", "Sonshitsu");
            documents.Add(doc);
            tab.Documents = documents;
            items.Add(tab);

            // 事業全般資料
            // ボタンをJSONからよみこむようにしてから

            // 地権者個別資料
            // ボタンをJSONからよみこむようにしてから

        }

        partial void OnSelectedTabIndexChanged(int oldValue, int newValue)
        {
            // タブが切り替えられたとき選択状態を初期化する
            SelectedLandowner = null;
            SelectedPdfDocument = null;
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
