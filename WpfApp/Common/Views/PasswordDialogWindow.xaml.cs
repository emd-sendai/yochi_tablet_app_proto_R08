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
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
