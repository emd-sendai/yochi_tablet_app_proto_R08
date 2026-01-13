using System.Windows;
using WpfApp.Common.ViewModels;
using WpfApp.Common.Views;
using WpfApp.Interfaces;

namespace WpfApp.Services
{
    public class MessageDialogService : IMessageDialogService
    {
        public void ShowMessage(string message)
        {
            ShowDialog(message, showCancel: false);
        }

        public bool ShowConfirm(string message)
        {
            return ShowDialog(message, showCancel: true) == true;
        }

        private bool? ShowDialog(string message, bool showCancel)
        {
            var vm = new MessageDialogViewModel
            {
                Message = message,
                ShowCancel = showCancel
            };

            var dialog = new MessageDialogWindow
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };

            return dialog.ShowDialog();
        }
    }
}
