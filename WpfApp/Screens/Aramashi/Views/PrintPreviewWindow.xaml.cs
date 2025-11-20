using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp.Screens.Aramashi.ViewModels;

namespace WpfApp.Screens.Aramashi.Views
{
    /// <summary>
    /// PrintPreviewWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PrintPreviewWindow : Window
    {
        public PrintPreviewWindow()
        {
            InitializeComponent();

            this.DataContextChanged += (s, e) =>
            {
                if (e.NewValue is PrintPreviewViewModel vm)
                {
                    vm.RequestClose += (_, _) => this.Close();

                    vm.PropertyChanged += Vm_PropertyChanged;
                }
            };
        }

        private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PresentationViewerViewModel.Scale))
            {
                // Scale が変更された時に何か処理したい場合
                // ここでUI更新やデバッグログなどを追加できる
                Debug.WriteLine($"Scale changed: {(sender as PresentationViewerViewModel)?.Scale}");
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (DataContext is PrintPreviewViewModel vm)
            {
                vm.CleanupTempFiles(); // 不要ファイル削除だけ実行
            }
            //this.Close();
        }

        private void Border_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this;
            e.Mode = ManipulationModes.Scale | ManipulationModes.Translate;
        }

        private void Border_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (DataContext is PresentationViewerViewModel vm)
            {
                vm.Zoom(e.DeltaManipulation.Scale.X);
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is PresentationViewerViewModel vm)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
                {
                    e.Handled = true;
                    double factor = e.Delta > 0 ? 1.1 : 1 / 1.1;
                    vm.Zoom(factor);
                }
            }
        }
    }
}
