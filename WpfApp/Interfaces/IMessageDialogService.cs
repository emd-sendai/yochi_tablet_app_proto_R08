namespace WpfApp.Interfaces
{
    /// <summary>
    /// メッセージダイアログ画面を表示するためのインターフェース。
    /// </summary>
    public interface IMessageDialogService
    {
        void ShowMessage(string message);

        bool ShowConfirm(string message);

        void ShowTestMessage();
    }
}
