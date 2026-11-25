using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using WpfApp.Common.Models;
using WpfApp.Common.Repositories.Interfaces;
using WpfApp.Interfaces;
using WpfApp.Screens.Admin.Views;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using WpfApp.Shell.Models;
using WpfApp.Shell.Views.Header;
using WpfApp.Shell.Views.Sidebar;

namespace WpfApp.Screens.Admin.ViewModels
{
    public partial class WorkLandownerManagementViewModel : ObservableObject, IShellLayout
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly IMessageDialogService _message;
        private readonly IAppPathService _path;
        private readonly IFileService _file;
        private readonly IWorkRepository _workRepos;
        private readonly IWorkLandownerRepository _workLandownerRepos;

        public Type HeaderType => typeof(HeaderType001View);
        public Type SidebarType => typeof(SidebarType003View);

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private bool isExpander1Open = true;

        [ObservableProperty]
        private bool isExpander2Open = true;

        [ObservableProperty]
        private bool isExpander3Open = true;

        [ObservableProperty]
        private int selectedMode = 5;

        /// <summary>
        /// 事業一覧
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<WorkData> works = new();

        /// <summary>
        /// 選択中の事業（nullの場合は未選択状態を表す）
        /// </summary>
        [ObservableProperty]
        private WorkData? selectedWork;

        /// <summary>
        /// 事業に紐付けられた地権者一覧
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<LandownerData> workLandowners = new();

        /// <summary>
        /// 選択中の地権者（nullの場合は未選択状態を表す）
        /// </summary>
        [ObservableProperty]
        private LandownerData? selectedLandowner;

        public WorkLandownerManagementViewModel(NavigationService navigationService, IDialogService dialogService,
            IWorkRepository workRepository, IWorkLandownerRepository workLandownerRepos)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _workRepos = workRepository ?? throw new ArgumentNullException(nameof(workRepository));
            _workLandownerRepos = workLandownerRepos ?? throw new ArgumentNullException(nameof(workLandownerRepos));
            _message = new MessageDialogService();
            _path = new AppPathService();
            _file = new FileService();

            Title = "事業実績管理";
            LoadWorks();
        }

        private async void LoadWorks()
        {
            var list = await _workRepos.GetAllAsync();
            Works = new ObservableCollection<WorkData>(list);
        }

        private async void LoadWorkLandowners(int workId)
        {
            var list = await _workLandownerRepos.GetLandownersByWorkIdAsync(workId);
            WorkLandowners = new ObservableCollection<LandownerData>(list);
        }

        partial void OnSelectedWorkChanged(WorkData? oldValue, WorkData? newValue)
        {
            // 選択中の事業が変更され、変更後の値がnullではない場合
            if (oldValue != newValue && newValue != null)
            {
                LoadWorkLandowners(newValue.WorkId);
                SelectedLandowner = null;
            }
        }

        [RelayCommand]
        private async Task InsertWorkLandowner()
        {
            if (SelectedWork == null) { return; }

            var workId = SelectedWork.WorkId;

            // 地権者選択画面を開く
            var vm = new WorkLandownerSelectViewModel(_workLandownerRepos, workId);
            var window = new WorkLandownerSelectWindow(vm);

            if (window.ShowDialog() == true)
            {
                if (vm.SelectedLandowner == null)
                {
                    _message.ShowMessage("地権者を選択してください。");
                    return;
                }
                // 登録用のモデルを作成する
                var landowner = vm.SelectedLandowner;
                var now = DateTime.Now;
                var workLandowner = new WorkLandownerData()
                {
                    WorkLandownerGuid = Guid.NewGuid().ToString(),
                    WorkId = workId,
                    LandownerId = landowner.LandownerId,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                try
                {
                    // レコードを登録する
                    await _workLandownerRepos.InsertAsync(workLandowner);
                    // 一覧を更新する
                    WorkLandowners.Add(landowner);
                    var sortedList = WorkLandowners.OrderBy(x => x.LandownerId).ToList();
                    WorkLandowners.Clear();
                    foreach (var item in sortedList)
                    {
                        WorkLandowners.Add(item);
                    }
                    SelectedLandowner = landowner;
                    _message.ShowMessage("地権者を追加しました。");
                }
                catch (Exception ex)
                {
                    _message.ShowMessage("地権者情報を追加できませんでした。");
                    Debug.WriteLine($"[ERROR] 地権者を追加できませんでした。LandownerId：{landowner.LandownerId}");
                    Debug.WriteLine(ex);
                    throw;
                }
            }
        }

        [RelayCommand]
        private async Task DeleteWorkLandowner()
        {
            if (SelectedWork == null) { return; }
            if (SelectedLandowner == null) { return; }

            var landownerId = SelectedLandowner.LandownerId;

            if (_message.ShowConfirm("地権者個別資料も削除されます。\nよろしいですか？"))
            {
                // 削除対象のディレクトリを取得する
                var direPath = _path.GetLandownerDire(SelectedWork.FormattedWorkId, SelectedLandowner.FormattedLandownerId);

                try
                {
                    // ディレクトリを削除
                    if (Directory.Exists(direPath))
                    {
                        _file.DeleteDirectory(direPath);
                    }
                }
                catch (IOException ex)
                {
                    _message.ShowMessage("対象のファイルを削除できませんでした。");
                    Debug.WriteLine($"[ERROR] 対象のファイルを削除できませんでした。");
                    Debug.WriteLine(ex);
                    throw;
                }
                catch (UnauthorizedAccessException ex)
                {
                    _message.ShowMessage("削除対象のファイルにアクセス権限がありません。");
                    Debug.WriteLine($"[ERROR] 削除対象のディレクトリやファイルにアクセス権限がありません。");
                    Debug.WriteLine(ex);
                    throw;
                }
                catch (Exception ex)
                {
                    _message.ShowMessage("ファイルの削除時に予期せぬエラーが発生しました。");
                    Debug.WriteLine($"[ERROR] ファイルの削除時に予期せぬエラーが発生しました。");
                    Debug.WriteLine(ex);
                    throw;
                }

                try
                {
                    // レコードを削除する
                    await _workLandownerRepos.DeleteAsync(SelectedWork.WorkId, SelectedLandowner.LandownerId);
                    // 一覧を更新する
                    WorkLandowners.Remove(SelectedLandowner);
                    SelectedLandowner = null;
                    _message.ShowMessage("地権者を削除しました。");
                }
                catch (Exception ex)
                {
                    _message.ShowMessage("地権者を削除できませんでした。\n※資料の削除は完了しました。");
                    Debug.WriteLine($"[ERROR] 地権者を削除できませんでした。LandownerId：{landownerId}");
                    Debug.WriteLine(ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// 管理メニュー画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToAdminMenu()
        {
            var vm = new AdminDocumentViewModel(_nav, _dia);
            _nav.Navigate(vm);
            vm.IsExpander1Open = IsExpander1Open;
            vm.IsExpander2Open = IsExpander2Open;
            vm.IsExpander3Open = IsExpander3Open;
        }

        /// <summary>
        /// メインメニュー画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToMenu()
        {
            if (_message.ShowConfirm("メインメニューに戻ります。よろしいですか？"))
            {
                _nav.Navigate(new MenuViewModel(_nav, _dia));
            }
        }

        /// <summary>
        /// 管理資料画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToAdminDocument()
        {
            var vm = new AdminDocumentViewModel(_nav, _dia);
            _nav.Navigate(vm);
            vm.IsExpander1Open = IsExpander1Open;
            vm.IsExpander2Open = IsExpander2Open;
            vm.IsExpander3Open = IsExpander3Open;
        }

        /// <summary>
        /// 事業情報管理画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToWorkManagement()
        {
            var vm = new WorkManagementViewModel(_nav, _dia, App.WorkRepository);
            _nav.Navigate(vm);
            vm.IsExpander1Open = IsExpander1Open;
            vm.IsExpander2Open = IsExpander2Open;
            vm.IsExpander3Open = IsExpander3Open;
        }

        /// <summary>
        /// 地権者情報管理画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToLandownerManagement()
        {
            var vm = new LandownerManagementViewModel(_nav, _dia, App.LandownerRepository, App.WorkLandownerRepository);
            _nav.Navigate(vm);
            vm.IsExpander1Open = IsExpander1Open;
            vm.IsExpander2Open = IsExpander2Open;
            vm.IsExpander3Open = IsExpander3Open;
        }

        /// <summary>
        /// 事業実績管理画面に留まる。（バインドエラー防止用）
        /// </summary>
        [RelayCommand]
        private void GoToWorkLandownerManagement()
        {
        }

        /// <summary>
        /// 管理者パスワード設定画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToAdminPasswordSettings()
        {
            var vm = new AdminPasswordSettingsViewModel(_nav, _dia, App.AdminPasswordRepository);
            _nav.Navigate(vm);
            vm.IsExpander1Open = IsExpander1Open;
            vm.IsExpander2Open = IsExpander2Open;
            vm.IsExpander3Open = IsExpander3Open;
        }
    }
}
