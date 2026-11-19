using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using WpfApp.Common.Models;
using WpfApp.Common.Repositories.Interfaces;
using WpfApp.Common.ViewModels;
using WpfApp.Common.Views;
using WpfApp.Interfaces;
using WpfApp.Screens.Aramashi.ViewModels;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using WpfApp.Shell.Models;
using WpfApp.Shell.Views.Header;
using WpfApp.Shell.Views.Sidebar;

namespace WpfApp.Screens.Documents.ViewModels
{
    /// <summary>
    /// DocumentMenuView.xamlに対応するViewModel。
    /// 各種資料画面での処理を行う。
    /// </summary>
    public partial class DocumentMenuViewModel : ObservableObject, IShellLayout
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly PdfDocumentService _pdf;
        private readonly IMessageDialogService _message;
        private readonly IAppPathService _path;
        private readonly IWorkRepository _workRepos;
        private readonly IWorkLandownerRepository _workLandownerRepos;

        public Type HeaderType => typeof(HeaderType001View);
        public Type SidebarType => typeof(SidebarType001View);

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private int selectedMode = 1;

        /// <summary>
        /// ボタンの名称表示用ボタンデータリスト（事業全般資料）
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<ButtonConfig>? worksButtons;

        /// <summary>
        /// ボタンの名称表示用ボタンデータリスト（地権者個別資料）
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<ButtonConfig>? landownerButtons;

        /// <summary>
        /// 事業リスト
        /// </summary>
        [ObservableProperty]
        private List<WorkData> works = new ();

        /// <summary>
        /// 選択中の事業（nullの場合は未選択状態を表す）
        /// </summary>
        [ObservableProperty]
        private WorkData? selectedWork;

        /// <summary>
        /// 事業に紐付けられた地権者リスト
        /// </summary>
        [ObservableProperty]
        private List<LandownerData> workLandowners = new ();

        /// <summary>
        /// 選択中の地権者（nullの場合は未選択状態を表す）
        /// </summary>
        [ObservableProperty]
        private LandownerData? selectedLandowner;

        [ObservableProperty]
        private bool isPasswordCorrect = false;

        [ObservableProperty]
        private bool isNotPasswordCorrect = true;

        public bool IsWorkSelected => SelectedWork != null;

        public bool IsLandownerSelected => SelectedLandowner != null;

        public bool IsNotLandownerSelected => SelectedLandowner == null;


        public DocumentMenuViewModel(NavigationService navigationService, IDialogService dialogService,
            IWorkRepository workRepository, IWorkLandownerRepository workLandownerRepository)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _pdf = new PdfDocumentService();
            _message = new MessageDialogService();
            _path = new AppPathService();
            _workRepos = workRepository;
            _workLandownerRepos = workLandownerRepository;

            Title = "各種資料";

            WorksButtons = LoadButtonFromJson(_path.WorksButtonPath);
            LandownerButtons = LoadButtonFromJson(_path.LandownerButtonPath);
            LoadWorks();
        }

        /// <summary>
        /// ボタンに表示する名称を取得する。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private ObservableCollection<ButtonConfig> LoadButtonFromJson(string path)
        {
            var buttons = new ObservableCollection<ButtonConfig>();

            if (!File.Exists(path))
            {
                _message.ShowMessage("ボタンの名称を表示できません。");
                Debug.WriteLine($"[DEBUG] ボタンデータが存在しません: {path}");
            }
            else
            {
                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<ButtonConfig>>
                    (json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if(list == null || list.Count <= 0)
                {
                    _message.ShowMessage("ボタンの名称を表示できません。");
                    Debug.WriteLine($"[DEBUG] ボタンデータが空です: {path}");
                }
                else
                {
                    list = list.OrderBy(x => x.BtnId).ToList();

                    foreach (var btn in list)
                    {
                        buttons.Add(btn);
                    }
                    Debug.WriteLine($"[DEBUG] ボタンデータ読み込み完了: {path} 件数 = {buttons.Count}");
                }
            }

            return buttons;
        }
        private async void LoadWorks()
        {
            Works = await _workRepos.GetAllAsync();
        }

        partial void OnSelectedWorkChanged(WorkData? oldValue, WorkData? newValue)
        {
            OnPropertyChanged(nameof(IsWorkSelected));

            // 選択中の事業が変更され、変更後の値がnullではない場合
            if (oldValue != newValue && newValue != null)
            {
                LoadWorkLandowners(newValue.WorkId);
            }
        }

        private async void LoadWorkLandowners(int workId)
        {
            WorkLandowners = await _workLandownerRepos.GetLandownersByWorkIdAsync(workId);
            SelectedLandowner = null;
        }

        partial void OnSelectedLandownerChanged(LandownerData? value)
        {
            OnPropertyChanged(nameof(IsLandownerSelected));
            OnPropertyChanged(nameof(IsNotLandownerSelected));
        }

        /// <summary>
        /// メインメニュー画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToMenu()
        {
            _nav.Navigate(new MenuViewModel(_nav, _dia));
        }

        /// <summary>
        /// 補償のあらまし画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToAramashiMenu()
        {
            _nav.Navigate(new AramashiMenuViewModel(_nav, _dia));
        }

        /// <summary>
        /// 各種資料画面に留まる。（バインドエラー防止用）
        /// </summary>
        [RelayCommand]
        private void GoToDocumentMenu()
        {
        }

        /// <summary>
        /// パスワード画面を表示する。
        /// パスワードが正しい場合に地権者ユーザ選択画面を表示する。
        /// </summary>
        [RelayCommand]
        private void OpenPasswordDialog()
        {
            // パスワードダイアログを生成
            var pwVm = new PasswordDialogViewModel(App.AdminPasswordRepository);
            var pwDialog = new PasswordDialogWindow
            {
                DataContext = pwVm,
                Owner = Application.Current.MainWindow
            };

            // パスワード入力後にOKボタンが押下された場合
            if (pwDialog.ShowDialog() == true)
            {
                // パスワードが正しい場合
                if (pwVm.Result)
                {
                    IsPasswordCorrect = true;
                    IsNotPasswordCorrect = false;
                }
            }
        }

        /// <summary>
        /// 事業全般資料の各項目に対応する資料選択画面を表示する。
        /// </summary>
        /// <param name="folderName"></param>
        [RelayCommand]
        private void OpenWorksDocument(string folderName)
        {
            if (SelectedWork == null) return;
            if (string.IsNullOrWhiteSpace(folderName)) return;
            var folderPath = _path.GetWorksDire(SelectedWork.WorkId, folderName);
            var pdfList = _pdf.GetPdfList(folderPath);

            // PDF資料が存在しない場合
            if (pdfList.Count <= 0)
            {
                Debug.WriteLine($"[DEBUG] ファイルが存在しません {folderPath}");
                //メッセージウィンドウを表示
                _message.ShowMessage("ファイルが存在しません");
            }
            else
            {
                // 非モーダル表示
                _dia.ShowWindow<DocumentSelectViewModel>(pdfList);
            }
        }

        /// <summary>
        /// 地権者個別資料の各項目に対応する資料選択画面を表示する。
        /// </summary>
        /// <param name="folderName"></param>
        [RelayCommand]
        private void OpenLandownerDocument(string folderName)
        {
            if (SelectedWork == null) return;
            if (SelectedLandowner == null) return;
            if (string.IsNullOrWhiteSpace(folderName)) return;

            var folderPath = _path.GetLandownersDire(SelectedWork.WorkId, SelectedLandowner.LandownerId, folderName);
            var pdfList = _pdf.GetPdfList(folderPath);

            // PDF資料が存在しない場合
            if (pdfList.Count <= 0)
            {
                Debug.WriteLine($"[DEBUG] ファイルが存在しません {folderPath}");
                //メッセージウィンドウを表示
                _message.ShowMessage("ファイルが存在しません");
            }
            else
            {
                // 非モーダル表示
                _dia.ShowWindow<DocumentSelectViewModel>(pdfList);
            }
        }
    }
}
