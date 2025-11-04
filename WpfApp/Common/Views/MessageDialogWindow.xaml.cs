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
using System.Windows.Shapes;
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
        }

        private void MessageDialogWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MessageDialogViewModel vm)
            {
                vm.RequestClose += (s, args) =>
                {
                    DialogResult = true; // ShowDialog で true を返す
                    Close();
                };
            }
        }
    }
}