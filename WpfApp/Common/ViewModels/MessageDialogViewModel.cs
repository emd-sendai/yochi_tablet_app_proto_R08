using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WpfApp.Common.ViewModels
{
    /// <summary>
    /// MessageDialogWindow.xamlに対応するViewModel。
    /// メッセージダイアログ画面での処理を行う。
    /// </summary>
    public partial class MessageDialogViewModel : ObservableObject
    {
        /// <summary>
        /// 表示するメッセージ
        /// </summary>
        [ObservableProperty]
        private string message = "";

        /// <summary>
        /// キャンセルボタンの表示/非表示
        /// </summary>
        [ObservableProperty]
        private bool showCancel = false;

        public event EventHandler<bool?>? RequestClose;

        /// <summary>
        /// trueを返して画面を閉じる。
        /// </summary>
        [RelayCommand]
        private void Ok()
        {
            RequestClose?.Invoke(this, true);
        }

        /// <summary>
        /// falseを返して画面を閉じる。
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            RequestClose?.Invoke(this, false);
        }
    }
}
