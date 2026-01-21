using System.Windows;
using WpfApp.Common.Models;
using WpfApp.Common.ViewModels;

namespace WpfApp.Common.Views
{
    /// <summary>
    /// LandownerSelectWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class LandownerSelectWindow : Window
    {
        public LandownerSelectWindow()
        {
            InitializeComponent();
            var vm = new LandownerSelectViewModel();
            DataContext = vm;

            vm.RequestClose += Vm_RequestClose;
        }

        private void Vm_RequestClose(object? sender, bool e)
        {
            // ShowDialog() で開かれていれば DialogResult を設定できる
            this.DialogResult = e;
            this.Close();
        }

        public LandownerData? SelectedLandowner =>
            (DataContext as LandownerSelectViewModel)?.SelectedLandowner;
    }
}
