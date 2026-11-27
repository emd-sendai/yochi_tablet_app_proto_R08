using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using WpfApp.Common.Enums;
using WpfApp.Common.Extensions;
using WpfApp.Common.Models;
using WpfApp.Interfaces;
using WpfApp.Screens.Admin.Enums;
using WpfApp.Screens.Admin.Extensions;
using WpfApp.Screens.Admin.Models;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using WpfApp.Shell.Models;
using WpfApp.Shell.Views.Header;
using WpfApp.Shell.Views.Sidebar;

namespace WpfApp.Screens.Admin.ViewModels
{
    public partial class FileManagementViewModel : ObservableObject, IShellLayout
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly IMessageDialogService _message;
        private readonly IFileService _file;
        private readonly IAppPathService _path;
        private readonly string _direPath;

        // 対応ファイル
        private const string AvailableExtension = "対応ファイル (*.pdf;*.xlsx;*.xls;*.xlsm;*.docx;*.doc;*.docm;*.pptx;*.ppt;*.pptm;*.mp4)" +
            "|*.pdf;*.xlsx;*.xls;*.xlsm;*.docx;*.doc;*.docm;*.pptx;*.ppt;*.pptm;*.mp4|";
        private const string PdfExtension = "PDFファイル (*.pdf)|*.pdf|";
        private const string ExcelExtension = "Excelファイル (*.xlsx;*.xls;*.xlsm)|*.xlsx;*.xls;*.xlsm|";
        private const string WordExtension = "Wordファイル (*.docx;*.doc;*.docm)|*.docx;*.doc;*.docm|";
        private const string PowerPointExtension = "PowerPointファイル (*.pptx;*.ppt;*.pptm)|*.pptx;*.ppt;*.pptm|";
        private const string Mp4Extension = "動画ファイル (*.mp4)|*.mp4";

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
        private int selectedMode = 100;

        [ObservableProperty]
        private ObservableCollection<FileData> files = new();

        [ObservableProperty]
        private FileData? selectedFile;

        [ObservableProperty]
        private string documentName = string.Empty;

        public FileManagementViewModel(NavigationService navigationService, IDialogService dialogService,
            int selectedMode, string documentName, string title, string direPath)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _message = new MessageDialogService();
            _file = new FileService();
            _path = new AppPathService();

            SelectedMode = selectedMode;
            DocumentName = documentName;
            Title = title;
            _direPath = direPath;

            GetFiles();
        }

        private void GetFiles()
        {
            var path = _direPath;

            if (Directory.Exists(path))
            {
                try
                {
                    var list = _file.GetFiles(path);
                    Files = new ObservableCollection<FileData>(list);
                }
                catch (DirectoryNotFoundException ex)
                {
                    _message.ShowMessage($"{FileGetFailureReason.DirectoryNotFound.GetErrorMessage()}");
                    Debug.WriteLine($"[ERROR] {FileGetFailureReason.DirectoryNotFound.GetErrorMessage()}");
                    Debug.WriteLine(ex);
                    throw;
                }
                catch (UnauthorizedAccessException ex)
                {
                    _message.ShowMessage($"{FileGetFailureReason.UnauthorizedAccess.GetErrorMessage()}");
                    Debug.WriteLine($"[ERROR] {FileGetFailureReason.UnauthorizedAccess.GetErrorMessage()}");
                    Debug.WriteLine(ex);
                    throw;
                }
                catch (IOException ex)
                {
                    _message.ShowMessage($"{FileGetFailureReason.IO.GetErrorMessage()}");
                    Debug.WriteLine($"[ERROR] {FileGetFailureReason.IO.GetErrorMessage()}");
                    Debug.WriteLine(ex);
                    throw;
                }
                catch (Exception ex)
                {
                    _message.ShowMessage($"{FileGetFailureReason.Others.GetErrorMessage()}");
                    Debug.WriteLine($"[ERROR] {FileGetFailureReason.Others.GetErrorMessage()}");
                    Debug.WriteLine(ex);
                    throw;
                }
            }
            else
            {
                _message.ShowMessage($"{FileGetFailureReason.DirectryNotExists.GetErrorMessage()}");
            }
        }

        private OpenFileDialog CreateOpenFileDialog(bool isMult)
        {
            var dlg = new OpenFileDialog
            {
                Title = "ファイルを選択",
                Filter = AvailableExtension + PdfExtension + ExcelExtension + WordExtension + PowerPointExtension + Mp4Extension,
                Multiselect = isMult
            };
            return dlg;
        }

        /// <summary>
        /// ファイル選択ダイアログを表示する。（複数選択可能）
        /// </summary>
        /// <returns></returns>
        private string[] ShowMultSelectDialog()
        {
            var dlg = CreateOpenFileDialog(true);
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                // 選択されたPDFファイルが格納された配列を返す
                return dlg.FileNames;
            }
            else
            {
                // 空の配列を返す
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// ファイル選択ダイアログを表示する。（単一選択）
        /// </summary>
        /// <returns></returns>
        private string ShowSelectDialog()
        {
            var dlg = CreateOpenFileDialog(false);
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                // 選択されたPDFファイルが格納された配列を返す
                return dlg.FileName;
            }
            else
            {
                // 空の文字列を返す
                return string.Empty;
            }
        }

        [RelayCommand]
        private void Add()
        {
            if (string.IsNullOrEmpty(_direPath))
            {
                _message.ShowMessage($"ファイルを追加できません。");
                Debug.WriteLine($"[ERROR] 追加先のディレクトリパスが無効です");
                return;
            }

            _message.ShowMessage($"ローカルフォルダからファイルを選択してください。（複数選択可能）");
            // 選択されたファイルのパスを取得する
            var inputedPaths = ShowMultSelectDialog();
            if (inputedPaths.Length <= 0) { return; }

            // 追加成功ファイルリスト
            var successFiles = new List<FileData>();
            // エラー発生データ
            var errors = new FileAddErrors();

            foreach (var inputedPath in inputedPaths)
            {
                try
                {
                    // ファイルが追加可能（同名のファイルが存在しない）場合
                    if (_file.CanAddFile(_direPath, inputedPath))
                    {
                        var file = _file.AddFile(_direPath, inputedPath);
                        successFiles.Add(file);
                    }
                    // それ以外の場合
                    else
                    {
                        errors.Add(FileAddFailureReason.FileAlreadyExists, inputedPath);
                    }
                }
                catch (FileNotFoundException ex)
                {
                    errors.Add(FileAddFailureReason.FileNotFound, inputedPath);
                    Debug.WriteLine($"[ERROR] {FileAddFailureReason.FileNotFound.GetErrorMessage()}");
                    Debug.WriteLine($"[ERROR] inputedPath : {inputedPath}");
                    Debug.WriteLine(ex);
                }
                catch (DirectoryNotFoundException ex)
                {
                    errors.Add(FileAddFailureReason.DirectoryNotFound, inputedPath);
                    Debug.WriteLine($"[ERROR] {FileAddFailureReason.DirectoryNotFound.GetErrorMessage()}");
                    Debug.WriteLine($"[ERROR] inputedPath : {inputedPath}");
                    Debug.WriteLine(ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    errors.Add(FileAddFailureReason.UnauthorizedAccess, inputedPath);
                    Debug.WriteLine($"[ERROR] {FileAddFailureReason.UnauthorizedAccess.GetErrorMessage()}");
                    Debug.WriteLine($"[ERROR] inputedPath : {inputedPath}");
                    Debug.WriteLine(ex);
                }
                catch (PathTooLongException ex)
                {
                    errors.Add(FileAddFailureReason.PathTooLong, inputedPath);
                    Debug.WriteLine($"[ERROR] {FileAddFailureReason.PathTooLong.GetErrorMessage()}");
                    Debug.WriteLine($"[ERROR] inputedPath : {inputedPath}");
                    Debug.WriteLine(ex);
                }
                catch (IOException ex)
                {
                    errors.Add(FileAddFailureReason.IO, inputedPath);
                    Debug.WriteLine($"[ERROR] {FileAddFailureReason.IO.GetErrorMessage()}");
                    Debug.WriteLine($"[ERROR] inputedPath : {inputedPath}");
                    Debug.WriteLine(ex);
                }
                catch (Exception ex)
                {
                    errors.Add(FileAddFailureReason.Others, inputedPath);
                    Debug.WriteLine($"[ERROR] {FileAddFailureReason.Others.GetErrorMessage()}");
                    Debug.WriteLine($"[ERROR] inputedPath : {inputedPath}");
                    Debug.WriteLine(ex);
                }
            }

            // 追加成功ファイルの表示
            if (successFiles.Any())
            {
                var fileNames = new List<string>();

                foreach (var file in successFiles)
                {
                    // 一覧を更新
                    Files.Add(file);
                    // メッセージ表示用ファイル名を追加
                    fileNames.Add(file.FileName);
                }
                _message.ShowMessage("以下のファイルを追加しました。\n" + string.Join("\n", fileNames));
            }

            // エラー発生ファイルの表示
            if (errors.Errors.Any())
            {
                var messages = new List<string>();

                foreach(var error in errors.Errors)
                {
                    var errorMessage = error.Key.GetErrorMessage();
                    var fileNames = new List<string>();

                    foreach (var file in error.Value)
                    {
                        // メッセージ表示用ファイル名を追加
                        fileNames.Add(file.FileName);
                    }
                    messages.Add("・" + errorMessage + "\n" + string.Join("\n", fileNames));
                }
                _message.ShowMessage("以下のファイルの追加に失敗しました。\n" + string.Join("\n", messages));
            }
        }

        [RelayCommand]
        private void Overwrite()
        {
            if (Files == null || Files.Count <= 0) { return; }
            if (SelectedFile == null) { return; }

            _message.ShowMessage($"ローカルフォルダからファイルを選択してください。（単一選択）");
            // 選択されたファイルのパスを取得する
            var inputedPath = ShowSelectDialog();
            if (string.IsNullOrEmpty(inputedPath)) { return; }

            try
            {
                if (!File.Exists(SelectedFile.FilePath))
                {
                    Debug.WriteLine($"[ERROR] 差し替え対象のファイルが存在しません。");
                    Debug.WriteLine($"[ERROR] inputedPath : {inputedPath}");
                    _message.ShowMessage($"差し替え対象のファイルが存在しません。");
                    return;
                }
                _file.UpdateFile(inputedPath, SelectedFile.FilePath);
                _message.ShowMessage("以下のファイルの内容を差し替えしました。\n" + SelectedFile.FileName);
            }
            catch (FileNotFoundException ex)
            {
                Debug.WriteLine($"[ERROR] {FileAddFailureReason.FileNotFound.GetErrorMessage()}");
                Debug.WriteLine($"[ERROR] inputedPath : {inputedPath}");
                Debug.WriteLine(ex);
                _message.ShowMessage($"{FileAddFailureReason.FileNotFound.GetErrorMessage()}");
                return;
            }
            catch (DirectoryNotFoundException ex)
            {
                Debug.WriteLine($"[ERROR] {FileAddFailureReason.DirectoryNotFound.GetErrorMessage()}");
                Debug.WriteLine($"[ERROR] inputedPath : {inputedPath}");
                Debug.WriteLine(ex);
                _message.ShowMessage($"{FileAddFailureReason.DirectoryNotFound.GetErrorMessage()}");
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"[ERROR] {FileAddFailureReason.UnauthorizedAccess.GetErrorMessage()}");
                Debug.WriteLine($"[ERROR] inputedPath : {inputedPath}");
                Debug.WriteLine(ex);
                _message.ShowMessage($"{FileAddFailureReason.UnauthorizedAccess.GetErrorMessage()}");
                return;
            }
            catch (PathTooLongException ex)
            {
                Debug.WriteLine($"[ERROR] {FileAddFailureReason.PathTooLong.GetErrorMessage()}");
                Debug.WriteLine($"[ERROR] inputedPath : {inputedPath}");
                Debug.WriteLine(ex);
                _message.ShowMessage($"{FileAddFailureReason.PathTooLong.GetErrorMessage()}");
                return;
            }
            catch (IOException ex)
            {
                Debug.WriteLine($"[ERROR] {FileAddFailureReason.IO.GetErrorMessage()}");
                Debug.WriteLine($"[ERROR] inputedPath : {inputedPath}");
                Debug.WriteLine(ex);
                _message.ShowMessage($"{FileAddFailureReason.IO.GetErrorMessage()}");
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] {FileAddFailureReason.Others.GetErrorMessage()}");
                Debug.WriteLine($"[ERROR] inputedPath : {inputedPath}");
                Debug.WriteLine(ex);
                _message.ShowMessage($"{FileAddFailureReason.Others.GetErrorMessage()}");
                return;
            }
        }

        [RelayCommand]
        private void Delete()
        {
            if (Files == null || Files.Count <= 0) { return; }
            if (SelectedFile == null) { return; }

            var selectedFilePath = SelectedFile.FilePath;
            var selectedFileName = SelectedFile.FileName;

            if (!_path.CanDelete(selectedFilePath))
            {
                Debug.WriteLine($"[ERROR] {FileDeleteFailureReason.IsUsedInLink.GetErrorMessage()}");
                Debug.WriteLine($"[ERROR] filePath : {selectedFilePath}");
                _message.ShowMessage($"{FileDeleteFailureReason.IsUsedInLink.GetErrorMessage()}");
                return;
            }

            if (_message.ShowConfirm($"以下のファイルを削除します。よろしいですか？\n{selectedFileName}"))
            {
                try
                {
                    if (!File.Exists(selectedFilePath))
                    {
                        Debug.WriteLine($"[ERROR] {FileDeleteFailureReason.FileNotFound.GetErrorMessage()}");
                        Debug.WriteLine($"[ERROR] filePath : {selectedFilePath}");
                        _message.ShowMessage($"{FileDeleteFailureReason.FileNotFound.GetErrorMessage()}");
                        return;
                    }

                    _file.DeleteFile(SelectedFile);
                    _message.ShowMessage("以下のファイルを削除しました。\n" + selectedFileName);
                    // 一覧を更新
                    Files.Remove(SelectedFile);
                    SelectedFile = null;
                }
                catch (DirectoryNotFoundException ex)
                {
                    Debug.WriteLine($"[ERROR] {FileDeleteFailureReason.DirectoryNotFound.GetErrorMessage()}");
                    Debug.WriteLine($"[ERROR] filePath : {selectedFilePath}");
                    Debug.WriteLine(ex);
                    _message.ShowMessage($"{FileDeleteFailureReason.DirectoryNotFound.GetErrorMessage()}");
                    return;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Debug.WriteLine($"[ERROR] {FileDeleteFailureReason.UnauthorizedAccess.GetErrorMessage()}");
                    Debug.WriteLine($"[ERROR] filePath : {selectedFilePath}");
                    Debug.WriteLine(ex);
                    _message.ShowMessage($"{FileDeleteFailureReason.UnauthorizedAccess.GetErrorMessage()}");
                    return;
                }
                catch (PathTooLongException ex)
                {
                    Debug.WriteLine($"[ERROR] {FileDeleteFailureReason.PathTooLong.GetErrorMessage()}");
                    Debug.WriteLine($"[ERROR] filePath : {selectedFilePath}");
                    Debug.WriteLine(ex);
                    _message.ShowMessage($"{FileDeleteFailureReason.PathTooLong.GetErrorMessage()}");
                    return;
                }
                catch (IOException ex)
                {
                    Debug.WriteLine($"[ERROR] {FileDeleteFailureReason.IO.GetErrorMessage()}");
                    Debug.WriteLine($"[ERROR] filePath : {selectedFilePath}");
                    Debug.WriteLine(ex);
                    _message.ShowMessage($"{FileDeleteFailureReason.IO.GetErrorMessage()}");
                    return;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] {FileDeleteFailureReason.Others.GetErrorMessage()}");
                    Debug.WriteLine($"[ERROR] filePath : {selectedFilePath}");
                    Debug.WriteLine(ex);
                    _message.ShowMessage($"{FileDeleteFailureReason.Others.GetErrorMessage()}");
                    return;
                }
            }
        }

        /// <summary>
        /// 管理メニュー画面に遷移する。
        /// </summary>
        [RelayCommand]
        private void GoToAdminMenu()
        {
            var vm = new AdminMenuViewModel(_nav, _dia);
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
        /// 管理資料設定画面に留まる。（バインドエラー防止用）
        /// </summary>
        [RelayCommand]
        private void GoToDocumentMenuSettings()
        {
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
