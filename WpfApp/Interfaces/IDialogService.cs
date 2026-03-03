namespace WpfApp.Interfaces
{
    /// <summary>
    /// 画面を表示するためのインターフェース。
    /// </summary>
    public interface IDialogService
    {
        Task<TResult?> ShowDialogAsync<TViewModel, TResult>(params object[] parameters) where TViewModel : class;
        void ShowWindow<TViewModel>(params object[] parameters) where TViewModel : class;
    }
}
