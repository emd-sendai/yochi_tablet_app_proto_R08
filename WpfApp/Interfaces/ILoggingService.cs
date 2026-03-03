namespace WpfApp.Interfaces
{
    /// <summary>
    /// ログを出力するためのインターフェース。
    /// </summary>
    public interface ILoggingService
    {
        void LogError(Exception ex);
    }
}
