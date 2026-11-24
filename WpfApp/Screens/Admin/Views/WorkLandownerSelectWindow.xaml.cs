using System.Windows;
using WpfApp.Screens.Admin.ViewModels;

namespace WpfApp.Screens.Admin.Views
{
    /// <summary>
    /// WorkLandownerSelectWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class WorkLandownerSelectWindow : Window
    {
        public WorkLandownerSelectWindow(WorkLandownerSelectViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;

            vm.ConfirmRequested += () =>
            {
                DialogResult = true;
            };

            vm.CancelRequested += () =>
            {
                DialogResult = false;
            };
        }
    }
}
