using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
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

        private static readonly string ButtonDataFolder = Path.Combine(BaseDire, "Data", "ButtonSettings");
        private static readonly string WorksButtonPath = Path.Combine(ButtonDataFolder, "documentWorksButtons.json");
        private static readonly string LandownerButtonPath = Path.Combine(ButtonDataFolder, "documentLandownerButtons.json");

        private static readonly string LandownersDataPath = Path.Combine(BaseDire, "Data", "landowners.json");

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
        private Dictionary<string, ObservableCollection<PdfDocumentData>>? landownerDocuments;

        [ObservableProperty]
        private int selectedTabIndex;

        [ObservableProperty]
        private LandownerData? selectedLandowner;

        [ObservableProperty]
        private PdfDocumentData? selectedPdfDocument;

        public PdfDocumentSettingsViewModel(NavigationService navigationService, IDialogService dialogService, string selectedScreen)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _selectedScreen = selectedScreen;
            Title = GetTitle();
            SetTabItems();
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
            var documents = new ObservableCollection<PdfDocumentData>();
            var doc = new PdfDocumentData();


            /* 用地補償の手順 */
            documents.Add(GetDocumentData("用地補償の手順", Path.Combine(AramashiDataFolder, "Aramashi"), "Aramashi"));
            tab.Documents = documents;
            // TabItemリストに格納
            items.Add(tab);


            /* その他 */
            tab = new TabItemData();
            documents = new ObservableCollection<PdfDocumentData>();

            documents.Add(GetDocumentData("税金等", Path.Combine(AramashiDataFolder, "Others", "Zeikin"), "Zeikin"));
            documents.Add(GetDocumentData("損失補償基準", Path.Combine(AramashiDataFolder, "Others", "Sonshitsu"), "Sonshitsu"));
            tab.Documents = documents;
            // TabItemリストに格納
            items.Add(tab);


            /* 事業全般資料 Works */
            tab = new TabItemData();
            documents = new ObservableCollection<PdfDocumentData>();

            var worksButtons = LoadButtonFromJson(WorksButtonPath);
            foreach (var button in worksButtons)
            {
                documents.Add(GetDocumentData(button.LabelText, Path.Combine(DocumentDataFolder, "Works", button.BtnId), button.BtnId));
            }
            tab.Documents = documents;
            // TabItemリストに格納
            items.Add(tab);


            /* 地権者個別資料 Landowner */
            tab = new TabItemData();

            landowners = new ObservableCollection<LandownerData>();
            // 地権者ユーザリストを取得
            landowners = LoadLandownersFromJson();
            // TabItemDataに格納
            tab.Landowners = landowners;

            LandownerDocuments = new();
            // 地権者個別資料のボタンの名称、Id（フォルダ名）を取得
            var landownerButtons = LoadButtonFromJson(LandownerButtonPath);

            foreach (var owner in landowners)
            {
                // 初期化
                documents = new ObservableCollection<PdfDocumentData>();

                for (int i = 0; i < landownerButtons.Count; i++)
                {
                    var button = landownerButtons[i];
                    // PDFデータフォルダのパスを取得して格納
                    documents.Add(GetDocumentData(button.LabelText, Path.Combine(DocumentDataFolder, "Landowners", owner.LandownerId, button.BtnId), button.BtnId));
                }
                // 地権者ごとのPDFフォルダ情報をDictonaryに格納
                LandownerDocuments.Add(owner.LandownerId, documents);
                Debug.WriteLine($"[DEBUG] 地権者個別資料データを格納します :{owner.LandownerId}");
            }
            // 最後のPDFフォルダ情報を格納（表示用）
            tab.Documents = documents;
            // TabItemリストに格納
            items.Add(tab);

            // 画面表示用リストに格納
            TabItems = items;
        }

        private PdfDocumentData GetDocumentData(string displayName, string folderPath, string id)
        {
            var doc = new PdfDocumentData();

            doc.DisplayName = displayName;
            doc.FolderPath = folderPath;
            doc.Id = id;

            return doc;
        }

        private List<ButtonConfig> LoadButtonFromJson(string path)
        {
            var buttons = new List<ButtonConfig>();

            if (!File.Exists(path))
            {
                MessageBox.Show("ボタンの名称を表示できません");
                Debug.WriteLine($"[DEBUG] ボタンデータが存在しません: {path}");
            }
            else
            {
                string json = File.ReadAllText(path);
                buttons = JsonSerializer.Deserialize<List<ButtonConfig>>
                    (json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ButtonConfig>();
            }
            return buttons;
        }

        private ObservableCollection<LandownerData> LoadLandownersFromJson()
        {
            var landowners = new ObservableCollection<LandownerData>();

            try
            {
                string path = LandownersDataPath;

                if (!File.Exists(path))
                {
                    MessageBox.Show("地権者データが見つかりません");
                    Debug.WriteLine($"[DEBUG] 地権者データが見つかりません: {path}");
                }

                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<LandownerData>>(json);

                landowners = new ObservableCollection<LandownerData>(list ?? new List<LandownerData>());
            }
            catch (Exception ex)
            {
                MessageBox.Show("地権者データを読み込むことができません");
                Debug.WriteLine($"[DEBUG] 地権者データを読み込むことができません: {ex.Message}");
            }

            return landowners;
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

        [RelayCommand]
        private void Select()
        {
            // 地権者個別資料の場合
            if (SelectedTabIndex == 3)
            {
                if(SelectedLandowner == null)
                {
                    ShowMessageWindow("地権者ユーザを選択してください");
                    Debug.WriteLine($"[ERROR] SelectedLandownerがNULLです");
                    return;
                }

                if (SelectedPdfDocument == null)
                {
                    ShowMessageWindow("項目を選択してください");
                    Debug.WriteLine($"[ERROR] SelectedPdfDocumentがNULLです");
                    return;
                }

                if (LandownerDocuments != null && LandownerDocuments.TryGetValue(SelectedLandowner.LandownerId, out var pdfDocuments))
                {
                    var isUpdated = false;

                    foreach(var pdfDoc in pdfDocuments)
                    {
                        if(pdfDoc.Id == SelectedPdfDocument.Id)
                        {
                            SelectedPdfDocument = pdfDoc;
                            Debug.WriteLine($"[DEBUG] SelectedPdfDocumentを更新しました");
                            isUpdated = true;
                            break;
                        }
                    }

                    if (!isUpdated)
                    {
                        ShowMessageWindow("地権者個別資料の項目を選択できません");
                        Debug.WriteLine($"[ERROR] LandownerDocuments[{SelectedLandowner.LandownerId}]に「{SelectedPdfDocument.Id}」と一致するPDFフォルダが存在しません");
                        Debug.WriteLine($"[ERROR] SelectedPdfDocumentを更新できません");
                        return;
                    }
                }
                else
                {
                    ShowMessageWindow("地権者個別資料の項目を選択できません");
                    Debug.WriteLine($"[ERROR] LandownerDocumentsがNULLです");
                    return;
                }

            }
            // それ以外の場合
            else
            {
                if (SelectedPdfDocument == null)
                {
                    ShowMessageWindow("項目を選択してください");
                    Debug.WriteLine($"[ERROR] SelectedPdfDocumentがNULLです");
                    return;
                }
            }

            // 画面遷移
            if(SelectedPdfDocument.FolderPath != null)
            {
                _nav.Navigate(new PdfDocumentManagementViewModel(_nav, _dia, Title, SelectedPdfDocument));
            }
            else
            {
                ShowMessageWindow("項目を選択できません");
                Debug.WriteLine($"[ERROR] SelectedPdfDocument.FolderPathがNULLです");
                return;
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
