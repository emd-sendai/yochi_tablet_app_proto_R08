using System.Windows;
using WpfApp.Screens.Admin.ViewModels;

namespace WpfApp.Screens.Admin.Views
{
    /// <summary>
    /// LandownerEditWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class LandownerEditWindow : Window
    {
        public LandownerEditWindow(LandownerEditViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;

            Loaded += Window_Loaded;

            vm.FocusLandownerKanaRequested += () =>
            {
                LandownerKanaTextBox.Focus();
            };

            vm.FocusLandownerNameRequested += () =>
            {
                LandownerNameTextBox.Focus();
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
            LandownerKanaTextBox.Focus();
        }
    }
}
