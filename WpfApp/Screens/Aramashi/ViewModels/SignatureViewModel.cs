using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Windows.Controls;
using WpfApp.Interfaces;

namespace WpfApp.Screens.Aramashi.ViewModels
{
    public partial class SignatureViewModel : ObservableObject, IDialogResult<string>
    {
        public event EventHandler? RequestClose;
        public string? Result { get; private set; }

        [ObservableProperty]
        private string? title;

        [ObservableProperty]
        private string? signMessage;

        // UI依存の通知イベント
        public event EventHandler<string>? ShowMessageRequested;
        //public event EventHandler<string>? LoadRequested;
        public event EventHandler<InkCanvas>? SaveRequested;
        public event EventHandler<string>? PrintRequested;
        public event EventHandler<InkCanvas>? ClearRequested;

        public SignatureViewModel(string _title , string _signMessage)
        {
            Title = _title;
            SignMessage = _signMessage;
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

            PrintRequested?.Invoke(this, "");
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
        private void Close(string str)
        {
            Result = str;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}