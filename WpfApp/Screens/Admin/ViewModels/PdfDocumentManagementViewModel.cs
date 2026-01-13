using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
                _messageDialog.ShowMessage($"表示できるPDFファイルがありません");
                Debug.WriteLine($"[ERROR] DocumentsがNULLもしくは空です");
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

        }

        [RelayCommand]
        private void Update()
        {
            if (SelectedDocument == null)
            {
                _messageDialog.ShowMessage($"PDFファイルを選択してください");
                Debug.WriteLine($"[ERROR] SelectedDocumentがNULLです");
            }


        }

        [RelayCommand]
        private void Delete()
        {
            if(SelectedDocument == null)
            {
                _messageDialog.ShowMessage($"PDFファイルを選択してください");
                Debug.WriteLine($"[ERROR] SelectedDocumentがNULLです");
                return;
            }

            if (_messageDialog.ShowConfirm($"以下を削除します。よろしいですか？\n「{SelectedDocument.FullFileName}」"))
            {

                // 削除処理

                _messageDialog.ShowMessage($"「{SelectedDocument.FullFileName}」を削除しました。");
            }
        }
    }
}
