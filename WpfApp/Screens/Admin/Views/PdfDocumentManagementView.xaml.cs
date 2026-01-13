using System.Windows;
using System.Windows.Controls;
using WpfApp.Screens.Admin.ViewModels;

namespace WpfApp.Screens.Admin.Views
{
    /// <summary>
    /// PdfDocumentManagementView.xaml の相互作用ロジック
    /// </summary>
    public partial class PdfDocumentManagementView : UserControl
    {
        public PdfDocumentManagementView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if(DataContext is PdfDocumentManagementViewModel vm)
            {
                vm.OnViewLoaded();
            }
        }
    }
}
