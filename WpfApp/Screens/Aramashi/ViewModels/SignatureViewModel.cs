using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApp.Common.ViewModels;
using WpfApp.Common.Views;
using WpfApp.Interfaces;

namespace WpfApp.Screens.Aramashi.ViewModels
{
    public partial class SignatureViewModel : ObservableObject, IDialogResult<string>
    {
        public event EventHandler? RequestClose;
        public string? Result { get; private set; }

        // UI依存の通知イベント
        public event EventHandler<string>? ShowMessageRequested;
        public event EventHandler<InkCanvas>? SaveRequested;
        public event EventHandler<InkCanvas>? PrintRequested;
        public event EventHandler<InkCanvas>? ClearRequested;

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
        private void Print(object? parameter)
        {
            if (parameter is not InkCanvas canvas)
                return;

            if (canvas.Strokes.Count == 0)
            {
                ShowMessageRequested?.Invoke(this, "署名がありません");
                return;
            }

            PrintRequested?.Invoke(this, canvas);
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