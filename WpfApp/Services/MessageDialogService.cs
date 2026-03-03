using System.Windows;
using WpfApp.Common.ViewModels;
using WpfApp.Common.Views;
using WpfApp.Interfaces;

namespace WpfApp.Services
{
    /// <summary>
    /// メッセージダイアログを表示するサービスクラス。
    /// </summary>
    public class MessageDialogService : IMessageDialogService
    {
        /// <summary>
        /// メッセージダイアログを表示する。（OKボタン付き）
        /// </summary>
        /// <param name="message"></param>
        public void ShowMessage(string message)
        {
            ShowDialog(message, showCancel: false);
        }

        /// <summary>
        /// メッセージダイアログを表示する。（OKボタン、キャンセルボタン付き）
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool ShowConfirm(string message)
        {
            return ShowDialog(message, showCancel: true) == true;
        }

        /// <summary>
        /// テストメッセージダイアログを表示する。（OKボタン付き）
        /// </summary>
        public void ShowTestMessage()
        {
            ShowDialog("この機能は準備中です。", showCancel: false);
        }

        /// <summary>
        /// メッセージダイアログを表示する。
        /// </summary>
        /// <param name="message"></param>
        /// <param name="showCancel"></param>
        /// <returns></returns>
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
