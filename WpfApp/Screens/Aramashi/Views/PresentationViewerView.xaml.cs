using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp.Screens.Aramashi.ViewModels;

namespace WpfApp.Screens.Aramashi.Views
{
    /// <summary>
    /// PresentationViewerView.xaml の相互作用ロジック
    /// </summary>
    public partial class PresentationViewerView : UserControl
    {
        public PresentationViewerView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) =>
            {
                if (e.NewValue is PresentationViewerViewModel vm)
                {
                    vm.PropertyChanged += ViewModel_PropertyChanged;
                    if (vm.CurrentSlide != null)
                        PageControl.ApplyPage(vm.CurrentSlide, vm);
                }
            };
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is PresentationViewerViewModel vm &&
                e.PropertyName == nameof(vm.CurrentSlide))
            {
                if (vm.CurrentSlide != null)
                    PageControl.ApplyPage(vm.CurrentSlide, vm);
            }
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
