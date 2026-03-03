using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Windows.Controls;
using WpfApp.Interfaces;
using static WpfApp.Screens.Aramashi.ViewModels.PresentationViewerViewModel;

namespace WpfApp.Screens.Aramashi.ViewModels
{
    /// <summary>
    /// SignatureWindow.xamlに対応するViewModel。
    /// 
    /// </summary>
    public partial class SignatureViewModel : ObservableObject, IDialogResult<string>
    {
        public event EventHandler? RequestClose;
        public string? Result { get; private set; }

        /// <summary>
        /// 署名メッセージ
        /// </summary>
        [ObservableProperty]
        private string? signMessage;

        /// <summary>
        /// 表示中の項目のJSONファイルパス
        /// </summary>
        [ObservableProperty]
        private string? jsonPath;

        /// <summary>
        /// 保存用署名ファイルの名称を格納するディクショナリ
        /// </summary>
        [ObservableProperty]
        private Dictionary<string, string> signaturefileNames = new();

        // UI依存の通知イベント
        public event EventHandler<string>? ShowMessageRequested;
        public event EventHandler<InkCanvas>? SaveRequested;
        public event EventHandler<string>? PrintRequested;
        public event EventHandler<InkCanvas>? ClearRequested;

        public SignatureViewModel(string _signMessage, string _jsonPath, PresentationCategory presentationCategory)
        {
            SignMessage = _signMessage ?? "";
            JsonPath = _jsonPath ?? "";
            GenerateSignatureFileNames(presentationCategory);
        }

        /// <summary>
        /// 表示中の項目カテゴリに応じた保存用署名ファイルの名称を取得する。
        /// </summary>
        /// <param name="presentationName"></param>
        /// <exception cref="ArgumentException"></exception>
        private void GenerateSignatureFileNames(PresentationCategory presentationName)
        {
            string baseName = presentationName switch
            {
                PresentationCategory.Others001 => "signature_others001",
                PresentationCategory.Others002 => "signature_others002",
                _ => throw new ArgumentException($"Invalid presentationName: {presentationName}")
            };

            SignaturefileNames["isf"] = $"{baseName}.isf";
            SignaturefileNames["png"] = $"{baseName}.png";
        }

        /// <summary>
        /// 入力されたInkCanvasデータを署名データとして保存する。
        /// </summary>
        /// <param name="parameter"></param>
        [RelayCommand]
        private void Save(object? parameter)
        {
            if (parameter is not InkCanvas canvas)
            {
                Debug.WriteLine($"[DEBUG] InkCanvasではありません");
                return;
            }

            if (canvas.Strokes.Count == 0)
            {
                ShowMessageRequested?.Invoke(this, "署名がありません");
                Debug.WriteLine($"[DEBUG] 署名がありません");
                return;
            }

            SaveRequested?.Invoke(this, canvas);
        }

        /// <summary>
        /// 署名データと表示中の項目のJSONファイルパス（文面用）を印刷プレビュー画面に渡して表示する。
        /// </summary>
        [RelayCommand]
        private void Print()
        {
            if (JsonPath != null)
            {
                PrintRequested?.Invoke(this, JsonPath);
            }
            else
            {
                Debug.WriteLine($"[DEBUG] スライドデータのパスが見つかりません");
                return;
            }
        }

        /// <summary>
        /// 署名データを削除する。
        /// </summary>
        /// <param name="parameter"></param>
        [RelayCommand]
        private void Clear(object? parameter)
        {
            if (parameter is not InkCanvas canvas) 
            {
                Debug.WriteLine($"[DEBUG] InkCanvasではありません");
                return;
            }

            ClearRequested?.Invoke(this, canvas);
        }

        /// <summary>
        /// 画面を閉じる。
        /// </summary>
        [RelayCommand]
        private void Close()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 画面を閉じるリクエストを送る。
        /// </summary>
        public void OnRequestClose()
        {
            // イベントの null チェックとスレッドセーフ呼び出し
            var handler = RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// 画面を閉じる。（×ボタン用）
        /// </summary>
        public void CloseByUser()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}