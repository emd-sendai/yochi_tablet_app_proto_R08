using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using WpfApp.Common.Models;
using WpfApp.Common.Views;
using WpfApp.Services;

namespace WpfApp.Common.ViewModels
{
    public partial class DocumentSelectViewModel : ObservableObject
    {
        private readonly PdfDocumentService _pdf;

        private readonly IReadOnlyList<PdfData>? _documents;

        [ObservableProperty]
        private ObservableCollection<PdfData>? documents;

        [ObservableProperty]
        private PdfData? selectedDocument;

        public event EventHandler? RequestClose;

        public DocumentSelectViewModel(IReadOnlyList<PdfData> pdfList)
        {
            _pdf = new PdfDocumentService();
            _documents = pdfList;
            Documents = new ObservableCollection<PdfData>(_documents);
        }

        [RelayCommand]
        private void Select()
        {
            // 資料が選択されていない場合
            if (SelectedDocument == null)
            {
                Debug.WriteLine($"[ERROR] SelectedDocumentがNULLです");
                //メッセージウィンドウを表示
                var vm = new MessageDialogViewModel
                {
                    Message = "ファイル名を選択してください"
                };

                var dialog = new MessageDialogWindow
                {
                    DataContext = vm,
                    Owner = Application.Current.MainWindow
                };

                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    // OK押下後の処理
                }
            }
            else
            {
                // ファイルが存在しない場合
                if (!File.Exists(SelectedDocument.FullPath))
                {
                    Debug.WriteLine($"[ERROR] PDFファイルが存在しません {SelectedDocument.FullPath}");
                    MessageBox.Show("ファイルデータが存在しません");
                }
                else
                {
                    Debug.WriteLine($"[DEBUG] PDFファイルをデフォルトのビュアーで開きます {SelectedDocument.FullPath}");
                    _pdf.OpenPdf(SelectedDocument);
                }
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            Documents = null;
            SelectedDocument = null;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
