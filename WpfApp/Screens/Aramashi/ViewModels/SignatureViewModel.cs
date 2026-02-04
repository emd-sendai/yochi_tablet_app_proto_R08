using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Windows.Controls;
using WpfApp.Interfaces;
using static WpfApp.Screens.Aramashi.ViewModels.PresentationViewerViewModel;

namespace WpfApp.Screens.Aramashi.ViewModels
{
    public partial class SignatureViewModel : ObservableObject, IDialogResult<string>
    {
        public event EventHandler? RequestClose;
        public string? Result { get; private set; }

        [ObservableProperty]
        private string? signMessage;

        [ObservableProperty]
        private string? jsonPath;

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

        [RelayCommand]
        private void Close()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        public void OnRequestClose()
        {
            // イベントの null チェックとスレッドセーフ呼び出し
            var handler = RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public void CloseByUser()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}