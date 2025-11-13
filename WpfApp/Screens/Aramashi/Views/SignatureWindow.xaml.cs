using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApp.Common.ViewModels;
using WpfApp.Common.Views;
using WpfApp.Screens.Aramashi.ViewModels;

namespace WpfApp.Screens.Aramashi.Views
{
    /// <summary>
    /// SignatureWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SignatureWindow : Window
    {

        public SignatureWindow()
        {
            InitializeComponent();

            Loaded += SignatureWindow_Loaded;
            //var vm = new SignatureViewModel();
            //DataContext = vm;
        }

        private void SignatureWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not SignatureViewModel vm)
            {
                Debug.WriteLine("[ERROR] DataContext が ViewModel ではありません");
                return;
            }

            Debug.WriteLine("[DEBUG] SignatureWindow Loaded - DataContext 取得成功");

            vm.ShowMessageRequested += (_, msg) => ShowMessageWindow(msg);
            vm.SaveRequested += (_, canvas) => SaveCanvasAsPng(canvas);
            vm.PrintRequested += (_, canvas) => PrintCanvas(canvas);
            vm.ClearRequested += (_, canvas) => canvas.Strokes.Clear();
            vm.RequestClose += (_, _) => this.Close();
        }

        private void ShowMessageWindow(string msg)
        {
            //メッセージウィンドウを表示
            var vm = new MessageDialogViewModel
            {
                Message = msg
            };

            var dialog = new MessageDialogWindow
            {
                DataContext = vm,
                Owner = this
            };

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                // OK押下後の処理
            }
        }

        private void SaveCanvasAsPng(InkCanvas canvas)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PNGファイル (*.png)|*.png",
                FileName = "signature.png"
            };

            if (dlg.ShowDialog() != true) return;

            var size = new Size(canvas.ActualWidth, canvas.ActualHeight);
            canvas.Measure(size);
            canvas.Arrange(new Rect(size));

            var rtb = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96d, 96d, PixelFormats.Default);
            rtb.Render(canvas);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using var fs = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write);
            encoder.Save(fs);

            ShowMessageWindow("署名をPNG画像として保存しました");
        }

        private void PrintCanvas(InkCanvas canvas)
        {
            var pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                pd.PrintVisual(canvas, "署名の印刷");
            }
        }
    }
}
