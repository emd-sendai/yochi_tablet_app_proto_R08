namespace WpfApp.Interfaces
{
    /// <summary>
    /// ダイアログ画面からデータを取得するためのインターフェース。
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface IDialogResult<TResult>
    {
        event EventHandler? RequestClose;
        TResult? Result { get; }

    }
}
