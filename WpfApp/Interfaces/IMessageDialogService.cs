namespace WpfApp.Interfaces
{
    public interface IMessageDialogService
    {
        void ShowMessage(string message);

        bool ShowConfirm(string message);
    }
}
