using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WpfApp.Common.Models;
using WpfApp.Interfaces;
using WpfApp.Screens.Admin.Models;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using static WpfApp.Screens.Admin.Models.TabItemData;
using static WpfApp.Screens.Admin.ViewModels.DisplaySettingsViewModel;
using static WpfApp.Screens.Admin.ViewModels.ScreenSelectViewModel;

namespace WpfApp.Screens.Admin.ViewModels
{
    /// <summary>
    /// PdfDocumentSettingsView.xamlに対応するViewModel。
    /// PDF資料設定画面での処理を行う。
    /// </summary>
    public partial class PdfDocumentSettingsViewModel : ObservableObject , INavigationTarget
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly IMessageDialogService _message;
        private readonly IAppPathService _path;

        public Func<object>? CreateBackViewModel => () => new ScreenSelectViewModel(_nav, _dia, DisplaySettingsCategory.PdfDocumentSettings);

        [ObservableProperty]
        private bool isLeftDrawerOpen;

        private readonly ScreenCategory _selectedScreen;

        [ObservableProperty]
        private bool isAramashi;

        [ObservableProperty]
        private bool isDocument;

        [ObservableProperty]
        private string title;

        private static readonly string _titleItem = "PDF資料設定";

        public Dictionary<ScreenCategory, string> Screens { get; } = new Dictionary<ScreenCategory, string>
        {
            { ScreenCategory.Aramashi, "補償のあらまし" },
            { ScreenCategory.Document, "各種資料" }
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

        public PdfDocumentSettingsViewModel(NavigationService navigationService, IDialogService dialogService, ScreenCategory selectedScreen)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _message = new MessageDialogService();
            _path = new AppPathService();

            _selectedScreen = selectedScreen;
            Title = GetTitle();
            SetTabItems();
            InitializeViewState();
        }

        /// <summary>
        /// 前の画面で選択された画面の名称を取得する。
        /// </summary>
        /// <returns></returns>
        private string? GetSelectedScreenTitle()
        {
            Screens.TryGetValue(_selectedScreen, out var selectedScreentitle);

            return selectedScreentitle;
        }

        /// <summary>
        /// 画面タイトルの文字列を取得する。
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 表示するタブを設定する。
        /// </summary>
        private void InitializeViewState()
        {
            if(_selectedScreen == ScreenCategory.Aramashi)
            {
                IsAramashi = true;
                IsDocument = false;
                SelectedTabIndex = 0;
                Debug.WriteLine($"[DEBUG] {_selectedScreen}のPDF資料選択項目を表示します");
            }
            else if(_selectedScreen == ScreenCategory.Document)
            {
                IsAramashi = false;
                IsDocument = true;
                SelectedTabIndex = 2;
                Debug.WriteLine($"[DEBUG] {_selectedScreen}のPDF資料選択項目を表示します");
            }
            else
            {
                Debug.WriteLine($"[ERROR] _selectedScreenがNULLもしくは不正な値です");
                _message.ShowMessage("選択項目を表示できません。");
            }
        }

        /// <summary>
        /// 各タブ内に表示するデータを設定する。
        /// </summary>
        private void SetTabItems()
        {
            var items = new ObservableCollection<TabItemData>();

            var tab = new TabItemData();
            var landowners = new ObservableCollection<LandownerData>();
            var documents = new ObservableCollection<PdfDocumentData>();
            var doc = new PdfDocumentData();


            /* 用地補償の手順 */
            documents.Add(GetDocumentData("用地補償の手順", _path.AramashiPdfDire, "Aramashi"));
            tab.Documents = documents;
            // TabItemリストに格納
            items.Add(tab);


            /* その他 */
            tab = new TabItemData();
            documents = new ObservableCollection<PdfDocumentData>();
            
            var othersButtons = LoadButtonFromJson(_path.OthersButtonPath);
            foreach (var button in othersButtons)
            {
                if (button.BtnId != "001")
                {
                    documents.Add(GetDocumentData(button.LabelText, Path.Combine(_path.OthersPdfDire, button.BtnId), button.BtnId));
                }
            }
            tab.Documents = documents;
            // TabItemリストに格納
            items.Add(tab);


            /* 事業全般資料 Works */
            tab = new TabItemData();
            documents = new ObservableCollection<PdfDocumentData>();

            var worksButtons = LoadButtonFromJson(_path.WorksButtonPath);
            foreach (var button in worksButtons)
            {
                documents.Add(GetDocumentData(button.LabelText, Path.Combine(_path.WorksPdfDire, button.BtnId), button.BtnId));
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
            var landownerButtons = LoadButtonFromJson(_path.LandownerButtonPath);

            foreach (var owner in landowners)
            {
                // 初期化
                documents = new ObservableCollection<PdfDocumentData>();

                for (int i = 0; i < landownerButtons.Count; i++)
                {
                    var button = landownerButtons[i];
                    // PDFデータフォルダのパスを取得して格納
                    documents.Add(GetDocumentData(button.LabelText, Path.Combine(_path.LandownersPdfDire, owner.LandownerId, button.BtnId), button.BtnId));
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

        /// <summary>
        /// 引数として受け取ったデータをPdfDocumentDataに格納して返す。
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="folderPath"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private PdfDocumentData GetDocumentData(string displayName, string folderPath, string id)
        {
            var doc = new PdfDocumentData();

            doc.DisplayName = displayName;
            doc.FolderPath = folderPath;
            doc.Id = id;

            return doc;
        }

        /// <summary>
        /// ボタンの設定データを取得する。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private List<ButtonConfig> LoadButtonFromJson(string path)
        {
            var buttons = new List<ButtonConfig>();

            if (!File.Exists(path))
            {
                _message.ShowMessage("項目を表示できません。");
                Debug.WriteLine($"[DEBUG] ボタンデータが存在しません: {path}");
            }
            else
            {
                string json = File.ReadAllText(path);
                buttons = JsonSerializer.Deserialize<List<ButtonConfig>>
                    (json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ButtonConfig>();
                buttons = buttons.OrderBy(x => x.BtnId).ToList();
            }
            return buttons;
        }

        /// <summary>
        /// 地権者ユーザのデータを取得する。
        /// </summary>
        /// <returns></returns>
        private ObservableCollection<LandownerData> LoadLandownersFromJson()
        {
            var landowners = new ObservableCollection<LandownerData>();

            try
            {
                string path = Path.Combine(_path.LandownersPath);

                if (!File.Exists(path))
                {
                    _message.ShowMessage("地権者ユーザを表示できません。");
                    Debug.WriteLine($"[DEBUG] 地権者データが存在しません: {path}");
                }

                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<LandownerData>>(json) ?? new List<LandownerData>();
                list = list.OrderBy(x => x.LandownerId).ToList();

                landowners = new ObservableCollection<LandownerData>(list);
            }
            catch (Exception ex)
            {
                _message.ShowMessage("地権者データを読み込むことができません。");
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
        /// 項目を選択し、PDF資料管理画面へ遷移する。
        /// 地権者個別資料タブの場合は、地権者ユーザと項目を選択する。
        /// </summary>
        [RelayCommand]
        private void Select()
        {
            // 地権者個別資料の場合
            if (SelectedTabIndex == 3)
            {
                if(SelectedLandowner == null)
                {
                    _message.ShowMessage("地権者ユーザを選択してください。");
                    Debug.WriteLine($"[ERROR] SelectedLandownerがNULLです");
                    return;
                }

                if (SelectedPdfDocument == null)
                {
                    _message.ShowMessage("項目を選択してください。");
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
                        _message.ShowMessage("地権者個別資料の項目を選択できません。");
                        Debug.WriteLine($"[ERROR] LandownerDocuments[{SelectedLandowner.LandownerId}]に「{SelectedPdfDocument.Id}」と一致するPDFフォルダが存在しません");
                        Debug.WriteLine($"[ERROR] SelectedPdfDocumentを更新できません");
                        return;
                    }
                }
                else
                {
                    _message.ShowMessage("地権者個別資料の項目を選択できません。");
                    Debug.WriteLine($"[ERROR] LandownerDocumentsがNULLです");
                    return;
                }

            }
            // それ以外の場合
            else
            {
                if (SelectedPdfDocument == null)
                {
                    _message.ShowMessage("項目を選択してください。");
                    Debug.WriteLine($"[ERROR] SelectedPdfDocumentがNULLです");
                    return;
                }
            }

            // 画面遷移
            if(SelectedPdfDocument.FolderPath != null)
            {
                _nav.Navigate(new PdfDocumentManagementViewModel(_nav, _dia, Title, SelectedPdfDocument, _selectedScreen));
            }
            else
            {
                _message.ShowMessage("項目を選択できません。");
                Debug.WriteLine($"[ERROR] SelectedPdfDocument.FolderPathがNULLです");
                return;
            }
        }
    }
}
