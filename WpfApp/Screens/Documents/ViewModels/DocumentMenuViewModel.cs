using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        private readonly IFileService _file;
        private readonly IMessageDialogService _message;
        private readonly IAppPathService _path;
        private readonly IWorkRepository _workRepos;
        private readonly IWorkLandownerRepository _workLandownerRepos;
        private readonly IButtonRepository _buttonRepos;

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
        private ObservableCollection<ButtonData> workButtons = new();

        /// <summary>
        /// ボタンの名称表示用ボタンデータリスト（地権者個別資料）
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<ButtonData> landownerButtons = new();

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
            IWorkRepository workRepository, IWorkLandownerRepository workLandownerRepository,
            IButtonRepository buttonRepository)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _workRepos = workRepository ?? throw new ArgumentNullException(nameof(workRepository));
            _workLandownerRepos = workLandownerRepository ?? throw new ArgumentNullException(nameof(workLandownerRepository));
            _buttonRepos = buttonRepository ?? throw new ArgumentNullException(nameof(buttonRepository));
            _file = new FileService();
            _message = new MessageDialogService();
            _path = new AppPathService();


            Title = "各種資料";

            LoadWorkButtons();
            LoadLandownerButtons();
            LoadWorks();
        }

        private async void LoadWorkButtons()
        {
            var list = await _buttonRepos.GetButtonsAsync(3);
            WorkButtons = new ObservableCollection<ButtonData>(list);
        }

        private async void LoadLandownerButtons()
        {
            var list = await _buttonRepos.GetButtonsAsync(4);
            LandownerButtons = new ObservableCollection<ButtonData>(list);
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
        /// <param name="buttonId"></param>
        private void OpenWorkDocument(int buttonId)
        {
            if (SelectedWork == null) { return; }
            if (!WorkButtonsContainsIndex(buttonId)) { return; }

            var folderName = WorkButtons[buttonId].DocumentDireName;

            if (string.IsNullOrWhiteSpace(folderName)) { return; }

            var direPath = _path.GetWorkDocumentDire(SelectedWork.FormattedWorkId, folderName);
            var list = _file.GetFiles(direPath);

            // 資料が存在しない場合
            if (list.Count <= 0)
            {
                Debug.WriteLine($"[DEBUG] ファイルが存在しません {direPath}");
                //メッセージウィンドウを表示
                _message.ShowMessage("ファイルが存在しません。");
            }
            else
            {
                // 非モーダル表示
                _dia.ShowWindow<DocumentSelectViewModel>(list);
            }
        }

        private bool WorkButtonsContainsIndex(int index)
        {
            return index < WorkButtons.Count;
        }

        [RelayCommand]
        private void OpenWorkDocument01()
        {
            OpenWorkDocument(0);
        }

        [RelayCommand]
        private void OpenWorkDocument02()
        {
            OpenWorkDocument(1);
        }

        [RelayCommand]
        private void OpenWorkDocument03()
        {
            OpenWorkDocument(2);
        }

        [RelayCommand]
        private void OpenWorkDocument04()
        {
            OpenWorkDocument(3);
        }

        [RelayCommand]
        private void OpenWorkDocument05()
        {
            OpenWorkDocument(4);
        }

        [RelayCommand]
        private void OpenWorkDocument06()
        {
            OpenWorkDocument(5);
        }

        [RelayCommand]
        private void OpenWorkDocument07()
        {
            OpenWorkDocument(6);
        }

        [RelayCommand]
        private void OpenWorkDocument08()
        {
            OpenWorkDocument(7);
        }

        /// <summary>
        /// 地権者個別資料の各項目に対応する資料選択画面を表示する。
        /// </summary>
        /// <param name="buttonId"></param>
        private void OpenLandownerDocument(int buttonId)
        {
            if (SelectedWork == null) { return; }
            if (SelectedLandowner == null) { return; }
            if (!LandownerButtonsContainsIndex(buttonId)) { return; }

            var folderName = LandownerButtons[buttonId].DocumentDireName;

            if (string.IsNullOrWhiteSpace(folderName)) { return; }

            var direPath = _path.GetLandownerDocumentDire(SelectedWork.FormattedWorkId, 
                SelectedLandowner.FormattedLandownerId, folderName);
            var list = _file.GetFiles(direPath);

            // 資料が存在しない場合
            if (list.Count <= 0)
            {
                Debug.WriteLine($"[DEBUG] ファイルが存在しません {direPath}");
                //メッセージウィンドウを表示
                _message.ShowMessage("ファイルが存在しません。");
            }
            else
            {
                // 非モーダル表示
                _dia.ShowWindow<DocumentSelectViewModel>(list);
            }
        }

        private bool LandownerButtonsContainsIndex(int index)
        {
            return index < LandownerButtons.Count;
        }

        [RelayCommand]
        private void OpenLandownerDocument01()
        {
            OpenLandownerDocument(0);
        }

        [RelayCommand]
        private void OpenLandownerDocument02()
        {
            OpenLandownerDocument(1);
        }

        [RelayCommand]
        private void OpenLandownerDocument03()
        {
            OpenLandownerDocument(2);
        }

        [RelayCommand]
        private void OpenLandownerDocument04()
        {
            OpenLandownerDocument(3);
        }

        [RelayCommand]
        private void OpenLandownerDocument05()
        {
            OpenLandownerDocument(4);
        }

        [RelayCommand]
        private void OpenLandownerDocument06()
        {
            OpenLandownerDocument(5);
        }

        [RelayCommand]
        private void OpenLandownerDocument07()
        {
            OpenLandownerDocument(6);
        }

        [RelayCommand]
        private void OpenLandownerDocument08()
        {
            OpenLandownerDocument(7);
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
            _nav.Navigate(new AramashiMenuViewModel(_nav, _dia, _buttonRepos));
        }

        /// <summary>
        /// 各種資料画面に留まる。（バインドエラー防止用）
        /// </summary>
        [RelayCommand]
        private void GoToDocumentMenu()
        {
        }
    }
}
