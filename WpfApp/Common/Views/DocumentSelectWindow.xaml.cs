using System.Windows;
using WpfApp.Common.ViewModels;

namespace WpfApp.Common.Views
{
    /// <summary>
    /// DocumentSelectWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DocumentSelectWindow : Window
    {
        public DocumentSelectWindow()
        {
            InitializeComponent();
            Loaded += DocumentSelectWindow_Loaded;
        }

        private void DocumentSelectWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is DocumentSelectViewModel vm)
            {
                vm.RequestClose += (_, _) =>
                {
                    this.Close();
                };
            }
        }
    }
}
