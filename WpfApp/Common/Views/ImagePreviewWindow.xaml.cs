using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfApp.Common.ViewModels;

namespace WpfApp.Common.Views
{
    /// <summary>
    /// ImagePreviewWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ImagePreviewWindow : Window
    {
        public ImagePreviewWindow()
        {
            InitializeComponent();

            this.DataContextChanged += (s, e) =>
            {
                if (e.NewValue is ImagePreviewViewModel vm)
                {
                    vm.RequestClose += (_, _) => this.Close();
                }
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ImagePreviewViewModel vm) return;
            if (image.Source is not BitmapImage bmp) return;

            double viewWidth = scrollViewer.ActualWidth;
            double viewHeight = scrollViewer.ActualHeight;
            if (viewWidth <= 0 || viewHeight <= 0) return;

            Debug.WriteLine($"Image Size: {bmp.PixelWidth} x {bmp.PixelHeight}");
            Debug.WriteLine($"DPI: {bmp.DpiX}, {bmp.DpiY}");

            // 画像の論理サイズ（DPI を考慮）
            double logicalWidth = bmp.PixelWidth * 96.0 / bmp.DpiX;
            double logicalHeight = bmp.PixelHeight * 96.0 / bmp.DpiY;

            // フィット用 Scale 計算
            double scaleX = viewWidth / logicalWidth;
            double scaleY = viewHeight / logicalHeight;
            double fitScale = Math.Min(scaleX, scaleY);

            // Min/Max で制限
            fitScale = Math.Clamp(fitScale, vm.MinScale, vm.MaxScale);

            vm.Scale = fitScale;
        }



        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is ImagePreviewViewModel vm)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
                {
                    e.Handled = true;
                    double factor = e.Delta > 0 ? 1.1 : 1 / 1.1;
                    vm.Zoom(factor);
                }
            }
        }

        private void ScrollViewer_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (DataContext is ImagePreviewViewModel vm)
            {
                // 拡大縮小
                if (e.DeltaManipulation.Scale.X != 1.0)
                {
                    vm.Zoom(e.DeltaManipulation.Scale.X);
                }

                // 画像のパン（スクロール）も反映
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.DeltaManipulation.Translation.X);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.DeltaManipulation.Translation.Y);

                e.Handled = true;
            }
        }
    }
}
