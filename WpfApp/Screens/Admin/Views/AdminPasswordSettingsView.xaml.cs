using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp.Screens.Admin.ViewModels;

namespace WpfApp.Screens.Admin.Views
{
    /// <summary>
    /// AdminPasswordSettingsView.xaml の相互作用ロジック
    /// </summary>
    public partial class AdminPasswordSettingsView : UserControl
    {
        public AdminPasswordSettingsView()
        {
            InitializeComponent();

            Loaded += View_Loaded;

            Loaded += (_, _) =>
            {
                if (DataContext is AdminPasswordSettingsViewModel vm)
                {
                    vm.ClearCurrentPassRequested += () =>
                    {
                        ClearCurrentPass();
                    };

                    vm.ClearAllPassRequested += () =>
                    {
                        ClearAllPass();
                    };

                    vm.FocusConfirmPassRequested += () =>
                    {
                        ConfirmPwBox.Focus();
                    };

                    vm.FocusNewPassRequested += () =>
                    {
                        NewPwBox.Focus();
                    };
                }
            };
        }

        private void PasswordBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Z0-9]+$");
        }

        private void PasswordBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(DataFormats.Text))
            {
                e.CancelCommand();
                return;
            }

            string text = e.DataObject.GetData(DataFormats.Text) as string ?? "";

            if (!Regex.IsMatch(text, @"^[a-zA-Z0-9]+$"))
            {
                e.CancelCommand();
            }
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AdminPasswordSettingsViewModel vm)
            {
                string currentPass = CurrentPwBox.Password;
                string newPass = NewPwBox.Password;
                string confirmPass = ConfirmPwBox.Password;

                vm.ChangePassword(currentPass, newPass, confirmPass);
            }
        }

        private void ClearCurrentPass()
        {
            CurrentPwBox.Clear();
            CurrentPwBox.Focus();
        }

        private void ClearAllPass()
        {
            CurrentPwBox.Clear();
            NewPwBox.Clear();
            ConfirmPwBox.Clear();
            CurrentPwBox.Focus();
        }

        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentPwBox.Focus();
        }
    }
}
