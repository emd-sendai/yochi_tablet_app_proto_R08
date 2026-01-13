using System.ComponentModel;
using System.Windows;
using WpfApp.Common.ViewModels;

namespace WpfApp.Common.Views
{
    /// <summary>
    /// MessageDialogWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MessageDialogWindow : Window
    {
        public MessageDialogWindow()
        {
            InitializeComponent();
            Loaded += MessageDialogWindow_Loaded;
            Closing += MessageDialogWindow_Closing;
        }

        private void MessageDialogWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MessageDialogViewModel vm)
            {
                vm.RequestClose += (s, result) =>
                {
                    DialogResult = result;
                    Close();
                };
            }
        }

        private void MessageDialogWindow_Closing(object? sender, CancelEventArgs e)
        {
            // DialogResult が未設定の場合
            if (DialogResult == null)
            {
                DialogResult = false; // キャンセル扱い
            }
        }
    }
}