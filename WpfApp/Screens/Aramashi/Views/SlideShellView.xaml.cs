using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp.Screens.Aramashi.ViewModels;

namespace WpfApp.Screens.Aramashi.Views
{
    /// <summary>
    /// SlideShellView.xaml の相互作用ロジック
    /// </summary>
    public partial class SlideShellView : UserControl
    {
        public SlideShellView()
        {
            InitializeComponent();
        }

        private void Border_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this;
            e.Mode = ManipulationModes.Scale | ManipulationModes.Translate;
        }

        private void Border_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (DataContext is SlideShellViewModel vm)
            {
                vm.Zoom(e.DeltaManipulation.Scale.X);
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is SlideShellViewModel vm)
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
