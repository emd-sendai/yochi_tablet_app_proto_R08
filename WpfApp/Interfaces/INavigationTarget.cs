namespace WpfApp.Interfaces
{
    /// <summary>
    /// 遷移先の画面を指定するためのインターフェース。
    /// </summary>
    public interface INavigationTarget
    {
        Func<object>? CreateBackViewModel { get; }
    }
}
