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

namespace WpfApp.Screens.Admin.ViewModels
{
    public partial class PdfDocumentManagementViewModel : ObservableObject
    {
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly PdfDocumentService _pdf;
        private readonly IMessageDialogService _messageDialog;

        [ObservableProperty]
        private bool isLeftDrawerOpen;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string subTitle;

        private static readonly string _subTitleItem = "PDFファイル一覧";

        private readonly PdfDocumentData _pdfDataFolder;

        [ObservableProperty]
        private ObservableCollection<PdfData>? documents;

        [ObservableProperty]
        private PdfData? selectedDocument;

        public PdfDocumentManagementViewModel(NavigationService navigationService, IDialogService dialogService, string prevTitle, PdfDocumentData selectedPdfData)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _messageDialog = new MessageDialogService();

            _pdf = new PdfDocumentService();
            _pdfDataFolder = selectedPdfData;

            Title = prevTitle;
            SubTitle = GetSubTitle();
        }

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
            var failedFiles = new List<string>();

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
                catch (InvalidOperationException)
                {
                    failedFiles.Add(fileName);
                }
            }

            // 追加成功データの表示
            if (successFiles.Any())
            {
                _messageDialog.ShowMessage("以下を追加しました。\n" + string.Join("\n", successFiles));
            }

            // 追加失敗データの表示
            if (failedFiles.Any())
            {
                _messageDialog.ShowMessage("以下は追加できませんでした。\n" + string.Join("\n", failedFiles));
            }
        }

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
            catch (InvalidOperationException)
            {
                _messageDialog.ShowMessage($"以下の内容を上書きできませんでした。\n「{SelectedDocument.FullFileName}」");
            }
        }

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

            if (_messageDialog.ShowConfirm($"以下を削除します。よろしいですか？\n「{SelectedDocument.FullFileName}」"))
            {
                // 削除処理
                if (_pdf.DeletePdf(SelectedDocument))
                {
                    _messageDialog.ShowMessage($"以下を削除しました。\n「{SelectedDocument.FullFileName}」");
                    
                    // 一覧表示の更新
                    Documents.Remove(SelectedDocument);
                    // 選択状態の初期化
                    SelectedDocument = null;
                }
                else
                {
                    _messageDialog.ShowMessage($"以下を削除できませんでした。\n「{SelectedDocument.FullFileName}」");
                }
            }
        }
    }
}
