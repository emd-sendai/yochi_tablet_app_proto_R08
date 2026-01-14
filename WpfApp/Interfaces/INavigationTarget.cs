namespace WpfApp.Interfaces
{
    public interface INavigationTarget
    {
        Func<object>? CreateBackViewModel { get; }
    }
}
