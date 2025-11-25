using System.Windows;
using System.Windows.Input;
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
