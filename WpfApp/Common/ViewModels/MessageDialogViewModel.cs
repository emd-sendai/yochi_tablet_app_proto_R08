using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WpfApp.Common.ViewModels
{
    public partial class MessageDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string message = "";

        [ObservableProperty]
        private bool showCancel = false;

        public event EventHandler<bool?>? RequestClose;

        [RelayCommand]
        private void Ok()
        {
            RequestClose?.Invoke(this, true);
        }

        [RelayCommand]
        private void Cancel()
        {
            RequestClose?.Invoke(this, false);
        }
    }
}
