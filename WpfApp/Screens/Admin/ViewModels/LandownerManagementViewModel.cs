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
    public partial class LandownerManagementViewModel : ObservableObject, IShellLayout
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly IMessageDialogService _message;
        private readonly IAppPathService _path;
        private readonly ILandownerRepository _landownerRepos;
        private readonly IWorkLandownerRepository _worklandownerRepos;

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
        private int selectedMode = 4;

        /// <summary>
        /// 地権者一覧
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<LandownerData> landowners = new();

        /// <summary>
        /// 選択中の地権者（nullの場合は未選択状態を表す）
        /// </summary>
        [ObservableProperty]
        private LandownerData? selectedLandowner;

        public LandownerManagementViewModel(NavigationService navigationService, IDialogService dialogService,
            ILandownerRepository landownerRepository, IWorkLandownerRepository workLandownerRepository)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _landownerRepos = landownerRepository ?? throw new ArgumentNullException(nameof(landownerRepository));
            _worklandownerRepos = workLandownerRepository ?? throw new ArgumentNullException(nameof(workLandownerRepository));
            _message = new MessageDialogService();
            _path = new AppPathService();

            Title = "地権者情報管理";
            LoadLandowners();
        }

        private async void LoadLandowners()
        {
            var list = await _landownerRepos.GetAllAsync();
            Landowners = new ObservableCollection<LandownerData>(list);
        }

        [RelayCommand]
        private async Task InsertLandowner()
        {
            // 新規登録用のモデルを作成する
            var landowner = new LandownerData();

            // 新規登録画面を開く
            var vm = new LandownerEditViewModel(landowner, true);
            var window = new LandownerEditWindow(vm);

            if (window.ShowDialog() == true)
            {
                // 新規登録画面から入力データを取得する
                var editedLandowner = vm.Landowner;

                if (editedLandowner == null)
                {
                    _message.ShowMessage("地権者情報を登録できません。");
                    return;
                }

                // 識別用Guidを設定する
                editedLandowner.LandownerGuid = Guid.NewGuid().ToString();
                // 作成日時、更新日時を設定する
                var now = DateTime.Now;
                editedLandowner.CreatedAt = now;
                editedLandowner.UpdatedAt = now;

                try
                {
                    // レコードを登録する
                    var landownerId = await _landownerRepos.InsertAndGetLandownerIdAsync(editedLandowner);
                    // 自動採番された地権者IDを設定する
                    editedLandowner.LandownerId = landownerId;
                    // 一覧を更新する
                    Landowners.Add(editedLandowner);
                    SelectedLandowner = editedLandowner;
                    _message.ShowMessage("地権者情報を登録しました。");
                }
                catch (Exception ex)
                {
                    _message.ShowMessage("地権者情報を登録できませんでした。");
                    Debug.WriteLine($"[ERROR] 地権者を登録できませんでした。LandownerId：{editedLandowner.LandownerId}");
                    Debug.WriteLine(ex);
                    throw;
                }
            }
        }

        [RelayCommand]
        private async Task UpdateLandowner()
        {
            if (SelectedLandowner == null) { return; }

            // 編集用のモデルを作成する
            var landowner = new LandownerData()
            {
                LandownerId = SelectedLandowner.LandownerId,
                LandownerGuid = SelectedLandowner.LandownerGuid,
                LandownerName = SelectedLandowner.LandownerName,
                LandownerKana = SelectedLandowner.LandownerKana,
                UpdatedAt = SelectedLandowner.UpdatedAt,
                CreatedAt = SelectedLandowner.CreatedAt
            };

            // 編集画面を開く
            var vm = new LandownerEditViewModel(landowner, false);
            var window = new LandownerEditWindow(vm);

            if (window.ShowDialog() == true)
            {
                // 編集画面から入力データを取得する
                var editedLandowner = vm.Landowner;

                if (editedLandowner == null)
                {
                    _message.ShowMessage("地権者情報を更新できません。");
                    return;
                }

                // 更新日時を設定する
                editedLandowner.UpdatedAt = DateTime.Now;

                try
                {
                    // レコードを更新する
                    await _landownerRepos.UpdateAsync(editedLandowner);
                    // 一覧を更新する
                    var target = Landowners.FirstOrDefault(x => x.LandownerId == editedLandowner.LandownerId);
                    if (target == null)
                    {
                        LoadLandowners();
                        SelectedLandowner = Landowners.FirstOrDefault(x => x.LandownerId == editedLandowner.LandownerId);
                    }
                    else
                    {
                        target.LandownerName = editedLandowner.LandownerName;
                        target.LandownerKana = editedLandowner.LandownerKana;
                        target.UpdatedAt = editedLandowner.UpdatedAt;
                    }
                    _message.ShowMessage("地権者情報を更新しました。");
                }
                catch (Exception ex)
                {
                    _message.ShowMessage("地権者情報を更新できませんでした。");
                    Debug.WriteLine($"[ERROR] 地権者を更新できませんでした。LandownerId：{editedLandowner.LandownerId}");
                    Debug.WriteLine(ex);
                    throw;
                }
            }
        }

        [RelayCommand]
        private async Task DeleteLandowner()
        {
            if (SelectedLandowner == null) { return; }

            var landownerId = SelectedLandowner.LandownerId;

            if (_message.ShowConfirm("各事業に紐付けられた地権者個別資料も削除されます。\nよろしいですか？"))
            {

                try
                {
                    // 対象の地権者と事業の紐付け情報を取得する
                    var workLandowners = await _worklandownerRepos.GetWorkLandownersByLandownerId(landownerId);
                    // 削除対象となるディレクトリを取得する
                    var pathList = new List<string>();
                    foreach (var worklandowner in workLandowners)
                    {
                        var path = _path.GetLandownerDire(worklandowner.FormattedWorkId, worklandowner.FormattedLandownerId);
                        pathList.Add(path);
                    }

                    // ディレクトリごと削除する（サービス作成後追加）

                }
                catch (Exception ex)
                {
                    _message.ShowMessage("各事業に紐付けられた地権者個別資料を削除できませんでした。");
                    Debug.WriteLine($"[ERROR] 各事業に紐付けられた地権者個別資料を削除できませんでした。");
                    Debug.WriteLine(ex);
                    throw;
                }

                try
                {
                    // レコードを削除する（事業と地権者の紐付けレコードは自動削除される）
                    await _landownerRepos.DeleteAsync(landownerId);
                    // 一覧を更新する
                    Landowners.Remove(SelectedLandowner);
                    SelectedLandowner = null;
                    _message.ShowMessage("地権者情報を削除しました。");
                }
                catch (Exception ex)
                {
                    _message.ShowMessage("地権者情報を削除できませんでした。");
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
        /// 地権者情報管理画面に留まる。（バインドエラー防止用）
        /// </summary>
        [RelayCommand]
        private void GoToLandownerManagement()
        {
        }

        /// <summary>
        /// 事業実績管理画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToWorkLandownerManagement()
        {
            var vm = new WorkLandownerManagementViewModel(_nav, _dia, App.WorkRepository, App.WorkLandownerRepository);
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
