using System.Windows;
using WpfApp.Screens.Admin.ViewModels;

namespace WpfApp.Screens.Admin.Views
{
    /// <summary>
    /// WorkEditWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class WorkEditWindow : Window
    {
        public WorkEditWindow(WorkEditViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;

            Loaded += Window_Loaded;

            vm.FocusWorkNameRequested += () =>
            {
                WorkNameTextBox.Focus();
            };

            vm.ConfirmRequested += () =>
            {
                DialogResult = true;
            };

            vm.CancelRequested += () =>
            {
                DialogResult = false;
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WorkNameTextBox.Focus();
        }
    }
}
