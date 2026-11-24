using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    public partial class WorkManagementViewModel : ObservableObject, IShellLayout
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly IMessageDialogService _message;
        private readonly IAppPathService _path;
        private readonly IWorkRepository _workRepos;

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
        private int selectedMode = 3;

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

        public WorkManagementViewModel(NavigationService navigationService, IDialogService dialogService,
            IWorkRepository workRepository)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _workRepos = workRepository ?? throw new ArgumentNullException(nameof(workRepository));
            _message = new MessageDialogService();
            _path = new AppPathService();

            Title = "事業情報管理";
            LoadWorks();
        }

        private async void LoadWorks()
        {
            var list = await _workRepos.GetAllAsync();
            Works = new ObservableCollection<WorkData>(list);
        }

        [RelayCommand]
        private async Task InsertWork()
        {
            // 新規作成用のモデルを作成する
            var work = new WorkData();

            // 新規登録画面を開く
            var vm = new WorkEditViewModel(work, true);
            var window = new WorkEditWindow(vm);

            if (window.ShowDialog() == true)
            {
                // 新規追加画面から入力データを取得
                var editedWork = vm.Work;

                if (editedWork == null)
                {
                    _message.ShowMessage("事業情報を登録できません。");
                    return;
                }
                // 識別用Guidを設定する
                editedWork.WorkGuid = Guid.NewGuid().ToString();
                // 作成日時、更新日時を設定する
                var now = DateTime.Now;
                editedWork.CreatedAt = now;
                editedWork.UpdatedAt = now;

                try
                {
                    // レコードを登録する
                    var workId = await _workRepos.InsertAndGetWorkIdAsync(editedWork);
                    // 自動採番されたWorkIdを設定する
                    editedWork.WorkId = workId;
                    // 一覧を更新する
                    Works.Add(editedWork);
                    SelectedWork = editedWork;
                    _message.ShowMessage("事業情報を登録しました。");
                }
                catch (Exception ex)
                {
                    _message.ShowMessage("事業情報を登録できませんでした。");
                    Debug.WriteLine($"[ERROR] 事業を登録できませんでした。WorkId：{editedWork.WorkId}");
                    Debug.WriteLine(ex);
                    throw;
                }
            }
        }

        [RelayCommand]
        private async Task UpdateWork()
        {
            if (SelectedWork == null) { return; }

            // 編集用のモデルを作成する
            var work = new WorkData()
            {
                WorkId = SelectedWork.WorkId,
                WorkGuid = SelectedWork.WorkGuid,
                WorkName = SelectedWork.WorkName,
                CreatedAt = SelectedWork.CreatedAt,
                UpdatedAt = SelectedWork.UpdatedAt
            };

            // 編集画面を開く
            var vm = new WorkEditViewModel(work, false);
            var window = new WorkEditWindow(vm);

            if (window.ShowDialog() == true)
            {
                // 編集画面から入力データを取得
                var editedWork = vm.Work;

                if (editedWork == null)
                {
                    _message.ShowMessage("事業情報を更新できません。");
                    return;
                }
                // 更新日時を設定する
                editedWork.UpdatedAt = DateTime.Now;

                try
                {
                    // レコードを更新する
                    await _workRepos.UpdateAsync(editedWork);
                    // 一覧を更新する
                    var target = Works.FirstOrDefault(x => x.WorkId == editedWork.WorkId);
                    if (target == null)
                    {
                        LoadWorks();
                        SelectedWork = Works.FirstOrDefault(x => x.WorkId == editedWork.WorkId);
                    }
                    else
                    {
                        target.WorkName = editedWork.WorkName;
                        target.UpdatedAt = editedWork.UpdatedAt;
                    }
                    _message.ShowMessage("事業情報を更新しました。");
                }
                catch (Exception ex)
                {
                    _message.ShowMessage("事業情報を更新できませんでした。");
                    Debug.WriteLine($"[ERROR] 事業を更新できませんでした。WorkId：{editedWork.WorkId}");
                    Debug.WriteLine(ex);
                    throw;
                }
            }
        }

        [RelayCommand]
        private async Task DeleteWork()
        {
            if (SelectedWork == null) { return; }

            var workId = SelectedWork.WorkId;

            if (_message.ShowConfirm("事業全般資料や地権者個別資料も削除されます。\nよろしいですか？"))
            {
                try
                {
                    // 紐付けられたディレクトリを取得
                    var workAllDire = _path.GetWorksAllDire(SelectedWork.FormattedWorkId);
                    // ディレクトリごと削除

                }
                catch (Exception ex)
                {
                    _message.ShowMessage("関連付けられた資料を削除できませんでした。");
                    Debug.WriteLine("[ERROR] 事業全般資料や地権者個別資料を削除できませんでした。");
                    Debug.WriteLine(ex);
                    throw;
                }

                try
                {
                    // レコードを削除する（事業と地権者の紐付けレコードは自動削除される）
                    await _workRepos.DeleteAsync(workId);
                    // 事業一覧を更新する
                    Works.Remove(SelectedWork);
                    SelectedWork = null;
                    _message.ShowMessage("事業情報を削除しました。");
                }
                catch (Exception ex)
                {
                    _message.ShowMessage("事業情報を削除できませんでした。");
                    Debug.WriteLine($"[ERROR] 事業を削除できませんでした。WorkId：{workId}");
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
        /// 事業情報管理画面に留まる。（バインドエラー防止用）
        /// </summary>
        [RelayCommand]
        private void GoToWorkManagement()
        {
        }

        /// <summary>
        /// 地権者情報管理画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToLandownerManagement()
        {
            var vm = new LandownerManagementViewModel(_nav, _dia, App.LandownerRepository);
            _nav.Navigate(vm);
            vm.IsExpander1Open = IsExpander1Open;
            vm.IsExpander2Open = IsExpander2Open;
            vm.IsExpander3Open = IsExpander3Open;
        }

        /// <summary>
        /// 事業実績管理画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToWorkLandownerManagement()
        {
            var vm = new WorkLandownerManagementViewModel(_nav, _dia, App.WorkLandownerRepository);
            _nav.Navigate(vm);
            vm.IsExpander1Open = IsExpander1Open;
            vm.IsExpander2Open = IsExpander2Open;
            vm.IsExpander3Open = IsExpander3Open;
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
