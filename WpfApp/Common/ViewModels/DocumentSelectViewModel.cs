using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using WpfApp.Common.Models;
using WpfApp.Services;

namespace WpfApp.Common.ViewModels
{
    /// <summary>
    /// DocumentSelectWindow.xamlに対応するViewModel。
    /// PDF資料選択画面での処理を行う。
    /// </summary>
    public partial class DocumentSelectViewModel : ObservableObject
    {
        private readonly PdfDocumentService _pdf;

        private readonly IReadOnlyList<PdfData>? _documents;

        /// <summary>
        /// 一覧表示用PDF資料リスト
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<PdfData>? documents;

        /// <summary>
        /// 選択されたPDF資料
        /// </summary>
        [ObservableProperty]
        private PdfData? selectedDocument;

        public event EventHandler? RequestClose;
        public event EventHandler<string>? ShowMessageRequested;

        public DocumentSelectViewModel(IReadOnlyList<PdfData> pdfList)
        {
            _pdf = new PdfDocumentService();
            _documents = pdfList;
            Documents = new ObservableCollection<PdfData>(_documents);
        }

        /// <summary>
        /// 選択されたPDF資料を開く。
        /// </summary>
        [RelayCommand]
        private void Select()
        {
            // 資料が選択されていない場合
            if (SelectedDocument == null)
            {
                Debug.WriteLine($"[ERROR] SelectedDocumentがNULLです");
                ShowMessageRequested?.Invoke(this, "ファイル名を選択してください");
            }
            else
            {
                // ファイルが存在しない場合
                if (!File.Exists(SelectedDocument.FullPath))
                {
                    Debug.WriteLine($"[ERROR] PDFファイルが存在しません {SelectedDocument.FullPath}");
                    ShowMessageRequested?.Invoke(this, "PDFファイルが存在しません");
                }
                else
                {
                    Debug.WriteLine($"[DEBUG] PDFファイルをデフォルトのビュアーで開きます {SelectedDocument.FullPath}");
                    _pdf.OpenPdf(SelectedDocument);
                }
            }
        }

        /// <summary>
        /// 選択状態を初期化して画面を閉じる。
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            Documents = null;
            SelectedDocument = null;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
