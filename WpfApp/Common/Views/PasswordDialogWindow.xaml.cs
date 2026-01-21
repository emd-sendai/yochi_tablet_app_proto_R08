using System.Windows;
using WpfApp.Common.ViewModels;

namespace WpfApp.Common.Views
{
    /// <summary>
    /// PasswordDialogWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PasswordDialogWindow : Window
    {
        public PasswordDialogWindow()
        {
            InitializeComponent();

            Loaded += PasswordDialogWindow_Loaded;
        }

        private void PasswordDialogWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PasswordDialogViewModel vm)
            {
                vm.RequestClose += (s, args) =>
                {
                    DialogResult = true; // ShowDialog で true が返る
                    Close();
                };
            }

            // パスワードボックスにフォーカスを設定
            Dispatcher.BeginInvoke(new Action(() =>
            {
                PwBox.Focus();
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
