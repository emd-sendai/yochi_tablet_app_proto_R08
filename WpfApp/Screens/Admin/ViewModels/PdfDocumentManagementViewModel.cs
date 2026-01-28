using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using WpfApp.Common.Models;
using WpfApp.Interfaces;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using static WpfApp.Screens.Admin.Models.TabItemData;
using static WpfApp.Screens.Admin.ViewModels.ScreenSelectViewModel;

namespace WpfApp.Screens.Admin.ViewModels
{
    /// <summary>
    /// PdfDocumentManagementView.xamlに対応するViewModel。
    /// PDF資料管理画面での処理（アプリ内のPDFファイルの追加、上書き、削除）を行う。
    /// </summary>
    public partial class PdfDocumentManagementViewModel : ObservableObject , INavigationTarget
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly PdfDocumentService _pdf;
        private readonly ILoggingService _log;
        private readonly IMessageDialogService _messageDialog;
        private readonly IAppPathService _path;

        public Func<object>? CreateBackViewModel => () => new PdfDocumentSettingsViewModel(_nav, _dia, _selectedScreen);

        [ObservableProperty]
        private bool isLeftDrawerOpen;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string subTitle;

        private static readonly string _subTitleItem = "PDFファイル一覧";

        private readonly ScreenCategory _selectedScreen;

        private readonly PdfDocumentData _pdfDataFolder;

        [ObservableProperty]
        private ObservableCollection<PdfData>? documents;

        [ObservableProperty]
        private PdfData? selectedDocument;

        private enum FailReason
        {
            FileAlreadyExists,      // 既存ファイルと重複
            FileLockedOrReadOnly,   // ファイルが使用中または読み取り専用
            NoAccess,               // アクセス権の不足
            Other                   // その他
        }

        private static readonly Dictionary<FailReason, string> FailReasonMessages = new()
        {
            { FailReason.FileAlreadyExists, "・既に同名のファイルが存在します" },
            { FailReason.FileLockedOrReadOnly, "・ファイルが使用中または読み取り専用です" },
            { FailReason.NoAccess, "・アクセス権が不足しています" },
            { FailReason.Other, "・その他（管理者へお問い合わせください）" }
        };

        public PdfDocumentManagementViewModel(NavigationService navigationService, IDialogService dialogService, string prevTitle, 
            PdfDocumentData selectedPdfData , ScreenCategory selectedScreen)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _pdf = new PdfDocumentService();
            _log = new LoggingService();
            _messageDialog = new MessageDialogService();
            _path = new AppPathService();

            _pdfDataFolder = selectedPdfData;

            Title = prevTitle;
            SubTitle = GetSubTitle();
            _selectedScreen = selectedScreen;
        }

        /// <summary>
        /// View読み込み時にPDFファイル一覧を取得する。
        /// </summary>
        public void OnViewLoaded()
        {
            if (_pdfDataFolder.FolderPath != null)
            {
                Documents = _pdf.GetPdfDatas(_pdfDataFolder.FolderPath);
            }
        }

        partial void OnDocumentsChanged(ObservableCollection<PdfData>? oldValue, ObservableCollection<PdfData>? newValue)
        {
            if(Documents == null || Documents.Count <= 0)
            {
                _messageDialog.ShowMessage($"表示できるPDFファイルがありません。");
                Debug.WriteLine($"[ERROR] 'Documents'がNULLもしくは空です");
            }
        }

        /// <summary>
        /// Viewに表示するサブタイトルを取得する。
        /// </summary>
        /// <returns></returns>
        private string GetSubTitle()
        {
            // サブタイトルに初期値を設定
            var subTitle = _subTitleItem;

            if (_pdfDataFolder.DisplayName != null)
            {
                subTitle = "【" + _pdfDataFolder.DisplayName + "】" + _subTitleItem;
            }
            return subTitle;
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
        /// 選択されたPDFファイルを追加する。（複数選択可能）
        /// </summary>
        [RelayCommand]
        private void Add()
        {
            if (_pdfDataFolder.FolderPath == null)
            {
                _messageDialog.ShowMessage("PDFファイルを新規追加できません。");
                Debug.WriteLine($"[ERROR] '_pdfDataFolder.FolderPath'がNULLです");
                return;
            }

            // 説明用メッセージダイアログを表示
            _messageDialog.ShowMessage("ローカルフォルダから新規追加するPDFファイルを選択してください。（複数選択可能）");
            // ファイル選択ダイアログを表示
            var selectedFiles = ShowMultSelectPdfDialog();

            if(selectedFiles.Length <= 0) { return; }

            // 追加成功データ格納用
            var successFiles = new List<string>();
            // 追加失敗データ格納用
            var failedFilesByReason = new Dictionary<FailReason, List<string>>();

            void AddFailed(string fileName, FailReason reason)
            {
                if (!failedFilesByReason.ContainsKey(reason))
                    failedFilesByReason[reason] = new List<string>();

                failedFilesByReason[reason].Add(fileName);
            }

            foreach (var filePath in selectedFiles)
            {
                var fileName = Path.GetFileName(filePath);

                try
                {
                    var pdfData = _pdf.AddPdf(_pdfDataFolder.FolderPath, filePath);
                    successFiles.Add(fileName);

                    // 一覧表示を更新
                    if (Documents == null || Documents.Count <= 0)
                    {
                        Documents = new ObservableCollection<PdfData> { pdfData };
                    }
                    else
                    {
                        Documents.Add(pdfData);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Debug.WriteLine($"[ERROR] {ex}");
                    AddFailed(fileName, FailReason.FileAlreadyExists);
                }
                catch (IOException ex)
                {
                    Debug.WriteLine($"[ERROR] 使用中か読み取り専用のためファイルを追加できません");
                    Debug.WriteLine($"[ERROR] {ex}");
                    AddFailed(fileName, FailReason.FileLockedOrReadOnly);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Debug.WriteLine($"[ERROR] 権限が不足しているためファイルを追加できません");
                    Debug.WriteLine($"[ERROR] {ex}");
                    AddFailed(fileName, FailReason.NoAccess);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] ファイルを追加できません");
                    Debug.WriteLine($"[ERROR] {ex}");
                    _log.LogError(ex);
                    AddFailed(fileName, FailReason.Other);
                }
            }

            // 追加成功データの表示
            if (successFiles.Any())
            {
                _messageDialog.ShowMessage("以下を追加しました。\n" + string.Join("\n", successFiles));
            }

            // 追加失敗データの表示
            if (failedFilesByReason.Any())
            {
                var lines = new List<string>();

                foreach (var reasonGroup in failedFilesByReason)
                {
                    lines.Add(FailReasonMessages[reasonGroup.Key]);
                    lines.AddRange(reasonGroup.Value.Select(f => "  " + f));
                }

                _messageDialog.ShowMessage("以下を追加できませんでした。\n" + string.Join("\n", lines));
            }
        }

        /// <summary>
        /// PDF選択ダイアログを表示する。（複数選択可能）
        /// </summary>
        /// <returns></returns>
        private string[] ShowMultSelectPdfDialog()
        {
            var dlg = new OpenFileDialog
            {
                Title = "PDF ファイルを選択",
                Filter = "PDF ファイル (*.pdf)|*.pdf",
                Multiselect = true
            };

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
        /// 選択されたファイルの内容で既存のPDFファイルの内容を上書きする。（単一選択）
        /// </summary>
        [RelayCommand]
        private void Update()
        {
            if (SelectedDocument == null)
            {
                _messageDialog.ShowMessage($"PDFファイルを選択してください。");
                Debug.WriteLine($"[ERROR] 'SelectedDocument'がNULLです");
                return;
            }

            // 説明用メッセージダイアログを表示
            _messageDialog.ShowMessage($"ローカルフォルダからPDFファイルを選択してください。（単一選択）");
            // ファイル選択ダイアログを表示
            var selectedFile = ShowSelectPdfDialog();

            if(selectedFile == null || selectedFile == string.Empty) { return; }

            try
            {
                _pdf.UpdatePdf(SelectedDocument.FullPath, selectedFile);
                _messageDialog.ShowMessage($"以下の内容を上書きしました。\n「{SelectedDocument.FullFileName}」");
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ERROR] {ex}");
                _messageDialog.ShowMessage($"対象のファイルが存在しないため、上書きできません。");
            }
            catch (IOException ex)
            {
                Debug.WriteLine($"[ERROR] 使用中か読み取り専用のためファイルを上書きできません");
                Debug.WriteLine($"[ERROR] {ex}");
                _messageDialog.ShowConfirm("ファイルが開かれているため、上書きできません。" +
                    "\nファイルを閉じてから、もう一度お試しください。");
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"[ERROR] アクセス権が不足しているためファイルを上書きできません");
                Debug.WriteLine($"[ERROR] {ex}");
                _messageDialog.ShowConfirm("アクセス権が不足しているため、ファイルを上書きできません。");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] ファイルを上書きできません");
                Debug.WriteLine($"[ERROR] {ex}");
                _log.LogError(ex);
                _messageDialog.ShowConfirm("ファイルを上書きできません。\n管理者へお問い合わせください。");
            }
        }

        /// <summary>
        /// PDF選択ダイアログを表示する。（単一選択）
        /// </summary>
        /// <returns></returns>
        private string ShowSelectPdfDialog()
        {
            var dlg = new OpenFileDialog
            {
                Title = "PDF ファイルを選択",
                Filter = "PDF ファイル (*.pdf)|*.pdf",
                Multiselect = false
            };

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                // 選択されたPDFファイルを返す
                return dlg.FileName;
            }
            else
            {
                // 空の文字列を返す
                return string.Empty;
            }
        }

        /// <summary>
        /// 既存のPDFファイルを削除する。
        /// </summary>
        [RelayCommand]
        private void Delete()
        {
            if(Documents == null)
            {
                _messageDialog.ShowMessage($"PDFファイルを削除できません。");
                Debug.WriteLine($"[ERROR] 'Documents'がNULLです");
                return;
            }

            if(SelectedDocument == null)
            {
                _messageDialog.ShowMessage($"PDFファイルを選択してください。");
                Debug.WriteLine($"[ERROR] 'SelectedDocument'がNULLです");
                return;
            }

            if (!_path.CanDelete(SelectedDocument.FullPath))
            {
                _messageDialog.ShowMessage($"このファイルはリンクテキストからの表示に使用されているため削除できません。");
                Debug.WriteLine($"[ERROR] リンクテキストからの直接表示に使用されているため削除できません: {SelectedDocument.FullPath}");
                return;
            }

            if (_messageDialog.ShowConfirm($"以下を削除します。よろしいですか？\n「{SelectedDocument.FullFileName}」"))
            {
                try
                {
                    _pdf.DeletePdf(SelectedDocument);
                    _messageDialog.ShowMessage($"以下を削除しました。\n「{SelectedDocument.FullFileName}」");
                    // 一覧表示の更新
                    Documents.Remove(SelectedDocument);
                    // 選択状態の初期化
                    SelectedDocument = null;
                }
                catch (InvalidOperationException ex)
                {
                    Debug.WriteLine($"[ERROR] {ex}");
                    _messageDialog.ShowMessage($"対象のファイルが存在しないため、削除できません。");
                }
                catch (IOException ex)
                {
                    Debug.WriteLine($"[ERROR] 使用中か読み取り専用のためファイルを削除できません");
                    Debug.WriteLine($"[ERROR] {ex}");
                    _messageDialog.ShowConfirm("ファイルが開かれているため、削除できません。" +
                        "\nファイルを閉じてから、もう一度お試しください。");
                }
                catch (UnauthorizedAccessException ex)
                {
                    Debug.WriteLine($"[ERROR] アクセス権が不足しているためファイルを削除できません");
                    Debug.WriteLine($"[ERROR] {ex}");
                    _messageDialog.ShowConfirm("アクセス権が不足しているため、ファイルを削除できません。");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] ファイルを削除できません");
                    Debug.WriteLine($"[ERROR] {ex}");
                    _log.LogError(ex);
                    _messageDialog.ShowConfirm("ファイルを削除できません。\n管理者へお問い合わせください。");
                }

            }


        }
    }
}
